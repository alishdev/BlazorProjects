using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace SemanticKernelPlayground;

public class ArchivePlugin
{
    [KernelFunction("archive")]
    [Description("Saves date to a file on the disk")]
    public void Archive(Kernel kernel, string filename, string data)
    {
        // Archive the current project
        string currentDir = Directory.GetCurrentDirectory();
        string archivePath = Path.Combine(currentDir, filename);
        System.IO.File.WriteAllText(archivePath, data);
        Console.WriteLine($"Archived data to {archivePath}");
    }
}