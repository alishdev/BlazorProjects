using Qdrant.Client;
using NAudio.Wave;
using Whisper.net;
//using System.Numerics;
using Qdrant.Client.Grpc;
using System.Text.Json;
using Value = Qdrant.Client.Grpc.Value;
using Grpc.Net.Client;
using System.Net.Http;

public class AudioProcessor
{
    private readonly QdrantClient _client;
    private const string CollectionName = "audio_collection";

    public AudioProcessor()
    {
        // create grpc client for qdrant
        var httpHandler = new HttpClientHandler();
        httpHandler.ServerCertificateCustomValidationCallback = 
            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

        var httpClient = new HttpClient(httpHandler);
        httpClient.DefaultRequestVersion = new Version(2, 0);
        httpClient.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionExact;

        var channelOptions = new GrpcChannelOptions
        {
            HttpClient = httpClient,
            MaxReceiveMessageSize = null,
            MaxSendMessageSize = null
        };

        var channel = GrpcChannel.ForAddress($"http://localhost:6334", channelOptions);
        QdrantGrpcClient qdrantGrpcClient = new QdrantGrpcClient(channel);
        _client = new QdrantClient(qdrantGrpcClient);
        //InitializeCollection();   // only when needed to recreate the collection
    }

    public async Task InitializeCollection()
    {
        var exists = await _client.GetCollectionInfoAsync(CollectionName);
        if (exists != null  || exists.Status != CollectionStatus.Green)
        {
            await _client.DeleteCollectionAsync(CollectionName);
        }

        await _client.CreateCollectionAsync(CollectionName, new VectorParams
        {
            Size = 128,
            Distance = Distance.Cosine
        });
    }

    public async Task ProcessAndUploadFiles(string directoryPath)
    {
        var files = Directory.GetFiles(directoryPath, "*.wav");
        
        foreach (var file in files)
        {
            Console.WriteLine($"Processing {Path.GetFileName(file)}");
            
            // Extract audio features using NAudio
            var features = await ExtractAudioFeatures(file);
            
            // Transcribe using Whisper
            var transcriptionResult = await TranscribeAudio(file);
            
            // Upload to Qdrant
            await UploadToQdrant(file, features, transcriptionResult);
            
            Console.WriteLine($"Uploaded {Path.GetFileName(file)} to database");
        }
    }

    private async Task<float[]> ExtractAudioFeatures(string audioPath)
    {
        using var audioFile = new AudioFileReader(audioPath);
        // This is a simplified MFCC calculation - you'll need a proper DSP library
        // for actual MFCC feature extraction
        var features = new float[128];
        // TODO: Implement proper MFCC feature extraction
        return features;
    }

    private async Task<TranscriptionResult> TranscribeAudio(string audioPath)
    {
        using var whisperFactory = WhisperFactory.FromPath("path-to-whisper-model");
        using var processor = whisperFactory.CreateBuilder()
            .WithLanguage("en")
            .Build();

        using var audioFile = new AudioFileReader(audioPath);
        var segments = new List<SegmentData>();

        await foreach (var result in processor.ProcessAsync(audioFile))
        {
            segments.Add(new SegmentData
            {
                Text = result.Text,
                Start = (float)result.Start.TotalSeconds,
                End = (float)result.End.TotalSeconds
            });
        }

        return new TranscriptionResult
        {
            Text = string.Join(" ", segments.Select(s => s.Text)),
            Segments = segments
        };
    }

    private async Task UploadToQdrant(string filename, float[] features, TranscriptionResult transcription)
    {
        var payload = new Dictionary<string, Value>
        {
            { "filename", new Value { StringValue = Path.GetFileName(filename) } },
            { "transcript", new Value { StringValue = transcription.Text } },
            { "segments", new Value { StringValue = JsonSerializer.Serialize(transcription.Segments) } }
        };

        var vector = new Vector();
        vector.Data.AddRange(features.Select(f => (float)f));

        var vectors = new Vectors { Vector = vector };

        var point = new PointStruct
        {
            Id = new PointId { Uuid = Guid.NewGuid().ToString() },
            Payload = { payload },
            Vectors = vectors
        };

        await _client.UpsertAsync(CollectionName, new[] { point });
    }

    public async Task<List<SearchResult>> SearchAudioFragments(string searchTerm)
    {
        Console.WriteLine($"Searching for '{searchTerm}' in database");

        var filter = new Filter();
        filter.Must.Add(new Condition
        {
            Field = new FieldCondition
            {
                Key = "transcript",
                Match = new Match 
                { 
                    Text = searchTerm 
                }
            }
        });

        var response = await _client.ScrollAsync(CollectionName, filter, limit: 10);
        var results = new List<SearchResult>();
        
        foreach (var point in response.Result)
        {
            var result = new SearchResult
            {
                Filename = point.Payload["filename"].StringValue,
                Transcript = point.Payload["transcript"].StringValue
            };
            result.Segments = new List<SegmentData>();
            if (point.Payload["segments"].ListValue.Values.Count > 0)
            {
                result.Segments = point.Payload["segments"].ListValue.Values
                    .Select(s => new SegmentData
                    {
                        Text = s.StructValue.Fields["text"].StringValue,
                        Start = (float)s.StructValue.Fields["start"].DoubleValue,
                        End = (float)s.StructValue.Fields["end"].DoubleValue
                    })
                    .ToList();
            }
            //point.Payload["segments"].ListValue.Values[0].StructValue.Fields["end"].DoubleValue
            results.Add(result);
        }
        
        return results;
    }
    
    public async Task<List<SearchResult>> AnswerQuestion(string searchTerm)
    {
        var processor = new AudioProcessor();
        System.Diagnostics.Debug.WriteLine($"Searching for '{searchTerm}' in database");
        return await processor.SearchAudioFragments(searchTerm);
        
        /*foreach (var result in results)
        {
            System.Diagnostics.Debug.WriteLine($"\nFound 'canoe' in file: {result.Filename}");
            System.Diagnostics.Debug.WriteLine($"Full Transcript: {result.Transcript}");

            foreach (var segment in result.Segments)
            {
                if (segment.Text.ToLower().Contains("canoe"))
                {
                    System.Diagnostics.Debug.WriteLine($"\nTimestamp: {segment.Start:F2}s - {segment.End:F2}s");
                    System.Diagnostics.Debug.WriteLine($"Segment: {segment.Text}");
                }
            }
        }*/
    }
}

public class TranscriptionResult
{
    public string Text { get; set; }
    public List<SegmentData> Segments { get; set; }
}

public class SegmentData
{
    public string Text { get; set; }
    public float Start { get; set; }
    public float End { get; set; }
}

public class SearchResult
{
    public string Filename { get; set; }
    public string Transcript { get; set; }
    public List<SegmentData> Segments { get; set; }
}