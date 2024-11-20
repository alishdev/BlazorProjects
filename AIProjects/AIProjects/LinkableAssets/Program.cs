namespace LinkableAssets;

class Program
{
    static void Main(string[] args)
    {
        FindGoodDomain findGoodDomain = new FindGoodDomain();
        List<string> domains = findGoodDomain.FindDomains().Result;
        foreach (string domain in domains)
        {
            Console.WriteLine(domain);
        }
    }
}