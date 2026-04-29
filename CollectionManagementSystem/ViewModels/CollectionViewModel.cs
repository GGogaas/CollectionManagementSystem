using CollectionManagementSystem.Helpers;
using CollectionManagementSystem.Models;
using CollectionManagementSystem.Presentation;
using CollectionManagementSystem.Services;
using System.Collections.ObjectModel;

namespace CollectionManagementSystem.ViewModels;

[QueryProperty(nameof(CollectionId), "collectionId")]
public class CollectionViewModel : BaseViewModel
{
    private readonly FileStorageService _storage;
    private Collection? _collection;
    private string _collectionId = string.Empty;

    public ObservableCollection<CollectionItemDisplay> Items { get; } = new();

    private string _title = string.Empty;
    public string Title { get => _title; set => SetProperty(ref _title, value); }

    private string _subtitle = string.Empty;
    public string Subtitle { get => _subtitle; set => SetProperty(ref _subtitle, value); }

    public string CollectionId
    {
        get => _collectionId;
        set { _collectionId = value; if (Guid.TryParse(value, out var id)) LoadCollection(id); }
    }

    public RelayCommand AddItemCommand { get; }
    public RelayCommand<CollectionItemDisplay> EditItemCommand { get; }
    public RelayCommand<CollectionItemDisplay> DeleteItemCommand { get; }
    public RelayCommand AddCustomFieldCommand { get; }
    public RelayCommand ExportCommand { get; }
    public RelayCommand ImportCommand { get; }
    public RelayCommand SummaryCommand { get; }

    public CollectionViewModel(FileStorageService storage)
    {
        _storage = storage;
        AddItemCommand = new RelayCommand(AddItem);
        EditItemCommand = new RelayCommand<CollectionItemDisplay>(EditItem);
        DeleteItemCommand = new RelayCommand<CollectionItemDisplay>(DeleteItem);
        AddCustomFieldCommand = new RelayCommand(AddCustomField);
        ExportCommand = new RelayCommand(ExportCollection);
        ImportCommand = new RelayCommand(ImportCollection);
        SummaryCommand = new RelayCommand(OpenSummary);
    }

    private void LoadCollection(Guid id)
    {
        _collection = _storage.LoadAllCollections().FirstOrDefault(c => c.Id == id);
        if (_collection == null) return;
        Title = _collection.Name;
        Subtitle = _collection.CollectionType;
        RefreshItems();
    }

    public void Reload()
    {
        if (_collection == null) return;
        _collection = _storage.LoadAllCollections().FirstOrDefault(c => c.Id == _collection.Id);
        if (_collection == null) return;
        Title = _collection.Name;
        Subtitle = _collection.CollectionType;
        RefreshItems();
    }

    public void RefreshItems()
    {
        if (_collection == null) return;
        Items.Clear();
        var sorted = _collection.Items
            .OrderBy(i => i.Status == ItemStatus.Sold ? 1 : 0)
            .ThenBy(i => i.Name);
        foreach (var item in sorted)
            Items.Add(new CollectionItemDisplay(item, _storage));
    }

    public async void EditItem(CollectionItemDisplay? itemVm)
    {
        if (itemVm == null || _collection == null) return;
        await Shell.Current.GoToAsync($"item?collectionId={_collection.Id}&itemId={itemVm.Id}");
    }

    private async void AddItem()
    {
        if (_collection == null) return;
        await Shell.Current.GoToAsync($"item?collectionId={_collection.Id}&itemId=");
    }

    public async void DeleteItem(CollectionItemDisplay? itemVm)
    {
        if (itemVm == null || _collection == null) return;
        bool confirm = await Shell.Current.DisplayAlert("Delete Item",
            $"Delete \"{itemVm.Name}\"?", "Delete", "Cancel");
        if (!confirm) return;
        _collection.Items.RemoveAll(i => i.Id == itemVm.Id);
        _storage.SaveCollection(_collection);
        RefreshItems();
    }

    private async void AddCustomField()
    {
        if (_collection == null) return;
        var name = await Shell.Current.DisplayPromptAsync("Add Custom Column",
            "Enter column name:", "OK", "Cancel", "e.g. Publisher, Edition...");
        if (string.IsNullOrWhiteSpace(name)) return;

        if (_collection.CustomFields.Any(f => f.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
        {
            await Shell.Current.DisplayAlert("Error", "A column with this name already exists.", "OK");
            return;
        }

        var typeStr = await Shell.Current.DisplayActionSheet("Column type:", "Cancel", null,
            "Text", "Number", "Dropdown");
        if (typeStr is null or "Cancel") return;

        var fieldType = typeStr switch
        {
            "Number" => CustomFieldType.Number,
            "Dropdown" => CustomFieldType.Dropdown,
            _ => CustomFieldType.Text
        };

        var field = new CustomField { Name = name.Trim(), FieldType = fieldType };

        if (fieldType == CustomFieldType.Dropdown)
        {
            var optStr = await Shell.Current.DisplayPromptAsync("Dropdown Options",
                "Enter options separated by commas:", "OK", "Cancel");
            if (!string.IsNullOrWhiteSpace(optStr))
                field.DropdownOptions = optStr.Split(',')
                    .Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToList();
        }

        _collection.CustomFields.Add(field);
        _storage.SaveCollection(_collection);
        RefreshItems();
    }

    private async void ExportCollection()
    {
        if (_collection == null) return;
        var exportDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "CollectionExports");
        Directory.CreateDirectory(exportDir);
        var exportPath = Path.Combine(exportDir, $"{_collection.Name}_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
        _storage.ExportCollection(_collection, exportPath);
        await Shell.Current.DisplayAlert("Export Complete", $"Exported to:\n{exportPath}", "OK");
    }

    private async void ImportCollection()
    {
        if (_collection == null) return;
        var result = await FilePicker.PickAsync(new PickOptions
        {
            PickerTitle = "Select collection file to import",
            FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                [DevicePlatform.WinUI] = [".txt"],
                [DevicePlatform.macOS] = ["txt"],
                [DevicePlatform.iOS] = ["public.text"],
                [DevicePlatform.Android] = ["text/plain"]
            })
        });
        if (result == null) return;

        var imported = _storage.ImportCollection(result.FullPath);
        if (imported == null)
        {
            await Shell.Current.DisplayAlert("Error", "Could not read the selected file.", "OK");
            return;
        }

        int added = 0;
        foreach (var item in imported.Items)
        {
            if (!_collection.Items.Any(i => i.Id == item.Id))
            {
                _collection.Items.Add(item);
                added++;
            }
        }

        foreach (var field in imported.CustomFields)
        {
            if (!_collection.CustomFields.Any(f => f.Name.Equals(field.Name, StringComparison.OrdinalIgnoreCase)))
                _collection.CustomFields.Add(field);
        }

        _storage.SaveCollection(_collection);
        RefreshItems();
        await Shell.Current.DisplayAlert("Import Complete", $"Added {added} new item(s).", "OK");
    }

    private async void OpenSummary()
    {
        if (_collection == null) return;
        await Shell.Current.GoToAsync($"summary?collectionId={_collection.Id}");
    }
}
