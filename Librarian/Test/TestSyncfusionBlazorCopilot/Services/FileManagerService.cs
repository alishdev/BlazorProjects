using Syncfusion.Blazor.FileManager;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading.Tasks;
using System.Text.Json;
using System.IO;

namespace TestSyncfusionBlazorCopilot.Services
{
    public class FileManagerService
    {
        public List<FileManagerDirectoryContent> CopyFiles = new List<FileManagerDirectoryContent>();
        public List<FileManagerDirectoryContent> Data = new List<FileManagerDirectoryContent>();
        public FileManagerService()
        {
            LoadFromJson();
        }
        private void LoadFromJson()
        {
            var jsonData = @"{
  ""Programs"": {
    ""Camps"": {
      ""Summer Camps"": [
        ""camps_dancelcamp.pdf"",
        ""camps_parkvillecamp.pdf"",
        ""camps_catonsvillecamp.pdf"",
        ""camps_wardcamp.pdf"",
        ""camps_hillcamp.pdf"",
        ""camps_pasadenacamp.pdf"",
        ""programs_generalinfoforms.pdf""
      ],
      ""Indoor Camps"": [
        ""camps_highlandscamp.pdf""
      ],
      ""Day & Overnight Camp"": [
        ""camps_camphashawha.pdf""
      ],
      ""Camp Whippoorwill"": [
        ""camps_campwhippoorwill.pdf""
      ],
      ""Day and Overnight Camps"": [
        ""camps_puhtok.pdf""
      ],
      ""2025 Camp Locations"": [
        ""programs_camplocations.pdf""
      ],
      ""Overnight Camps"": [
        ""programs_overnightcamps.pdf""
      ],
      ""Teen Adventure Day Camp"": [
        ""programs_teenleadershipcamps.pdf""
      ],
      ""Specialty Day Camps"": [
        ""programs_specialtycamps.pdf""
      ],
      ""FAQs"": [
        ""programs_ycampfaqs.pdf""
      ],
      ""Outdoor Adventure Day Camps"": [
        ""programs_outdooradventurecamps.pdf""
      ],
      ""Day Camps"": [
        ""programs_traditionaldaycamps.pdf""
      ]
    },
    ""Community Schools"": {
      ""Moravia Park Elementary"": [
        ""locations_commoost_moraviapark.pdf""
      ],
      ""Fort Worthington Elementary/Middle"": [
        ""locations_commoost_fortworthington.pdf""
      ],
      ""Graceland Park-O'Donnell Heights"": [
        ""locations_commoost_gracelandparkodonnellheights.pdf""
      ],
      ""Waverly Elementary"": [
        ""locations_commoost_waverly.pdf""
      ]
    },
    ""Before & After School Enrichment"": {
      ""Harford County Locations"": [
        ""area_harford-county.pdf""
      ],
      ""Mechanicsville Elementary"": [
        ""locations_beforeafterschoolenrichment_mechanicsville.pdf""
      ],
      ""Monarch Global Academy"": [
        ""locations_beforeafterschoolenrichment_monarchglobal.pdf""
      ]
    }
  }
}";
            var root = JsonDocument.Parse(jsonData).RootElement;
            int idCounter = 1;
            void ParseJson(JsonElement element, string parentId, string parentName)
            {
                if (element.ValueKind == JsonValueKind.Object)
                {
                    foreach (var prop in element.EnumerateObject())
                    {
                        string folderId = (idCounter++).ToString();
                        Data.Add(new FileManagerDirectoryContent
                        {
                            Id = folderId,
                            Name = prop.Name,
                            ParentId = parentId,
                            IsFile = false,
                            HasChild = true,
                            Type = "folder"
                        });
                        ParseJson(prop.Value, folderId, prop.Name);
                    }
                }
                else if (element.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in element.EnumerateArray())
                    {
                        string fileId = (idCounter++).ToString();
                        Data.Add(new FileManagerDirectoryContent
                        {
                            Id = fileId,
                            Name = item.GetString(),
                            ParentId = parentId,
                            IsFile = true,
                            HasChild = false,
                            Type = "file"
                        });
                    }
                }
            }
            ParseJson(root, null, "Root");
        }
        public async Task<FileManagerResponse<FileManagerDirectoryContent>> ReadAsync(string path, List<FileManagerDirectoryContent> fileDetails)
        {
            var response = new FileManagerResponse<FileManagerDirectoryContent>();
            string parentId;
            if (string.IsNullOrEmpty(path) || path == "/")
            {
                // Find the top-level folder (first folder with ParentId == null)
                var rootFolder = Data.FirstOrDefault(x => x.ParentId == null && !x.IsFile);
                parentId = rootFolder?.Id;
                response.CWD = rootFolder;
                response.Files = Data.Where(x => x.ParentId == parentId).ToList();
            }
            else
            {
                parentId = Data.FirstOrDefault(x => x.Name == path || x.Id == path)?.Id;
                response.CWD = Data.FirstOrDefault(x => x.Id == parentId);
                response.Files = Data.Where(x => x.ParentId == parentId).ToList();
            }
            await Task.Yield();
            return response;
        }
        public async Task<FileManagerResponse<FileManagerDirectoryContent>> DeleteAsync(string path, List<FileManagerDirectoryContent> fileDetails)
        {
            var response = new FileManagerResponse<FileManagerDirectoryContent>();
            var idsToDelete = fileDetails.Select(x => x.Id).ToList();
            Data.RemoveAll(x => idsToDelete.Contains(x.Id));
            response.Files = fileDetails;
            await Task.Yield();
            return response;
        }
        public async Task<FileManagerResponse<FileManagerDirectoryContent>> CreateAsync(string path, string name, FileManagerDirectoryContent parentFolder)
        {
            var response = new FileManagerResponse<FileManagerDirectoryContent>();
            var newFolder = new FileManagerDirectoryContent { Id = Guid.NewGuid().ToString(), Name = name, ParentId = parentFolder.Id, IsFile = false, HasChild = false, Type = "folder" };
            Data.Add(newFolder);
            response.Files = new List<FileManagerDirectoryContent> { newFolder };
            await Task.Yield();
            return response;
        }
        public async Task<FileManagerResponse<FileManagerDirectoryContent>> SearchAsync(string path, string searchText)
        {
            var response = new FileManagerResponse<FileManagerDirectoryContent>();
            response.Files = Data.Where(x => x.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase)).ToList();
            await Task.Yield();
            return response;
        }
        public async Task<FileManagerResponse<FileManagerDirectoryContent>> RenameAsync(string path, string newName, FileManagerDirectoryContent file)
        {
            var response = new FileManagerResponse<FileManagerDirectoryContent>();
            var item = Data.FirstOrDefault(x => x.Id == file.Id);
            if (item != null) item.Name = newName;
            response.Files = new List<FileManagerDirectoryContent> { item };
            await Task.Yield();
            return response;
        }
        public async Task<FileManagerResponse<FileManagerDirectoryContent>> MoveAsync(string path, FileManagerDirectoryContent targetData, List<FileManagerDirectoryContent> files)
        {
            var response = new FileManagerResponse<FileManagerDirectoryContent>();
            foreach (var file in files)
            {
                var item = Data.FirstOrDefault(x => x.Id == file.Id);
                if (item != null) item.ParentId = targetData.Id;
            }
            response.Files = files;
            await Task.Yield();
            return response;
        }
        public async Task<FileManagerResponse<FileManagerDirectoryContent>> CopyAsync(string path, FileManagerDirectoryContent targetData, string[] renameFiles, List<FileManagerDirectoryContent> files)
        {
            var response = new FileManagerResponse<FileManagerDirectoryContent>();
            var copied = new List<FileManagerDirectoryContent>();
            foreach (var file in files)
            {
                var copy = new FileManagerDirectoryContent
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = file.Name,
                    ParentId = targetData.Id,
                    IsFile = file.IsFile,
                    HasChild = file.HasChild,
                    Type = file.Type
                };
                copied.Add(copy);
                Data.Add(copy);
            }
            response.Files = copied;
            await Task.Yield();
            return response;
        }
    }
}
