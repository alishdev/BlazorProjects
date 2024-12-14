using System.Globalization;
using System.Runtime.InteropServices.JavaScript;

namespace ParseStrava;

class Program
{
    static void Main(string[] args)
    {
        ParseStravaActivities();
        Console.WriteLine("Hello, World!");
    }

    static void ParseStravaActivities()
    {
        // read file
        // each line contains 3 fields: date, time, distance
        // parse each line
        // sort by date
        // aggragate by week
        // output to file
        
        List<StravaActivity> activities = new List<StravaActivity>();
        
        string[] lines = File.ReadAllLines(@"/Users/macmyths/Desktop/temp/strava-activities.csv");
        foreach (string line in lines)
        {
            string[] parts = line.Split(new char[] { ',' });
            int len = parts.Length;
            string time = parts[len - 2];
            string distance = parts[len - 1];
            
            // parse date from "Oct 17, 2024, 3:25:16 PM"
            string dateString = parts[0].Replace("\"","").Trim() + parts[1];
            var cultureInfo = new CultureInfo("en-US");
            var dateTime = DateTime.Parse(dateString, cultureInfo);

            StravaActivity activity = new StravaActivity()
            {
                dt = dateTime,
                minutes = (decimal.Parse(time)/60.0m).ToString("{0.00}"),
                distance = decimal.Parse(distance)
            };
            activities.Add(activity);
        }
        
        activities.Sort((a, b) => a.dt.CompareTo(b.dt));
        /*for (int i = 0; i < activities.Count; i++)
        {
            Console.WriteLine($"{activities[i].dt}, {activities[i].minutes}, {activities[i].distance}");
        }*/
        AggregateByWeek(activities, out var l2023, out var l2024);
        Console.WriteLine("2023: ");
        for(int i = 1; i <= 52; i++)
        {
            if (l2023.ContainsKey(i))
            {
                Console.Write($"Week {i}, {l2023[i][0].dt.ToShortDateString()}");
                foreach (var activity in l2023[i])
                {
                    Console.Write($", {activity.minutes}");
                }
                Console.WriteLine();
            }
        }
        
        Console.WriteLine("2024: ");
        for(int i = 1; i <= 52; i++)
        {
            if (l2024.ContainsKey(i))
            {
                Console.Write($"Week {i}, {l2024[i][0].dt.ToShortDateString()}");
                foreach (var activity in l2024[i])
                {
                    Console.Write($", {activity.minutes}");
                }
                Console.WriteLine();
            }
        }
    }
    
    // function to aggregate by week and print to file
    static void AggregateByWeek(List<StravaActivity> activities, 
        out Dictionary<int, List<StravaActivity>> l2023, 
        out Dictionary<int, List<StravaActivity>> l2024)
    {
        l2023 = new();
        l2024 = new();
        // group by week
        //var groups = activities.GroupBy(a => a.dt);
        CultureInfo myCI = new CultureInfo("en-US");
        Calendar myCal = myCI.Calendar;
        
        var groups = activities.GroupBy(a => myCal.GetWeekOfYear(a.dt, myCI.DateTimeFormat.CalendarWeekRule, DayOfWeek.Monday));
        foreach (var group in groups)
        {
            foreach (var activity in group)
            {
                if (activity.dt.Year == 2023)
                {
                    if (!l2023.ContainsKey(group.Key))
                    {
                        l2023[group.Key] = new List<StravaActivity>();
                    }
                    l2023[group.Key].Add(activity);
                }
                else if (activity.dt.Year == 2024)
                {
                    if (!l2024.ContainsKey(group.Key))
                    {
                        l2024[group.Key] = new List<StravaActivity>();
                    }
                    l2024[group.Key].Add(activity);
                }
            }
        }
    }

    class StravaActivity
    {
        public DateTime dt { get; set; }
        public string? minutes { get; set; }
        public decimal distance { get; set; }
    }
}