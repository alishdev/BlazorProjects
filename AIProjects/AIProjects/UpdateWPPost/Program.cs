namespace UpdateWPPost;

class Program
{
    static void Main(string[] args)
    {
        Longitude = "123.456";
        Latitude = "78.90";
        var location = $$"""
                         You are at {{{Longitude}}, {{Latitude}}}
                         """;
        Console.WriteLine(location);
    }

    public static string Latitude { get; set; }

    public static string? Longitude { get; set; }
}