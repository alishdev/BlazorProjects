using System.ComponentModel;
using Microsoft.SemanticKernel;
using SimpleFeedReader;

namespace SemanticKernelPlayground;

public class NewsPlugin
{
    [KernelFunction("get_news")]    // in snake case for python
    [Description("Gets news items for today's date")]
    [return: Description("A list of curret news stories.")]
    public List<FeedItem> GetNews(Kernel kernel, string category)
    {
        var feed = new FeedReader();
        return feed.RetrieveFeed($"https://rss.nytimes.com/services/xml/rss/nyt/{category}.xml").Take(5).ToList();
    }
}