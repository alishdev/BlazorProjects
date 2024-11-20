using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Newtonsoft.Json;
using NUglify;

namespace BuildRunSiteContent;

public class RunInUSAService
{
    public async Task<RunInUSAModel> GetRaceDetailsByUrl(string url)
    {
        string content = await UrlToText(url);
        // parse the content to get the details
        return await GetRaceDetails(content);
    }
    
    public async Task<RunInUSAModel> GetRaceDetails(string content)
    {
        string prompt = """
                        analyze web page about a running race/races from the follwing information in html ({{$arg1}}). 
                        Extract the data and return it in json format. Json format example: {"City":"Columbia","State":"MD","RaceWebSite":"https://columbiamarathon.com","Added":"10/31/2002","Updated":"10/21/2023","IsBostonQualifier":"true","Races":[{"when":"12/24/2024","distance":"26.2M"},{"when":"12/24/2024","distance":"13.1M"}]}
                        Return only json without any other text.
                        """;
        AIHelper aiHelper = new AIHelper();
        string result = await aiHelper.RunPrompt(prompt, content);
        return ParseRaceDetails(result);
    }
    
    public RunInUSAModel ParseRaceDetails(string content)
    {
        // the text is inside ```json``` tag
        string pattern = @"```json(.*?)```";
        Match match = Regex.Match(content, pattern, RegexOptions.Singleline);
        if (!match.Success)
            throw new Exception("No JSON content found in the text.");
        string innerContent = match.Groups[1].Value.Trim();
        try
        {
            return JsonConvert.DeserializeObject<RunInUSAModel>(innerContent)!;
        }
        catch (Exception ex)
        {
            throw new Exception("Error parsing JSON content: " + ex.Message);
        }
    }
    
    public async Task<string> UrlToText(string url)
    {
        var httpClient = new HttpClient();
        httpClient.Timeout = TimeSpan.FromMinutes(10);
        var response = await httpClient.GetStringAsync(url);
        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(response);

        string htmlContent = htmlDoc.DocumentNode.InnerHtml;
        string textContent = Uglify.HtmlToText(htmlContent).Code;
        return textContent;
    }
}

public class RaceDateDistance
{
    public DateTime When { get; set; }
    public string Distance { get; set; }
}

public class RunInUSAModel
{
    public string City { get; set; }
    public string State { get; set; }
    public RaceDateDistance[] Races { get; set; }
    public string RaceWebSite { get; set; }
    public bool IsBostonQualifier { get; set; }
    public DateTime Added { get; set; }
    public DateTime Updated { get; set; }
    public string PageContent { get; set; }
}