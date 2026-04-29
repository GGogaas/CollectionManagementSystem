using CollectionManagementSystem.Models;
using CollectionManagementSystem.ViewModels;

namespace CollectionManagementSystem.Views;

public partial class ItemDetailPage : ContentPage
{
    private readonly ItemDetailViewModel _viewModel;

    public ItemDetailPage(ItemDetailViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ItemDetailViewModel.CustomFieldEditors))
            BuildCustomFieldEditors();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        BuildCustomFieldEditors();
    }

    private void BuildCustomFieldEditors()
    {
        CustomFieldsContainer.Children.Clear();
        if (_viewModel.CustomFieldEditors.Count == 0) return;

        var header = new Label
        {
            Text = "CUSTOM FIELDS",
            TextColor = Color.FromArgb("#64748B"),
            FontSize = 12,
            FontAttributes = FontAttributes.Bold
        };
        CustomFieldsContainer.Children.Add(header);

        foreach (var editor in _viewModel.CustomFieldEditors)
        {
            var fieldLabel = new Label
            {
                Text = editor.FieldName.ToUpper(),
                TextColor = Color.FromArgb("#64748B"),
                FontSize = 12,
                FontAttributes = FontAttributes.Bold,
                Margin = new Thickness(0, 4, 0, 4)
            };

            View inputView;

            if (editor.FieldType == CustomFieldType.Dropdown && editor.DropdownOptions.Count > 0)
            {
                var picker = new Picker
                {
                    TextColor = Color.FromArgb("#1E293B"),
                    FontSize = 15
                };
                picker.ItemsSource = editor.DropdownOptions;
                picker.SelectedIndex = editor.DropdownSelectedIndex;
                picker.SelectedIndexChanged += (s, e) =>
                {
                    if (picker.SelectedIndex >= 0)
                        editor.DropdownSelectedIndex = picker.SelectedIndex;
                };
                inputView = picker;
            }
            else
            {
                var entry = new Entry
                {
                    Text = editor.Value,
                    Placeholder = $"Enter {editor.FieldName.ToLower()}",
                    PlaceholderColor = Color.FromArgb("#94A3B8"),
                    TextColor = Color.FromArgb("#1E293B"),
                    FontSize = 15,
                    Keyboard = editor.FieldType == CustomFieldType.Number ? Keyboard.Numeric : Keyboard.Default
                };
                entry.TextChanged += (s, e) => editor.Value = e.NewTextValue ?? string.Empty;
                inputView = entry;
            }

            var border = new Border
            {
                Stroke = Color.FromArgb("#E2E8F0"),
                StrokeThickness = 1.5,
                Padding = new Thickness(12, 2),
                BackgroundColor = Colors.White
            };
            border.StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 8 };
            border.Content = inputView;

            var container = new VerticalStackLayout { Spacing = 4 };
            container.Children.Add(fieldLabel);
            container.Children.Add(border);
            CustomFieldsContainer.Children.Add(container);
        }
    }
}
