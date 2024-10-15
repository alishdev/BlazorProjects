namespace SemanticKernelPlayground;

class Program
{
    static void Main(string[] args)
    {
        //BasicTest test = new();
        PluginsTest test = new();
        //test.Test().Wait();
        test.MusicPluginTest().Wait();
    }
}