using LangChain.Abstractions.Chains.Base;
using LangChain.Chains.LLM;
using LangChain.Chains.Sequentials;
using LangChain.Prompts;
using LangChain.Providers.OpenAI.Predefined;
using LangChain.Schema;

namespace LangChainPlayground;
#pragma warning disable SKEXP0050, CS8604
public class BasicModel
{
    public async Task Play()
    {
        using var httpClient = new HttpClient();
        // read api-key setting from launchSettings.json
        var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        
        var llm = new OpenAiLatestFastChatModel(apiKey);

        var firstTemplate = "What is a good name for a company that makes {product}?";
        var firstPrompt = new PromptTemplate(new PromptTemplateInput(firstTemplate, new List<string>(1) { "product" }));

        var chainOne = new LlmChain(new LlmChainInput(llm, firstPrompt)
        {
            Verbose = true,
            OutputKey = "company_name"
        });

        var secondTemplate = "Write a 20 words description for the following company:{company_name}";
        var secondPrompt = new PromptTemplate(new PromptTemplateInput(secondTemplate, new List<string>(1) { "company_name" }));

        var chainTwo = new LlmChain(new LlmChainInput(llm, secondPrompt));

        var overallChain = new SequentialChain(new SequentialChainInput(
            new IChain[]
            {
                chainOne,
                chainTwo
            },
            new[] { "product" },
            new[] { "company_name", "text" }
        ));

        var result = await overallChain.CallAsync(new ChainValues(new Dictionary<string, object>(1)
        {
            { "product", "colourful socks" }
        }));

        Console.WriteLine(result.Value["text"]);
        Console.WriteLine("SequentialChain sample finished.");
    }
}

