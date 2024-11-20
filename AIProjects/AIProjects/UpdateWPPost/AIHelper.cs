using Microsoft.SemanticKernel;

namespace UpdateWPPost;

public class AIHelper
{
    public async Task<string> ComparePosts(string keyword, string post1, string post2)
    {
        //compare file nsls1.txt and nsls2.txt and tell me which one better answers the question "is nsls legit"?
        //Explain why and provide examples from texts.
        
        // read api key from environment variable
        string? openApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        if (string.IsNullOrEmpty(openApiKey))
        {
            Console.WriteLine("Please set OPENAI_API_KEY environment variable.");
            throw new Exception("OPENAI_API_KEY is not set.");
        }
        
        // create openai client
        var builder = Kernel.CreateBuilder().
            AddOpenAIChatCompletion(apiKey: openApiKey,
                                    modelId: "chatgpt-4o-latest");
        var kernel = builder.Build();
        
        // create prompt
        string prompt = """
            compare post 1 with text ({{$post1}}) and post 2 with text ({{$post2}}) 
            and tell me which one better answers the question {{$keyword}}?
            Explain why and provide examples from texts.
            """;
        var result = await kernel.InvokePromptAsync(prompt, new KernelArguments(){
            {"post1", post1},
            {"post2", post2},
            {"keyword", keyword}
        });
        ;
        return result.ToString();
    }
}