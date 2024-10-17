namespace UpdateWPPost;

class Program
{
    static void Main(string[] args)
    {
        string keyword = "is wgu a good school";
        string myUrl = @"https://collegerealitycheck.com/western-governors-university/";
        
        var searchApi = new SearchApi();
        string myPost = searchApi.UrlToText(myUrl).GetAwaiter().GetResult();
        string result = myPost + "\n\n----------------\n\n";
        Console.WriteLine(result);

        List<string> posts = searchApi.GetPostsByKeyword(keyword).Result;
        foreach (string page in posts)
        {
            Console.WriteLine(page);
        }
        
        var aiHelper = new AIHelper();
        result = aiHelper.ComparePosts(keyword, posts[0], myPost).Result;
        Console.WriteLine($"Result: {result}" + "\n.Done.");
    }
}