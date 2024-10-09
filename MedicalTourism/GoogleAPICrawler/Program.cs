namespace GoogleAPICrawler;

class Program
{
    static void Main(string[] args)
    {
        GoogleApiUtil util = new GoogleApiUtil();
        Task task = util.SearchForKeyword("shoes");
        task.Wait();
        Console.WriteLine("Finished");
    }
}