public class CustomConsole
{
    private static string logFilePath = "log.txt";

    public static void WriteLine(string message)
    {
        // Print to console
        Console.WriteLine(message);

        // Write to file
        using (StreamWriter writer = new StreamWriter(logFilePath, true))
        {
            writer.WriteLine(message);
        }
    }

    public static void Write(string message)
    {
        // Print to console
        Console.Write(message);

        // Write to file
        using (StreamWriter writer = new StreamWriter(logFilePath, true))
        {
            writer.Write(message);
        }
    }

    public static void SetLogFilePath(string path)
    {
        logFilePath = path;
        // Clear the file
        File.WriteAllText(logFilePath, string.Empty);
    }
}
