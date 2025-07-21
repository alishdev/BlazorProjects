using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
//File Manager's base functions are available in the below namespace.
using Syncfusion.EJ2.FileManager.Base;
//File Manager's operations are available in the below namespace.
using Syncfusion.EJ2.FileManager.PhysicalFileProvider;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
//using Microsoft.Extensions.FileProviders;
//using Syncfusion.Blazor.FileManager;
using SyncfusionBlazorManual.FileManagerService;

namespace SyncfusionBlazorManual.Controllers
{
    [Route("api/[controller]")]
    public class FileManagerController : Controller
    {
        public PhysicalFileProvider operation;
        public string basePath;
        string root = "wwwroot\\Files";
        [Obsolete]
        public FileManagerController(IWebHostEnvironment hostingEnvironment)
        {
            this.basePath = hostingEnvironment.ContentRootPath;
            this.operation = new PhysicalFileProvider();
            this.operation.RootFolder(this.basePath + "\\" + this.root); // It denotes in which files and folders are available.
        }

        // Processing the File Manager operations.
        [Route("FileOperations")]
        public object FileOperations([FromBody] FileManagerDirectoryContent args)
        {
            FileManagerService.FileManagerService service = new FileManagerService.FileManagerService();
            try
            {
                switch (args.Action)
                {
                    // Add your custom action here.
                    case "read":
                        // Path - Current path; ShowHiddenItems - Boolean value to show/hide hidden items.
                        //return this.operation.ToCamelCase(this.operation.GetFiles(null, args.ShowHiddenItems));
                        return this.operation.ToCamelCase(service.GetFiles(args.Path));
                    case "delete":
                        // Path - Current path where the folder to be deleted; Names - Name of the files to be deleted
                        return this.operation.ToCamelCase(this.operation.Delete(args.Path, args.Names));
                    case "copy":
                        //  Path - Path from where the file was copied; TargetPath - Path where the file/folder is to be copied; RenameFiles - Files with same name in the copied location that is confirmed for renaming; TargetData - Data of the copied file
                        return this.operation.ToCamelCase(this.operation.Copy(args.Path, args.TargetPath, args.Names, args.RenameFiles, args.TargetData));
                    case "move":
                        // Path - Path from where the file was cut; TargetPath - Path where the file/folder is to be moved; RenameFiles - Files with same name in the moved location that is confirmed for renaming; TargetData - Data of the moved file
                        return this.operation.ToCamelCase(this.operation.Move(args.Path, args.TargetPath, args.Names, args.RenameFiles, args.TargetData));
                    case "details":
                        // Path - Current path where details of file/folder is requested; Name - Names of the requested folders
                        return this.operation.ToCamelCase(this.operation.Details(args.Path, args.Names));
                    case "create":
                        // Path - Current path where the folder is to be created; Name - Name of the new folder
                        return this.operation.ToCamelCase(this.operation.Create(args.Path, args.Name));
                    case "search":
                        // Path - Current path where the search is performed; SearchString - String typed in the searchbox; CaseSensitive - Boolean value which specifies whether the search must be casesensitive
                        return this.operation.ToCamelCase(this.operation.Search(args.Path, args.SearchString, args.ShowHiddenItems, args.CaseSensitive));
                    case "rename":
                        // Path - Current path of the renamed file; Name - Old file name; NewName - New file name
                        return this.operation.ToCamelCase(this.operation.Rename(args.Path, args.Name, args.NewName));
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }
            
            return null;
        }
    }
}