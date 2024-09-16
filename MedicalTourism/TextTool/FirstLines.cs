namespace TextTool
{
    internal class FirstLines
    {
        // This method returns the first N lines of a text file
        public void ShrinkFile(string filePath, int lineCount)
        {
            List<string> firstLines = new List<string>();

            using (StreamReader reader = new StreamReader(filePath))
            {
                for (int i = 0; i < lineCount; i++)
                {
                    string line = reader.ReadLine();
                    if (line == null)
                        break;

                    firstLines.Add(line);
                }
            }

            string outputPath = Path.Combine(Path.GetDirectoryName(filePath),
                Path.GetFileNameWithoutExtension(filePath) + "-min" + Path.GetExtension(filePath));
            File.WriteAllLines(outputPath, firstLines);
        }
    }
}
