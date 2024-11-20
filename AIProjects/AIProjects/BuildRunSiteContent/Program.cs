using Newtonsoft.Json;

namespace BuildRunSiteContent;

class Program
{
    static void Main(string[] args)
    {
        // TODO: read https://runningintheusa.com/details/10028
        // return information about the race in json format
        // use the link to race website to get more information
        /*RunInUSAService runInUSAService = new RunInUSAService();
        //string url = "https://runningintheusa.com/details/10028";
        string filePath = "/Users/macmyths/Desktop/temp/10028.html";
        string fileContent = File.ReadAllText(filePath);
        RunInUSAModel raceDetails = runInUSAService.GetRaceDetails(fileContent).GetAwaiter().GetResult();
        
        // print each field in raceDetails to console on a separate line
        Console.WriteLine("City: " + raceDetails.City);
        Console.WriteLine("State: " + raceDetails.State);
        Console.WriteLine("RaceWebSite: " + raceDetails.RaceWebSite);
        Console.WriteLine("Added: " + raceDetails.Added);
        Console.WriteLine("Updated: " + raceDetails.Updated);
        Console.WriteLine("IsBostonQualifier: " + raceDetails.IsBostonQualifier);
        foreach (var race in raceDetails.Races)
        {
            Console.WriteLine($"race: {race.Distance} on {race.When}");
        }*/
        
        CollectAllRaces();
    }

    static void CollectAllRaces()
    {
        int page = 5001;

        RunInUSAService runInUSAService = new RunInUSAService();
        for (int i = page; i < 40000; i++)
        {
            string url = $"https://runningintheusa.com/details/{i}";
            string filePath = $"{i}.json";
            try
            {
                RunInUSAModel raceDetails = runInUSAService.GetRaceDetailsByUrl(url).GetAwaiter().GetResult();
                if (string.IsNullOrEmpty(raceDetails.City) && string.IsNullOrEmpty(raceDetails.State))
                    Console.WriteLine($"Skip {i}");
                else
                {
                    File.WriteAllText(filePath, JsonConvert.SerializeObject(raceDetails));
                    Console.WriteLine(i);   
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Skip {i}");
            }
        }
        Console.WriteLine("Done");
    }
}