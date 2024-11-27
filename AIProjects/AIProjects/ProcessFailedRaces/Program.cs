namespace ProcessFailedRaces;

class Program
{
    static void Main(string[] args)
    {
        // read journal file and get all races that failed to process
        
        FileProcessor fp = new FileProcessor();
        /*string path = "/Users/macmyths/BlazorProjects/AIProjects/AIProjects/BuildRunSiteContent/bin/Debug/net8.0/10028.html";
        string fileContent = File.ReadAllText(path);
        fp.ProcessRace(path, fileContent);*/
        
        //string journalPath = "/Users/macmyths/Desktop/temp/1/RunInUSAService1.txt";
        //fp.ProcessJournal(journalPath);
        
        //string htmlPath = "/Users/macmyths/Desktop/temp/2/13837.html";    // when "when\":\"Nov 24, 2024 - Sunday
        //string htmlPath = "/Users/macmyths/Desktop/temp/2/13852.html";    // \"Races\": {\n        \"Swim\": {\n 
        //string htmlPath = "/Users/macmyths/Desktop/temp/2/14054.html";
        
        //get all files with extension html in the directory
        string dir = "/Users/macmyths/Desktop/temp/2/";
        string[] files = Directory.GetFiles(dir, "*.html");
        foreach (string htmlPath in files)
        {
            fp.ToJson(htmlPath);    
        }
    }
}