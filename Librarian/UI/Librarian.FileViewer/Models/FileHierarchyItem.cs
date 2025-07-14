namespace Librarian.FileViewer.Models;

public class FileHierarchyItem
{
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public long Size { get; set; }
    public DateTime DateModified { get; set; }
    public bool IsFile { get; set; }
    public List<FileHierarchyItem> Children { get; set; } = new();
    public string Extension => IsFile ? System.IO.Path.GetExtension(Name) : string.Empty;
    public bool HasChildren => Children.Any();
}