using Librarian.Core;

namespace FileCrawler
{
    public class FileCrawler : ICrawler
    {
        public void Run(object parameter)
        {
            try
            {
                if (parameter == null)
                {
                    Console.WriteLine("Error: Parameter cannot be null");
                    return;
                }

                if (parameter is not string directoryPath)
                {
                    Console.WriteLine($"Error: Parameter must be a string, received {parameter.GetType()}");
                    return;
                }

                if (string.IsNullOrWhiteSpace(directoryPath))
                {
                    Console.WriteLine("Error: Directory path cannot be empty or whitespace");
                    return;
                }

                if (!Directory.Exists(directoryPath))
                {
                    Console.WriteLine($"Error: Directory '{directoryPath}' does not exist");
                    return;
                }

                Console.WriteLine($"FileCrawler started for directory: {directoryPath}");
                
                var files = Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories);
                
                Console.WriteLine($"Found {files.Length} files:");
                
                foreach (var file in files)
                {
                    try
                    {
                        var fileInfo = new FileInfo(file);
                        Console.WriteLine($"File: {file} (Size: {fileInfo.Length} bytes, Modified: {fileInfo.LastWriteTime})");
                    }
                    catch (Exception fileEx)
                    {
                        Console.WriteLine($"Error processing file '{file}': {fileEx.Message}");
                    }
                }
                
                Console.WriteLine($"FileCrawler completed for directory: {directoryPath}");
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"Access denied: {ex.Message}");
            }
            catch (DirectoryNotFoundException ex)
            {
                Console.WriteLine($"Directory not found: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error in FileCrawler: {ex.Message}");
            }
        }
    }
}