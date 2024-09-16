using MTUtils;

namespace TextTool
{
    internal class Program
    {
        private static void Main(string[] args)
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
