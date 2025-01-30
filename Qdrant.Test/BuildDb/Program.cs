using OpenAI;
using Qdrant.Client;
using System.CommandLine;
using NAudio.Wave;
using Whisper.net;
using System.Text.Json;
using System.Text.Json.Serialization;

public class SearchResult
{
    public string Answer { get; set; }
    public List<AudioFragment> Fragments { get; set; } = new();
}

public class AudioFragment
{
    public string Filename { get; set; }
    public float Start { get; set; }
    public float End { get; set; }
    public string Text { get; set; }
}

public class SearchPoint
{
    public string Id { get; set; }
    public JsonElement Payload { get; set; }
    public float Score { get; set; }
    public float[] Vector { get; set; }
}

class Program
{
    static async Task<int> Main(string[] args)
    {
        var rootCommand = new RootCommand("Process audio files and query the database");
        var modeOption = new Option<int>(
            "--mode",
            "1: Create collection and process files, 2: Query the database"
        );
        rootCommand.AddOption(modeOption);

        rootCommand.SetHandler(async (mode) =>
        {
            await ProcessMode(mode);
        }, modeOption);

        return await rootCommand.InvokeAsync(args);
    }

    static async Task ProcessMode(int mode)
    {
        // Initialize OpenAI
        var openAiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        if (string.IsNullOrEmpty(openAiKey))
        {
            throw new Exception("Please set OPENAI_API_KEY environment variable");
        }
        var openAiClient = new OpenAIClient(openAiKey);

        // Initialize Qdrant client
        Console.WriteLine("Connecting to Qdrant");
        var qdrantClient = new QdrantClient("localhost", 6333);
        
        string collectionName = "audio_collection";

        if (mode == 1)
        {
            await CreateCollectionAndProcessFiles(qdrantClient, openAiClient, collectionName);
        }
        else if (mode == 2)
        {
            await QueryDatabase(qdrantClient, openAiClient, collectionName);
        }
    }

    static async Task CreateCollectionAndProcessFiles(QdrantClient qdrantClient, OpenAIClient openAiClient, string collectionName)
    {
        Console.WriteLine("Mode 1: Creating collection and processing files");

        // Create collection if it doesn't exist
        try
        {
            await qdrantClient.GetCollectionInfoAsync(collectionName);
        }
        catch
        {
            await qdrantClient.CreateCollectionAsync(
                collectionName,
                new VectorParams { Size = 1536, Distance = Distance.Cosine }
            );
        }

        // Load Whisper model
        Console.WriteLine("Loading Whisper model...");
        using var whisperModel = WhisperFactory.LoadModel("base");

        Console.WriteLine("Loading mp3 files");
        var mp3Files = Directory.GetFiles("mp3_files", "*.mp3");
        foreach (var file in mp3Files)
        {
            await ProcessFile(file, whisperModel, qdrantClient, openAiClient, collectionName);
        }
    }

    static async Task ProcessFile(string filePath, WhisperModel whisperModel, QdrantClient qdrantClient, OpenAIClient openAiClient, string collectionName)
    {
        Console.WriteLine($"Processing {Path.GetFileName(filePath)}");

        // Convert MP3 to WAV temporarily
        var tempWavPath = Path.GetTempFileName() + ".wav";
        try
        {
            using (var reader = new Mp3FileReader(filePath))
            using (var writer = new WaveFileWriter(tempWavPath, reader.WaveFormat))
            {
                reader.CopyTo(writer);
            }

            // Transcribe audio using Whisper
            var result = await whisperModel.TranscribeAsync(tempWavPath);
            var transcript = result.Text;
            var segments = result.Segments.Select(s => new
            {
                text = s.Text,
                start = s.Start,
                end = s.End
            }).ToList();

            Console.WriteLine("Getting OpenAI embedding");
            var embedding = await GetEmbedding(openAiClient, transcript);

            Console.WriteLine("Uploading to Qdrant");
            await qdrantClient.UpsertAsync(
                collectionName,
                new[]
                {
                    new Point
                    {
                        Id = Guid.NewGuid().ToString(),
                        Payload = new Dictionary<string, object>
                        {
                            { "filename", Path.GetFileName(filePath) },
                            { "file_path", filePath },
                            { "transcript", transcript },
                            { "segments", segments }
                        },
                        Vector = embedding
                    }
                }
            );
            Console.WriteLine($"Uploaded {Path.GetFileName(filePath)} metadata to database");
        }
        finally
        {
            if (File.Exists(tempWavPath))
            {
                File.Delete(tempWavPath);
            }
        }
    }

    static async Task<float[]> GetEmbedding(OpenAIClient openAiClient, string text)
    {
        // Split text into chunks
        const int chunkSize = 6000;
        var chunks = new List<string>();
        for (int i = 0; i < text.Length; i += chunkSize)
        {
            chunks.Add(text.Substring(i, Math.Min(chunkSize, text.Length - i)));
        }

        // Get embeddings for each chunk
        var embeddings = new List<float[]>();
        foreach (var chunk in chunks)
        {
            var response = await openAiClient.EmbeddingsEndpoint.CreateEmbeddingAsync(
                "text-embedding-ada-002",
                chunk
            );
            embeddings.Add(response.Data[0].Embedding);
        }

        // Average the embeddings
        if (!embeddings.Any())
        {
            throw new Exception("No embeddings generated");
        }

        var meanEmbedding = new float[embeddings[0].Length];
        for (int i = 0; i < meanEmbedding.Length; i++)
        {
            meanEmbedding[i] = embeddings.Average(e => e[i]);
        }

        return meanEmbedding;
    }

    static async Task<List<SearchPoint>> SearchAudioFragments(QdrantClient qdrantClient, OpenAIClient openAiClient, string collectionName, string searchTerm, int limit = 3)
    {
        // Get embedding for the search term
        var searchVector = await GetEmbedding(openAiClient, searchTerm);
        
        // Search Qdrant using the embedding
        var results = await qdrantClient.SearchAsync(
            collectionName,
            searchVector,
            limit: limit,
            withPayload: true  // Make sure to include the full payload
        );
        
        return results.ToList();
    }

    static async Task<SearchResult> GetAiAnswer(OpenAIClient openAiClient, string question, List<SearchPoint> searchResults)
    {
        if (!searchResults.Any())
        {
            return new SearchResult { Answer = "I do not have the information" };
        }
        
        // Compile context from search results
        var context = "Context from audio transcripts:\n";
        var fragmentsMap = new Dictionary<string, List<dynamic>>();
        
        // Limit context size for each result
        const int maxCharsPerResult = 2000;
        foreach (var result in searchResults)
        {
            var transcript = result.Payload.GetProperty("transcript").GetString();
            var truncatedTranscript = transcript.Length > maxCharsPerResult 
                ? transcript[..maxCharsPerResult] 
                : transcript;
                
            var filename = result.Payload.GetProperty("filename").GetString();
            context += $"\nFrom {filename}:\n{truncatedTranscript}";
            
            var segments = JsonSerializer.Deserialize<List<dynamic>>(
                result.Payload.GetProperty("segments").GetRawText()
            );
            fragmentsMap[filename] = segments;
        }
        
        // Create prompt for OpenAI
        var prompt = $@"Based only on the following context, answer the question. If the context doesn't contain relevant information, respond with ""I do not have the information"".
If you find relevant information, also identify at least two specific segments from the audio that support your answer.

Question: {question}

{context}

Format your response as follows:
ANSWER: [Your answer here]
SEGMENTS: [List the filenames and timestamp ranges that support your answer]
If no information is found, just respond with: ANSWER: I do not have the information";

        var chatResponse = await openAiClient.ChatEndpoint.GetCompletionAsync(new ChatRequest
        {
            Model = "gpt-4-turbo",
            Messages = new List<ChatMessage>
            {
                new ChatMessage(ChatRole.System, "You are a helpful assistant. Only use the provided context to answer questions. If you cannot find the specific information in the context, say 'I do not have the information'."),
                new ChatMessage(ChatRole.User, prompt)
            },
            Temperature = 0,
            MaxTokens = 1000
        });
        
        var responseText = chatResponse.FirstChoice.Message.Content;
        
        // Parse response to extract answer and segments
        if (responseText.Contains("I do not have the information"))
        {
            return new SearchResult { Answer = "I do not have the information" };
        }
        
        // Extract answer and segments from response
        var parts = responseText.Split("SEGMENTS:", StringSplitOptions.TrimEntries);
        var answerPart = parts[0].Replace("ANSWER:", "").Trim();
        var segmentsPart = parts.Length > 1 ? parts[1].Trim() : "";
        
        // Process segments to create fragments list
        var fragments = new List<AudioFragment>();
        foreach (var (filename, segments) in fragmentsMap)
        {
            foreach (var segment in segments)
            {
                var segmentText = segment.GetProperty("text").GetString();
                if (segmentsPart.Contains(segmentText))
                {
                    fragments.Add(new AudioFragment
                    {
                        Filename = filename,
                        Start = segment.GetProperty("start").GetSingle(),
                        End = segment.GetProperty("end").GetSingle(),
                        Text = segmentText
                    });
                }
            }
        }
        
        return new SearchResult
        {
            Answer = answerPart,
            Fragments = fragments.Take(3).ToList()  // Limit to top 3 most relevant fragments
        };
    }

    static async Task QueryDatabase(QdrantClient qdrantClient, OpenAIClient openAiClient, string collectionName)
    {
        Console.WriteLine("Mode 2: Getting AI answer for question");
        var question = "Which extracurricular activities are best for Georgetown?";
        var searchResults = await SearchAudioFragments(qdrantClient, openAiClient, collectionName, "Georgetown extracurricular activities");
        var result = await GetAiAnswer(openAiClient, question, searchResults);

        Console.WriteLine($"\nQuestion: {question}");
        Console.WriteLine($"\nAnswer: {result.Answer}");

        if (result.Fragments.Any())
        {
            Console.WriteLine("\nRelevant audio fragments:");
            foreach (var fragment in result.Fragments)
            {
                Console.WriteLine($"\nFile: {fragment.Filename}");
                Console.WriteLine($"Timestamp: {fragment.Start:F2}s - {fragment.End:F2}s");
                Console.WriteLine($"Text: {fragment.Text}");
            }
        }
    }
} 