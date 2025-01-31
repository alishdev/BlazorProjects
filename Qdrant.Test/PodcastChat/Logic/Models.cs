using Newtonsoft.Json;

namespace PodcastChat.Logic;

public class SearchResultContext
{
    public string? Transcript { get; set; }
    public Dictionary<string, List<AudioFragment>>? Fragments { get; set; }
}

public class TranscriptPayload
{
    [JsonProperty("filename")]
    public PayloadValue? Filename { get; set; }

    [JsonProperty("file_path")]
    public PayloadValue? FilePath { get; set; }

    [JsonProperty("transcript")]
    public PayloadValue? Transcript { get; set; }

    [JsonProperty("segments")]
    public PayloadValue? Segments { get; set; }
}

public class PayloadValue
{
    [JsonProperty("NullValue")]
    public int NullValue { get; set; }

    [JsonProperty("HasNullValue")]
    public bool HasNullValue { get; set; }

    [JsonProperty("DoubleValue")]
    public double DoubleValue { get; set; }

    [JsonProperty("HasDoubleValue")]
    public bool HasDoubleValue { get; set; }

    [JsonProperty("IntegerValue")]
    public int IntegerValue { get; set; }

    [JsonProperty("HasIntegerValue")]
    public bool HasIntegerValue { get; set; }

    [JsonProperty("StringValue")]
    public string? StringValue { get; set; }

    [JsonProperty("HasStringValue")]
    public bool HasStringValue { get; set; }

    [JsonProperty("BoolValue")]
    public bool BoolValue { get; set; }

    [JsonProperty("HasBoolValue")]
    public bool HasBoolValue { get; set; }

    [JsonProperty("StructValue")]
    public StructValue? StructValue { get; set; }

    [JsonProperty("ListValue")]
    public ListValue? ListValue { get; set; }

    [JsonProperty("KindCase")]
    public int KindCase { get; set; }
}

public class StructValue
{
    [JsonProperty("Fields")]
    public Dictionary<string, PayloadValue>? Fields { get; set; }
}

public class ListValue
{
    [JsonProperty("Values")]
    public List<PayloadValue>? Values { get; set; }
}

// Helper extension methods to easily get values
public static class PayloadValueExtensions
{
    public static string GetString(this PayloadValue value)
    {
        return value.HasStringValue ? value.StringValue : null;
    }

    public static double? GetDouble(this PayloadValue value)
    {
        return value.HasDoubleValue ? value.DoubleValue : null;
    }

    public static int? GetInteger(this PayloadValue value)
    {
        return value.HasIntegerValue ? value.IntegerValue : null;
    }

    public static bool? GetBool(this PayloadValue value)
    {
        return value.HasBoolValue ? value.BoolValue : null;
    }

    public static List<TranscriptSegment> GetSegments(this PayloadValue value)
    {
        if (value.ListValue?.Values == null) return new List<TranscriptSegment>();

        return value.ListValue.Values
            .Where(v => v.StructValue?.Fields != null)
            .Select(v => new TranscriptSegment
            {
                Start = v!.StructValue!.Fields["start"]!.GetDouble() ?? 0,
                End = v.StructValue.Fields["end"].GetDouble() ?? 0,
                Text = v.StructValue.Fields["text"].GetString()
            })
            .ToList();
    }
}

// Simplified segment model for easier use
public class TranscriptSegment
{
    public double Start { get; set; }
    public double End { get; set; }
    public string? Text { get; set; }
}