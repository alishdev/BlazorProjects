namespace ParseAnthemFile
{
    internal class TimeJournal
    {
        private static string fileName = string.Empty;

        public static void SetLogFilePath(string path)
        {
            fileName = path;
        }

        public static void Write(object[] args)
        {
            if (fileName == string.Empty)
            {
                fileName = $"TimeJournal-{DateTime.Now.Month}-{DateTime.Now.Day}-{DateTime.Now.Ticks}.txt";
                System.IO.File.WriteAllText(fileName, string.Empty);
            }
            // append to tex file
            System.IO.File.AppendAllText(fileName, string.Join(',', args) + Environment.NewLine);
        }
    }
}
