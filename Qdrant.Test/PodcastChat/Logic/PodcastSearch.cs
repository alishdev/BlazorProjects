using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel;
using Qdrant.Client;
using Newtonsoft.Json;

namespace PodcastChat.Logic;

public class SearchResult
{
    public string? Answer { get; set; }
    public List<AudioFragment>? Fragments { get; set; } = new();
}

public class AudioFragment
{
    public string? Filename { get; set; }
    public float Start { get; set; }
    public float End { get; set; }
    public string? Text { get; set; }
}

public class SearchPoint
{
    public string? Id { get; set; }
    //public SearchPayload Payload { get; set; }
    public TranscriptPayload? Payload { get; set; }
    public float Score { get; set; }
    public float[]? Vector { get; set; }
}

public class PodcastSearch
{
    public static async Task<List<SearchPoint>> SearchAudioFragments(QdrantClient qdrantClient, float[] searchVector, string collectionName, string searchTerm, int limit = 3)
    {
        try
        {
            // Get embedding for the search term
            //var searchVector = await GetEmbedding(openAiClient, searchTerm);
            
            // Search Qdrant using the embedding
            var results = await qdrantClient.SearchAsync(
                collectionName: collectionName,
                vector: searchVector,
                limit: (ulong)limit/*,
                withPayload: new WithPayloadSelector { Enable = true }*/
            );
            
            // Convert ScoredPoint to SearchPoint
            return results.Select(r => new SearchPoint
            {
                Id = r.Id.ToString(),
                Payload = JsonConvert.DeserializeObject<TranscriptPayload>(r.Payload.ToString()), //JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(r.Payload)),
                Score = r.Score,
                Vector = searchVector  // Use the search vector since ScoredPoint doesn't return vectors
            }).ToList();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    static async Task<SearchResultContext> GetContext(List<SearchPoint> searchResults)
    {
        // Compile context from search results
        var context = ""; //""Context from audio transcripts:\n";
        var fragmentsMap = new Dictionary<string, List<AudioFragment>>();

        // Limit context size for each result
        const int maxCharsPerResult = 8000;
        foreach (var result in searchResults)
        {
            var transcript = result.Payload!.Transcript;
            var truncatedTranscript = transcript!.StringValue!.Length > maxCharsPerResult
                ? transcript.StringValue[..maxCharsPerResult]
                : transcript.StringValue;

            var filename = result.Payload.Filename;
            context += transcript.StringValue; // $"\nFrom {filename}:\n{truncatedTranscript}";

            List<AudioFragment> audioFragments = new();
            var segments = result!.Payload!.Segments!.ListValue!.Values;
            foreach (var sv in segments!)
            {
                audioFragments.Add(new AudioFragment
                {
                    Start = (float)sv.StructValue.Fields["start"].DoubleValue,
                    End = (float)sv.StructValue.Fields["end"].DoubleValue,
                    Text = sv.StructValue.Fields["text"].StringValue
                });
            }
            fragmentsMap[filename.StringValue] = audioFragments;
        }

        return new SearchResultContext()
        {
            Transcript = context,
            Fragments = fragmentsMap
        };
    }

    static string GetPrompt()
    {
        // Create prompt for OpenAI
        var prompt =
            """
                Based only on the following context, answer the question. If the context doesn't contain relevant information, respond with ""I do not have the information"".
                Question: {{$question}}
                Context: {{$context}}
                Format your response as follows:
                ANSWER: [Your answer here]
                If no information is found, just respond with: ANSWER: I do not have the information";
            """;
            
            return prompt;
    }

    public static async Task<SearchResult> GetAiAnswer(Kernel kernel, string question,
        List<SearchPoint> searchResults)
    {
        try
        {
            if (!searchResults.Any())
            {
                return new SearchResult { Answer = "I do not have the information" };
            }

            //question = "Which do you know about housing in Georgetown?";
            var searchResultContext = await GetContext(searchResults);
            
            string prompt = GetPrompt();

            var chatResponse = await kernel.InvokePromptAsync(
                prompt,
                new KernelArguments()
                {
                    { "question", question },
                    { "context", searchResultContext.Transcript }
                }
            );

            // Parse response to extract answer and segments
            if (chatResponse.ToString().Contains("I do not have the information"))
            {
                return new SearchResult { Answer = "I do not have the information" };
            }
            
            return new SearchResult
            {
                Answer = chatResponse.GetValue<string>(),
                Fragments = searchResultContext.Fragments.Values.SelectMany(f => f).ToList()
            };

            /*
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
                    var segmentText = segment.Text;
                    if (segmentsPart.Contains(segmentText))
                    {
                        fragments.Add(new AudioFragment
                        {
                            Filename = filename,
                            Start = segment.Start,
                            End = segment.End,
                            Text = segmentText
                        });
                    }
                }
            }*/
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    static async Task<float[]> GetEmbeddings(string question, string openAiKey)
    {
#pragma warning disable SKEXP0010, SKEXP0001
        ITextEmbeddingGenerationService openAiTextEmbedding = new Microsoft.SemanticKernel.Connectors.OpenAI.OpenAITextEmbeddingGenerationService(
            "text-embedding-ada-002", openAiKey);
        
        var searchVector = await openAiTextEmbedding.GenerateEmbeddingAsync(question);

        return searchVector.ToArray();
    }
    
    private static string GetOpenAIKey()
    {
        var openAiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        if (string.IsNullOrEmpty(openAiKey))
        {
            throw new Exception("Please set OPENAI_API_KEY environment variable");
        }

        return openAiKey;
    }
    private static Kernel GetKernel()
    {
        var builder = Kernel.CreateBuilder()
            .AddOpenAIChatCompletion(apiKey: GetOpenAIKey(),
                modelId: "chatgpt-4o-latest");
        var kernel = builder.Build();
        return kernel;
    }

    public static async Task<SearchResult> QueryDatabase(string question)
    {
        // Initialize Qdrant client
        Console.WriteLine("Connecting to Qdrant");
        var qdrantClient = new QdrantClient("localhost", 6334);
        
        string collectionName = "audio_collection";
            
        PodcastSearch podcastSearch = new PodcastSearch();
        
        var kernel = GetKernel();
        
        var searchVector = await GetEmbeddings(question, GetOpenAIKey());
        
        var searchResults = await PodcastSearch.SearchAudioFragments(qdrantClient, searchVector.ToArray(), collectionName, question);
        
        var result = await PodcastSearch.GetAiAnswer(kernel, question, searchResults);

        return result;
    }

/*
    static async Task QueryDatabase(QdrantClient qdrantClient, OpenAIService openAiClient, string collectionName)
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
    }    */

/*
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
        var openAiClient = new OpenAIService(openAiKey);

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

    static async Task CreateCollectionAndProcessFiles(QdrantClient qdrantClient, OpenAIService openAiClient, string collectionName)
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
        using var whisperFactory = WhisperFactory.FromPath("ggml-base.bin");
        using var processor = whisperFactory.CreateBuilder()
            .WithLanguage("auto")
            .Build();

        Console.WriteLine("Loading mp3 files");
        var mp3Files = Directory.GetFiles("mp3_files", "*.mp3");
        foreach (var file in mp3Files)
        {
            await ProcessFile(file, processor, qdrantClient, openAiClient, collectionName);
        }
    }

    static async Task ProcessFile(string filePath, IWhisperProcessor processor, QdrantClient qdrantClient, OpenAIService openAiClient, string collectionName)
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
            var segments = new List<dynamic>();
            await foreach (var result in processor.ProcessAsync(tempWavPath))
            {
                segments.Add(new
                {
                    text = result.Text,
                    start = result.Start,
                    end = result.End
                });
            }

            var transcript = string.Join(" ", segments.Select(s => s.text));

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
    }*/
/*
    static async Task<float[]> GetEmbedding(OpenAIService openAiClient, string text)
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
            var embeddingResponse = await openAiClient.Embeddings.CreateEmbedding(new EmbeddingCreateRequest
            {
                Model = Models.TextEmbeddingAdaV2,
                Input = chunk
            });
            embeddings.Add(embeddingResponse.Data[0].Embedding.Select(d => (float)d).ToArray());  // Convert double[] to float[]
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
    }*/

}