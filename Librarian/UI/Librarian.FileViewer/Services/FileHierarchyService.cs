using System.Text.Json;
using Librarian.FileViewer.Models;

namespace Librarian.FileViewer.Services;

public class FileHierarchyService
{
    private readonly string _basePath = @"C:\Projects\BlazorProjects\Librarian\Crawl\source_docs";
    private readonly string _hierarchyFile = "file_hierarchy.json";
    private List<FileHierarchyItem>? _cachedHierarchy;

    public async Task<List<FileHierarchyItem>> GetFileHierarchyAsync()
    {
        if (_cachedHierarchy != null)
            return _cachedHierarchy;

        var filePath = Path.Combine(_basePath, _hierarchyFile);
        
        if (!File.Exists(filePath))
        {
            // Create a sample hierarchy if the file doesn't exist
            _cachedHierarchy = CreateSampleHierarchy();
            await SaveHierarchyAsync(_cachedHierarchy);
            return _cachedHierarchy;
        }

        try
        {
            var json = await File.ReadAllTextAsync(filePath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            
            // Try to deserialize as the expected format first
            try
            {
                _cachedHierarchy = JsonSerializer.Deserialize<List<FileHierarchyItem>>(json, options) ?? new();
                return _cachedHierarchy;
            }
            catch
            {
                // If that fails, try to parse as the current JSON structure
                _cachedHierarchy = await ParseCurrentJsonStructure(json);
                return _cachedHierarchy;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading file hierarchy: {ex.Message}");
            _cachedHierarchy = CreateSampleHierarchy();
            return _cachedHierarchy;
        }
    }

    public async Task SaveHierarchyAsync(List<FileHierarchyItem> hierarchy)
    {
        try
        {
            var filePath = Path.Combine(_basePath, _hierarchyFile);
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            
            var json = JsonSerializer.Serialize(hierarchy, options);
            await File.WriteAllTextAsync(filePath, json);
            _cachedHierarchy = hierarchy;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving file hierarchy: {ex.Message}");
        }
    }

    private List<FileHierarchyItem> CreateSampleHierarchy()
    {
        return new List<FileHierarchyItem>
        {
            new FileHierarchyItem
            {
                Name = "Projects",
                Path = "Projects",
                Type = "folder",
                IsFile = false,
                DateModified = DateTime.Now,
                Children = new List<FileHierarchyItem>
                {
                    new FileHierarchyItem
                    {
                        Name = "Documents",
                        Path = "Projects/Documents",
                        Type = "folder",
                        IsFile = false,
                        DateModified = DateTime.Now,
                        Children = new List<FileHierarchyItem>
                        {
                            new FileHierarchyItem
                            {
                                Name = "readme.txt",
                                Path = "Projects/Documents/readme.txt",
                                Type = "file",
                                IsFile = true,
                                Size = 1024,
                                DateModified = DateTime.Now
                            },
                            new FileHierarchyItem
                            {
                                Name = "notes.md",
                                Path = "Projects/Documents/notes.md",
                                Type = "file",
                                IsFile = true,
                                Size = 2048,
                                DateModified = DateTime.Now
                            }
                        }
                    },
                    new FileHierarchyItem
                    {
                        Name = "config.json",
                        Path = "Projects/config.json",
                        Type = "file",
                        IsFile = true,
                        Size = 512,
                        DateModified = DateTime.Now
                    }
                }
            },
            new FileHierarchyItem
            {
                Name = "Resources",
                Path = "Resources",
                Type = "folder",
                IsFile = false,
                DateModified = DateTime.Now,
                Children = new List<FileHierarchyItem>
                {
                    new FileHierarchyItem
                    {
                        Name = "styles.css",
                        Path = "Resources/styles.css",
                        Type = "file",
                        IsFile = true,
                        Size = 1536,
                        DateModified = DateTime.Now
                    }
                }
            }
        };
    }

    public void ClearCache()
    {
        _cachedHierarchy = null;
    }

    private Task<List<FileHierarchyItem>> ParseCurrentJsonStructure(string json)
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            
            var jsonDocument = JsonDocument.Parse(json);
            var root = jsonDocument.RootElement;
            
            var result = new List<FileHierarchyItem>();
            
            foreach (var property in root.EnumerateObject())
            {
                var categoryItem = new FileHierarchyItem
                {
                    Name = property.Name,
                    Path = property.Name,
                    Type = "folder",
                    IsFile = false,
                    DateModified = DateTime.Now,
                    Children = new List<FileHierarchyItem>()
                };
                
                if (property.Value.ValueKind == JsonValueKind.Object)
                {
                    ParseJsonObject(property.Value, categoryItem, property.Name);
                }
                
                result.Add(categoryItem);
            }
            
            return Task.FromResult(result);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error parsing JSON structure: {ex.Message}");
            return Task.FromResult(CreateSampleHierarchy());
        }
    }

    private void ParseJsonObject(JsonElement element, FileHierarchyItem parent, string basePath)
    {
        foreach (var property in element.EnumerateObject())
        {
            var currentPath = $"{basePath}/{property.Name}";
            
            if (property.Value.ValueKind == JsonValueKind.Object)
            {
                // This is a folder
                var folderItem = new FileHierarchyItem
                {
                    Name = property.Name,
                    Path = currentPath,
                    Type = "folder",
                    IsFile = false,
                    DateModified = DateTime.Now,
                    Children = new List<FileHierarchyItem>()
                };
                
                ParseJsonObject(property.Value, folderItem, currentPath);
                parent.Children.Add(folderItem);
            }
            else if (property.Value.ValueKind == JsonValueKind.Array)
            {
                // This is a folder containing files
                var folderItem = new FileHierarchyItem
                {
                    Name = property.Name,
                    Path = currentPath,
                    Type = "folder",
                    IsFile = false,
                    DateModified = DateTime.Now,
                    Children = new List<FileHierarchyItem>()
                };
                
                foreach (var fileElement in property.Value.EnumerateArray())
                {
                    if (fileElement.ValueKind == JsonValueKind.String)
                    {
                        var fileName = fileElement.GetString();
                        if (!string.IsNullOrEmpty(fileName))
                        {
                            var fileItem = new FileHierarchyItem
                            {
                                Name = fileName,
                                Path = Path.Combine(_basePath, fileName),
                                Type = "file",
                                IsFile = true,
                                Size = 0,
                                DateModified = DateTime.Now
                            };
                            folderItem.Children.Add(fileItem);
                        }
                    }
                }
                
                parent.Children.Add(folderItem);
            }
        }
    }
}