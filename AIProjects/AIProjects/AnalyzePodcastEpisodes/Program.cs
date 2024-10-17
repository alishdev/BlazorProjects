using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AnalyzePodcastEpisodes;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Analyzing podcast episodes...");
        AnalyzePodcast analyzePodcast = new AnalyzePodcast();
        string podcastPath = @"/Users/macmyths/Desktop/Podcast/Transcribe/All Interviews/ESTELLEFULL.txt";
        PodcastMetadata? metadata = analyzePodcast.ExtractFromPodcast(podcastPath).GetAwaiter().GetResult();
        if (metadata == null)
        {
            Console.WriteLine("Failed to extract metadata from the podcast.");
            return;
        }
        
        // open json file and search for the episode title
        // if not found, append the metadata to the json file to the end of the file

        JArray? podcastMetadata = null;
        var found = false;

        var jsonPath = @"/Users/macmyths/Desktop/Podcast/Transcribe/All Interviews/metadata.json";
        if (File.Exists(jsonPath))
        {
            var jsonContent = File.ReadAllText(jsonPath);
            podcastMetadata = JArray.Parse(jsonContent);
        }

        if (podcastMetadata == null)
            podcastMetadata = new JArray();
        else
            foreach (var item in podcastMetadata)
            {
                var podcast = item as JObject;
                if (podcast != null && podcast["StudentName"]?.ToString() == metadata?.StudentName)
                {
                    found = true;
                    break;
                }
            }

        if (!found)
        {
            podcastMetadata.Add(JObject.FromObject(metadata));
            File.WriteAllText(jsonPath, podcastMetadata.ToString(Formatting.Indented));
        }

        Console.WriteLine("Done!");
    }
}