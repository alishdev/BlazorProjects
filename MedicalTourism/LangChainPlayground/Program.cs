namespace LangChainPlayground;

class Program
{
    static void Main(string[] args)
    {
        BasicModel model = new BasicModel();
        //RAGModel model = new RAGModel();
        model.Play().Wait();
    }
}