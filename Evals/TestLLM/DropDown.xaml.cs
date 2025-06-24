using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace TestLLM;

public partial class DropDown : ContentView
{
    public static readonly BindableProperty ItemsSourceProperty =
        BindableProperty.Create(nameof(ItemsSource), typeof(ObservableCollection<string>), typeof(DropDown), new ObservableCollection<string>(), propertyChanged: OnItemsSourceChanged);

    public static readonly BindableProperty SelectedItemProperty =
        BindableProperty.Create(nameof(SelectedItem), typeof(string), typeof(DropDown), default(string), BindingMode.TwoWay, propertyChanged: OnSelectedItemChanged);

    public ObservableCollection<string> ItemsSource
    {
        get => (ObservableCollection<string>)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public string SelectedItem
    {
        get => (string)GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    public event EventHandler<string>? SelectionChanged;

    public DropDown()
    {
        InitializeComponent();
        // Set default
        if (string.IsNullOrEmpty(SelectedItem) && ItemsSource.Count > 0)
        {
            SelectedItem = ItemsSource[0];
        }
        SelectedValueLabel.Text = SelectedItem ?? string.Empty;
    }

    private static void OnItemsSourceChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (DropDown)bindable;
        if (newValue is ObservableCollection<string> newList && newList.Count > 0)
        {
            control.DropdownOptions.ItemsSource = newList;
            if (string.IsNullOrEmpty(control.SelectedItem))
            {
                control.SelectedItem = newList[0];
            }
            control.SelectedValueLabel.Text = control.SelectedItem;
        }
    }

    private static void OnSelectedItemChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (DropDown)bindable;
        if (newValue is string selected)
        {
            control.SelectedValueLabel.Text = selected;
        }
    }

    private void OnDropdownClicked(object? sender, EventArgs e)
    {
        DropdownOptionsFrame.IsVisible = !DropdownOptionsFrame.IsVisible;
    }

    private void OnDropdownOptionSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is string selectedValue)
        {
            SelectedItem = selectedValue;
            SelectedValueLabel.Text = selectedValue;
            DropdownOptionsFrame.IsVisible = false;
            SelectionChanged?.Invoke(this, selectedValue);
        }
    }
} 