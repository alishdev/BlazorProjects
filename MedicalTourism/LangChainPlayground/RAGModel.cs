using LangChain.Providers.HuggingFace.Downloader;
using LangChain.Providers.LLamaSharp;
using static LangChain.Chains.Chain;

namespace LangChainPlayground;


public class RAGModel
{
    public async Task Play()
    {
        // get system directory for downloaded files
        //  var systemDir = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        //var filename = Path.Combine(systemDir, "Romeo.txt");
        
        // get model path
        var modelPath = await HuggingFaceModelDownloader.GetModelAsync(
            repository: "TheBloke/Thespis-13B-v0.5-GGUF",
            fileName: "thespis-13b-v0.5.Q2_K.gguf",
            version: "main");

// load model
        var model = LLamaSharpModelInstruction.FromPath(modelPath);

// building a chain
        var prompt = @"
You are an AI assistant that greets the world.
World: Hello, Assistant!
Assistant:";


        var chain =
            Set(prompt, outputKey: "prompt")
            | LLM(model, inputKey: "prompt", outputKey: "result");


        var result = await chain.RunAsync("result");


        Console.WriteLine(result);
    }
}