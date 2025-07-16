using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Graphics;
using Librarian.FileViewer.Models;
using Librarian.FileViewer.Services;
using Syncfusion.Maui.TreeView;

namespace Librarian.FileViewer.Components.Pages
{
    public partial class FileViewer : ContentPage
    {
        private readonly FileHierarchyService _hierarchyService;
        private readonly FileContentService _contentService;
        private List<FileHierarchyItem> _hierarchyItems;
        private string _selectedFilePath = string.Empty;

        public List<FileHierarchyItem> HierarchyItems => _hierarchyItems ?? new List<FileHierarchyItem>();



        public FileViewer()
        {
            InitializeComponent();
            _hierarchyService = new FileHierarchyService();
            _contentService = new FileContentService();
            LoadHierarchyAsync();
        }

        private async void LoadHierarchyAsync()
        {
            try
            {
                _hierarchyItems = await _hierarchyService.GetFileHierarchyAsync();
                TreeItemsControl.ItemsSource = _hierarchyItems;
                
                // Debug info
                System.Diagnostics.Debug.WriteLine($"Loaded {_hierarchyItems?.Count ?? 0} items");
                if (_hierarchyItems != null && _hierarchyItems.Any())
                {
                    System.Diagnostics.Debug.WriteLine($"First item: {_hierarchyItems.First().Name}");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error loading hierarchy: {ex.Message}", "OK");
            }
        }

        private async void TreeView_NodeTapped(object sender, SelectionChangedEventArgs e)
        {
            // Handle node tapping
            if (e.CurrentSelection.FirstOrDefault() is FileHierarchyItem item)
            {
                if (!item.IsFile) return;

                _selectedFilePath = item.Path;
                UpdateFileInfo(item);
                await LoadFileContentAsync(item);
            }
        }

        private void TreeView_NodeExpanded(object sender, EventArgs e)
        {
            // Handle node expansion if needed
        }

        private void UpdateFileInfo(FileHierarchyItem item)
        {
            FilePathTextBlock.Text = item.Path;
            FileTypeTextBlock.Text = _contentService.GetFileType(item.Path).ToUpperInvariant();
        }

        private async Task LoadFileContentAsync(FileHierarchyItem item)
        {
            ShowLoadingIndicator();

            try
            {
                var fileContent = await _contentService.GetFileContentAsync(item.Path);
                var fileType = _contentService.GetFileType(item.Path);

                await Dispatcher.DispatchAsync(() =>
                {
                    HideAllContentPanels();
                    DisplayFileContent(fileType, fileContent, item.Path);
                });
            }
            catch (Exception ex)
            {
                await Dispatcher.DispatchAsync(() =>
                {
                    HideAllContentPanels();
                    ShowError($"Error loading file: {ex.Message}");
                });
            }
        }

        private void ShowLoadingIndicator()
        {
            HideAllContentPanels();
            LoadingGrid.IsVisible = true;
        }

        private void HideAllContentPanels()
        {
            LoadingGrid.IsVisible = false;
            NoSelectionGrid.IsVisible = false;
            PdfWebBrowser.IsVisible = false;
            TextContentScrollViewer.IsVisible = false;
            ImageContentScrollViewer.IsVisible = false;
            BinaryContentScrollViewer.IsVisible = false;
        }

        private void DisplayFileContent(string fileType, string content, string filePath)
        {
            switch (fileType)
            {
                case "pdf":
                    DisplayPdfContent(filePath);
                    break;
                case "text":
                case "json":
                case "markdown":
                    DisplayTextContent(content);
                    break;
                case "image":
                    DisplayImageContent(content, filePath);
                    break;
                default:
                    DisplayBinaryContent(content);
                    break;
            }
        }

        private void DisplayPdfContent(string filePath)
        {
            var fullPath = GetFullFilePath(filePath);
            if (File.Exists(fullPath))
            {
                var uri = new Uri($"file:///{fullPath.Replace('\\', '/')}");
                PdfWebBrowser.Source = uri;
                PdfWebBrowser.IsVisible = true;
            }
            else
            {
                ShowError($"PDF file not found: {fullPath}");
            }
        }

        private void DisplayTextContent(string content)
        {
            TextContentTextBox.Text = content;
            TextContentScrollViewer.IsVisible = true;
        }

        private void DisplayImageContent(string content, string filePath)
        {
            ImageInfoTextBlock.Text = content;
            // Note: For actual image display, you'd need to load the image file
            // This is a simplified version showing image info
            ImageContentScrollViewer.IsVisible = true;
        }

        private void DisplayBinaryContent(string content)
        {
            BinaryInfoTextBlock.Text = content;
            BinaryContentScrollViewer.IsVisible = true;
        }

        private void ShowError(string message)
        {
            HideAllContentPanels();
            NoSelectionGrid.IsVisible = true;
            var label = NoSelectionGrid.Children.OfType<Label>().FirstOrDefault();
            if (label != null)
            {
                label.Text = message;
                label.TextColor = Colors.Red;
            }
        }

        private string GetFullFilePath(string filePath)
        {
            var basePath = @"C:\Projects\BlazorProjects\Librarian\Crawl";
            return Path.Combine(basePath, filePath);
        }

        private async void RefreshButton_Click(object sender, EventArgs e)
        {
            _hierarchyService.ClearCache();
            await Task.Run(() => LoadHierarchyAsync());
        }
    }

    // Converters
    public class IconConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length >= 2 && values[0] is bool isFile && values[1] is bool isExpanded)
            {
                if (isFile)
                    return "📄"; // File icon
                else
                    return isExpanded ? "📂" : "📁"; // Folder icons
            }
            return "📄";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ToggleIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isExpanded)
            {
                return isExpanded ? "▼" : "▶";
            }
            return "▶";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ToggleVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isFile)
            {
                return isFile ? Visibility.Collapsed : Visibility.Visible;
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ChildrenVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isExpanded)
            {
                return isExpanded ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class FileTypeDisplayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string fileType)
            {
                return fileType.ToUpperInvariant();
            }
            return "UNKNOWN";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 