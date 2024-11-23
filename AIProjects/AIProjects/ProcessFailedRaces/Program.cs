namespace ProcessFailedRaces;

class Program
{
    static void Main(string[] args)
    {
        // read journal file and get all races that failed to process
        
        FileProcessor fp = new FileProcessor();
        string path = "/Users/macmyths/BlazorProjects/AIProjects/AIProjects/BuildRunSiteContent/bin/Debug/net8.0/10028.html";
        string fileContent = File.ReadAllText(path);
        fp.ProcessRace(path, fileContent);
    }
}