using System.Net;
using Microsoft.SemanticKernel;

namespace PinterestImageBuilder;

class Program
{
    static void Main(string[] args)
    {
        AIImageBuilder aiImageBuilder = new AIImageBuilder();
        string prompt = "a happy monkey sitting in a tree. The monkey has a yellow band around its head and is holding a banana. The background is a lush green forest. 8k Photograph.";
        var url = aiImageBuilder.CreateImage(prompt).GetAwaiter();
        IReadOnlyList < ImageContent > result = url.GetResult();
        int count = 0;
        foreach (var content in result)
        {
            Console.WriteLine(content.Uri);   
            using (WebClient client = new WebClient()) 
            {
                client.DownloadFile(new Uri(content.Uri.AbsoluteUri), "image" + count + ".png");
            }
        }
    }
}