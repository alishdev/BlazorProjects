using Newtonsoft.Json;

namespace ProcessFailedRaces;

public class FileProcessor
{
    public void ProcessRace(string path, string innerContent)
    {
        if (innerContent.StartsWith('\"'))
        {
            innerContent = innerContent.Substring(1);
            // remove the last character
            innerContent = innerContent.Substring(0, innerContent.Length - 1);
        }

        // replace \" with "
        innerContent = innerContent.Replace("\\\"", "\"").Replace("\\n", "").Replace("\\r", "");
        File.WriteAllText(path: Path.ChangeExtension(path:path, extension:".json"), contents:innerContent);
        File.Delete(path);
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