namespace LangChainPlayground;

class Program
{
    static void Main(string[] args)
    {
        BasicModel model = new BasicModel();
        model.Play().Wait();
    }
}