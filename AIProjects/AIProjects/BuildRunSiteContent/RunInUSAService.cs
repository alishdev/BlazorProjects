using HtmlAgilityPack;
using MTUtils;
using Newtonsoft.Json;
using NUglify;
using System.Text.RegularExpressions;

namespace BuildRunSiteContent;

public class RunInUSAService
{
    private readonly AIHelper _aiHelper = new AIHelper();
    public RunInUSAService()
    {
        TimeJournal.SetLogFilePath("RunInUSAService.txt");
    }
    public async Task<RunInUSAModel> GetRaceDetailsByUrl(int raceid, string url)
    {
        string content = await UrlToText(url); 
        return await GetRaceDetails(raceid, content);
    }

    private async Task<RunInUSAModel> GetRaceDetails(int raceid, string content)
    {
        TimeJournal.Write(new object[] { content });
        string prompt = """
                        analyze web page about a running race/races from the following information in html ({{$arg1}}). 
                        Extract the data and return it in json format. Json format example: {"City":"Columbia","State":"MD","RaceWebSite":"https://columbiamarathon.com","Added":"10/31/2002","Updated":"10/21/2023","IsBostonQualifier":"true","Races":[{"when":"12/24/2024","distance":"26.2M"},{"when":"12/24/2024","distance":"13.1M"}]}
                        Return only json without any other text.
                        """;

        string result = await _aiHelper.RunPrompt(prompt, content);
        return ParseRaceDetails(raceid, result);
    }

    private RunInUSAModel ParseRaceDetails(int raceid, string content)
    {
        // the text is inside ```json``` tag
        string pattern = @"```json(.*?)```";
        Match match = Regex.Match(content, pattern, RegexOptions.Singleline);
        //if (!match.Success)
        //  throw new Exception("No JSON content found in the text.");
        string innerContent = match.Success ? match.Groups[1].Value.Trim() : content;
        try
        {
            return JsonConvert.DeserializeObject<RunInUSAModel>(innerContent)!;
        }
        catch (Exception)
        {
			var path = Path.Combine(Directory.GetCurrentDirectory(), $"{raceid}.html");
            File.WriteAllText(path, JsonConvert.SerializeObject(innerContent));
            throw new Exception("Error parsing JSON content: " + innerContent);
        }
    }

    private async Task<string> UrlToText(string url)
    {
        HttpClient httpClient = new HttpClient();
        httpClient.Timeout = TimeSpan.FromMinutes(10);
        string response = await httpClient.GetStringAsync(url);
        HtmlDocument htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(response);

        string htmlContent = htmlDoc.DocumentNode.InnerHtml;
        string textContent = Uglify.HtmlToText(htmlContent).Code;
        return textContent;
    }
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