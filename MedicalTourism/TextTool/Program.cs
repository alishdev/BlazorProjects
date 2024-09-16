using MTUtils;

namespace TextTool
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            if (args[0] == "-sh")
            {
                RunShrink(args);
            }
            else if (args[0] == "-f2")
            {
                RunAnalyzeFile2(args);
            }
        }

        private static void RunAnalyzeFile2(string[] args)
        {
            string filePath = null;

            foreach (string arg in args)
            {
                if (File.Exists(arg))
                {
                    filePath = arg;
                }
            }

            if (!string.IsNullOrEmpty(filePath))
            {
                CustomConsole.WriteLine($"Analyzing file {filePath}.");
                AnalyzaAnthemFile1and2.AnalyzeFile2(filePath);
                CustomConsole.WriteLine("File processed successfully.");
            }
            else
            {
                CustomConsole.WriteLine("Usage: TextTool -f2 <file>");
            }
        }

        private static void RunShrink(string[] args)
        {
            string filePath = null;
            int lineCount = 0;

            foreach (string arg in args)
            {
                if (File.Exists(arg))
                    filePath = arg;
                if (lineCount == 0 && int.TryParse(arg, out int nVal))
                    lineCount = nVal;
            }

            if (lineCount > 0 && !string.IsNullOrEmpty(filePath))
            {
                CustomConsole.WriteLine($"Shrinking file {filePath} to {lineCount} lines.");
                FirstLines firstLines = new();
                firstLines.ShrinkFile(filePath, lineCount);
                CustomConsole.WriteLine("File processed successfully.");
            }
            else
            {
                CustomConsole.WriteLine("Usage: TextTool <file> <lineCount>");
            }
        }
    }
}
