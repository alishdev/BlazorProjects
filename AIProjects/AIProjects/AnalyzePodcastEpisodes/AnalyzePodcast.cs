using Microsoft.SemanticKernel;
using Newtonsoft.Json;

namespace AnalyzePodcastEpisodes;

public class AnalyzePodcast
{
    public async Task<PodcastMetadata?> ExtractFromPodcast(string podcastPath)
    {
        if (!File.Exists(podcastPath))
            throw new FileNotFoundException("Podcast file not found.", podcastPath);

        string podcastText = await File.ReadAllTextAsync(podcastPath);

        var openApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        if (string.IsNullOrEmpty(openApiKey))
            throw new Exception("OpenAI API key not found.");

        var builder = Kernel.CreateBuilder()
            .AddOpenAIChatCompletion(apiKey: openApiKey,
                modelId: "chatgpt-4o-latest");

        string prompt =
            @"This is a podcast episode where Kamila interviewed a student about their college application experience: {{$podcastText}}."
            + " Extract the following information from the podcast: Student Name (firstname and lastname in upper case) Episode title, Colleges applied, Colleges accepted, GPA (weighted and unweighted)"
            + ", SAT/ACT scores, Extracurricular activities, Essay topics, Interview experiences, Scholarships received. Save the information in a JSON object with the following format {{$jsonFormat}}.";

        string jsonFormat = @"{
""StudentName"": ""string"",
  ""EpisodeTitle"": ""string"",
  ""CollegesApplied"": [""string""],
  ""CollegesAccepted"": [""string""],
  ""GPAWeighted"": 0.0,
  ""GPAUnweighted"": 0.0,
  ""SATScores"": {
    ""string"": 0
  },
  ""ACTScores"": {
    ""string"": 0
  },
  ""ExtracurricularActivities"": [""string""],
  ""EssayTopics"": [""string""],
  ""InterviewExperiences"": [""string""],
  ""ScholarshipsReceived"": [""string""]
}";

        var kernel = builder.Build();

        var result = await kernel.InvokePromptAsync(
            prompt,
            new KernelArguments()
            {
                { "podcastText", podcastText },
                { "jsonFormat", jsonFormat }
            }
        );

        Console.WriteLine(result);

        // Deserialize the result to PodcastMetadata
        if (!string.IsNullOrEmpty(result.ToString()))
        {
            string resultString = result.ToString();
            // remove starting and ending garbage produced by AI
            string aiPrefix = @"```json";
            if (resultString.StartsWith(aiPrefix))
                resultString = resultString.Substring(aiPrefix.Length);
            string aiSuffix = @"```";
            if (resultString.EndsWith(aiSuffix))
                resultString = resultString.Substring(0, resultString.Length - aiSuffix.Length);
            return JsonConvert.DeserializeObject<PodcastMetadata>(resultString);
        }

        return null;
    }
}