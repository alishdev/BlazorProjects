using Newtonsoft.Json;

namespace ProcessFailedRaces;

public class FileProcessor
{
    public void ProcessRace(string path)
    {
        string innerContent = ReadContentCleanup(path);
        File.WriteAllText(path: Path.ChangeExtension(path:path, extension:".json"), contents:innerContent);
        File.Delete(path);
    }

    private string Cleanup(string innerContent)
    {
        if (innerContent.StartsWith('\"'))
        {
            innerContent = innerContent.Substring(1);
            // remove the last character
            innerContent = innerContent.Substring(0, innerContent.Length - 1);
        }

        // replace \" with "
        innerContent = innerContent.Replace("\\\"", "\"").Replace("\\n", "").Replace("\\r", "");
        return innerContent;
    }

    private string ReadContentCleanup(string path)
    {
        string innerContent = File.ReadAllText(path);
        return Cleanup(innerContent);
    }

    public void ToJson(string path)
    {
        var exceptionSettings = new JsonSerializerSettings { Error = (se, ev) => { ev.ErrorContext.Handled = true; } };
        string innerContent = ReadContentCleanup(path);
        var json = JsonConvert.DeserializeObject<RunInUSAModel>(innerContent, exceptionSettings);
        string jsonText = JsonConvert.SerializeObject(json);
        File.WriteAllText(path: Path.ChangeExtension(path:path, extension:".json"), contents:innerContent);
        File.Delete(path);
    }

    public void ProcessJournal(string path)
    {
        int count = 0;
        string dir = Path.GetDirectoryName(path)!;
        using (StreamReader reader = new StreamReader(path))
        {
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                string firstline = line;
                string? json = null;
                if (line.EndsWith("Error parsing JSON content: {"))
                {
                    json = "{";
                    while ((line = reader.ReadLine()) != null)
                    {
                        json += line;
                        if (line == "}" || (line.Length == 2 && line[0] == '}' /*&& line[1] == '\u200B'*/))
                            break;
                    }
                }
                else if (line.Contains("Error parsing JSON content:"))
                {
                    int index = line.IndexOf("Error parsing JSON content:");
                    json = line.Substring(index + "Error parsing JSON content:".Length);
                }
                
                // save the json content to a file
                if (json != null)
                {
                    string[] parts = firstline.Split(new char[] {',' });
                    int raceid = int.Parse(parts[0]);
                    Console.WriteLine(raceid);
                    string cleanupText = Cleanup(json);
                    File.WriteAllText(path: (Path.Combine(dir, $"{raceid}.html")), contents:cleanupText);
                    File.Delete(path);
                    
                    count++;
                }
            }
        }
        Console.WriteLine($"count = {count}");
    }

    public class RaceDateDistance
    {
        public DateTime? When { get; set; }
        public string? Distance { get; set; }
    }

    public class RunInUSAModel
    {
        public string? City { get; set; }
        public string? State { get; set; }
        public RaceDateDistance[]? Races { get; set; }
        public string? RaceWebSite { get; set; }
        public bool? IsBostonQualifier { get; set; }
        public DateTime? Added { get; set; }
        public DateTime? Updated { get; set; }
        public string? PageContent { get; set; }
    }
}