using MTUtils;

namespace TextTool
{
    public class AnalyzaAnthemFile1and2
    {
        public static void AnalyzeFile1(string file1)
        {
        }

        public static void AnalyzeFile2(string file2)
        {
            // read file2 into list of strings
            List<string> lines = new List<string>();
            lines.AddRange(File.ReadAllLines(file2));

            Dictionary<string, int> plans = new Dictionary<string, int>();
            // process each line
            foreach (string line in lines)
            {
                // split line into words
                int x, y;
                string prefix;
                (x, y, prefix) = FindOfIndex(line);
                if (!plans.ContainsKey(prefix))
                    plans.Add(prefix, 0);
                plans[prefix]++;
                //CustomConsole.WriteLine($"x={x}, y={y}, prefix={prefix}");
            }
            // save plans to csv file
            string csvFile = Path.Combine(Path.GetDirectoryName(file2), Path.GetFileNameWithoutExtension(file2) + "-clean.csv");
            File.Delete(csvFile);

            // save line by line to text file
            foreach (string line in plans.Keys)
            {
                File.AppendAllText(csvFile, $"{line},{plans[line]}" + Environment.NewLine);
            }
            CustomConsole.WriteLine($"File {csvFile} saved.");
        }

        private static (int, int, string) FindOfIndex(string line)
        {
            int X = 0, Y = 0;

            // remove first column - it's just an index
            int index = line.IndexOf(",");
            if (index > 0)
            {
                line = line.Substring(index + 1);
            }

            string prefix = line;

            // parsing string _23_of_35.
            int ofIndex = line.IndexOf("_of_");
            if (ofIndex > 0)
            {
                int start = ofIndex + 4;
                int end = line.IndexOf(".", start);
                string number = line.Substring(start, end - start);
                Y = int.Parse(number);

                start = end = ofIndex - 1;
                while (start > 1 && line[start - 1] != '_')
                {
                    start--;
                }
                number = line.Substring(start, end - start + 1);
                X = int.Parse(number);

                // find prefix - text before 01_of_02
                prefix = line.Substring(0, ofIndex - number.Length - 1);
            }
            else
            {
                // TODO: parse 01_02.json
                Uri uri = new Uri(line);
                // get filename from url
                string filename = Path.GetFileName(uri.LocalPath);
                string filenameWithoutExtension = filename.Replace(".json.gz", "");
                string[] parts = filenameWithoutExtension.Split('_');
                if (parts.Length > 0)
                {
                    int len = parts.Length;
                    if (int.TryParse(parts[len - 1], out Y) && int.TryParse(parts[len - 2], out X))
                    {
                        if (X > Y)
                        {
                            X = Y = 0;
                        }
                        else
                        {
                            string postFix = string.Join("_", parts[len - 2], parts[len - 1]);
                            ofIndex = line.IndexOf(postFix);
                            prefix = line.Substring(0, ofIndex - 1);
                        }
                    }
                }
            }

            return (X, Y, prefix);
        }
    }
}
