using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Librarian.FileViewer.Models;

namespace Librarian.FileViewer.Components
{
    public partial class FileTreeNode : ContentView, INotifyPropertyChanged
    {
        public static readonly BindableProperty ItemProperty =
            BindableProperty.Create(nameof(Item), typeof(FileHierarchyItem), typeof(FileTreeNode), 
                null, propertyChanged: OnItemChanged);

        public static readonly BindableProperty OnFileSelectedProperty =
            BindableProperty.Create(nameof(OnFileSelected), typeof(Action<FileHierarchyItem>), typeof(FileTreeNode), null);

        public FileHierarchyItem Item
        {
            get { return (FileHierarchyItem)GetValue(ItemProperty); }
            set { SetValue(ItemProperty, value); }
        }

        public Action<FileHierarchyItem> OnFileSelected
        {
            get { return (Action<FileHierarchyItem>)GetValue(OnFileSelectedProperty); }
            set { SetValue(OnFileSelectedProperty, value); }
        }

        private bool _isExpanded;
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                if (_isExpanded != value)
                {
                    _isExpanded = value;
                    OnPropertyChanged(nameof(IsExpanded));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public FileTreeNode()
        {
            InitializeComponent();
        }

        private static void OnItemChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var control = bindable as FileTreeNode;
            if (control != null && newValue is FileHierarchyItem item)
            {
                control.UpdateChildren();
            }
        }

        private void UpdateChildren()
        {
            if (Item?.Children != null && Item.Children.Any())
            {
                ChildrenItemsControl.ItemsSource = Item.Children;
            }
        }

        private void HeaderGrid_Tapped(object sender, TappedEventArgs e)
        {
            ToggleExpanded();
        }

        private void NameTextBlock_Tapped(object sender, TappedEventArgs e)
        {
            OnNodeClick(Item);
        }

        private void ToggleExpanded()
        {
            if (!Item.IsFile)
            {
                IsExpanded = !IsExpanded;
            }
        }

        private void OnNodeClick(FileHierarchyItem item)
        {
            if (item.IsFile)
            {
                OnFileSelected?.Invoke(item);
            }
            else
            {
                ToggleExpanded();
            }
        }

        private void Child_OnFileSelected(FileHierarchyItem item)
        {
            OnFileSelected?.Invoke(item);
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
                return isFile ? false : true;
            }
            return true;
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
                return isExpanded ? true : false;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 