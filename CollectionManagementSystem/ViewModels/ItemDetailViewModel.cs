using CollectionManagementSystem.Helpers;
using CollectionManagementSystem.Models;
using CollectionManagementSystem.Services;
using System.Collections.ObjectModel;
using System.Globalization;

namespace CollectionManagementSystem.ViewModels;

[QueryProperty(nameof(CollectionId), "collectionId")]
[QueryProperty(nameof(ItemId), "itemId")]
public class ItemDetailViewModel : BaseViewModel
{
    private readonly FileStorageService _storage;
    private Collection? _collection;
    private CollectionItem? _item;
    private bool _isNewItem;
    private bool _collectionIdSet;
    private bool _itemIdSet;

    private string _collectionId = string.Empty;
    public string CollectionId
    {
        get => _collectionId;
        set { _collectionId = value; _collectionIdSet = true; TryLoad(); }
    }

    private string _itemId = string.Empty;
    public string ItemId
    {
        get => _itemId;
        set { _itemId = value; _itemIdSet = true; TryLoad(); }
    }

    private string _name = string.Empty;
    public string Name { get => _name; set => SetProperty(ref _name, value); }

    private string _priceText = "0.00";
    public string PriceText { get => _priceText; set => SetProperty(ref _priceText, value); }

    private int _statusIndex;
    public int StatusIndex { get => _statusIndex; set => SetProperty(ref _statusIndex, value); }

    private int _rating = 5;
    public int Rating { get => _rating; set => SetProperty(ref _rating, value); }

    private string _comment = string.Empty;
    public string Comment { get => _comment; set => SetProperty(ref _comment, value); }

    private string _imagePath = string.Empty;
    public string ImagePath
    {
        get => _imagePath;
        set
        {
            SetProperty(ref _imagePath, value);
            OnPropertyChanged(nameof(HasImage));
            OnPropertyChanged(nameof(ItemImageSource));
        }
    }

    private string _pageTitle = "Add Item";
    public string PageTitle { get => _pageTitle; set => SetProperty(ref _pageTitle, value); }

    public bool HasImage => !string.IsNullOrEmpty(_imagePath);
    public bool IsExistingItem => !_isNewItem;


    public ImageSource? ItemImageSource
    {
        get
        {
            if (string.IsNullOrEmpty(_imagePath)) return null;
            var path = _storage.GetFullImagePath(_imagePath);
            return File.Exists(path) ? ImageSource.FromFile(path) : null;
        }
    }

    public List<string> StatusOptions { get; } =
        ["New", "Used", "For Sale", "Sold", "Want to Buy"];

    public ObservableCollection<CustomFieldEditor> CustomFieldEditors { get; } = new();

    public RelayCommand SaveCommand { get; }
    public RelayCommand CancelCommand { get; }
    public RelayCommand DeleteItemCommand { get; }
    public RelayCommand PickImageCommand { get; }
    public RelayCommand RemoveImageCommand { get; }

    public ItemDetailViewModel(FileStorageService storage)
    {
        _storage = storage;
        SaveCommand = new RelayCommand(Save);
        CancelCommand = new RelayCommand(Cancel);
        DeleteItemCommand = new RelayCommand(DeleteItem);
        PickImageCommand = new RelayCommand(PickImage);
        RemoveImageCommand = new RelayCommand(() => ImagePath = string.Empty);
    }

    private void TryLoad()
    {
        if (!_collectionIdSet || !_itemIdSet) return;

        _collection = _storage.LoadAllCollections()
            .FirstOrDefault(c => c.Id.ToString() == _collectionId);
        if (_collection == null) return;

        if (!string.IsNullOrEmpty(_itemId) && Guid.TryParse(_itemId, out var itemGuid))
            _item = _collection.Items.FirstOrDefault(i => i.Id == itemGuid);

        _isNewItem = _item == null;
        _item ??= new CollectionItem();

        PageTitle = _isNewItem ? "Add Item" : "Edit Item";
        OnPropertyChanged(nameof(IsExistingItem));
        Name = _item.Name;
        PriceText = _item.Price == 0 ? string.Empty : _item.Price.ToString("F2", CultureInfo.InvariantCulture);
        StatusIndex = (int)_item.Status;
        Rating = _item.Rating;
        Comment = _item.Comment;
        ImagePath = _item.ImagePath;

        CustomFieldEditors.Clear();
        foreach (var field in _collection.CustomFields)
        {
            var val = _item.CustomFieldValues.TryGetValue(field.Name, out var v) ? v : string.Empty;
            CustomFieldEditors.Add(new CustomFieldEditor(field, val));
        }
    }

    private async void Save()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            await Shell.Current.DisplayAlert("Validation", "Item name is required.", "OK");
            return;
        }

        if (!decimal.TryParse(PriceText, NumberStyles.Any, CultureInfo.InvariantCulture, out var price))
            price = 0;

        var clampedRating = Math.Clamp(Rating, 1, 10);

        if (_isNewItem && _collection != null)
        {
            var duplicate = _collection.Items
                .FirstOrDefault(i => i.Name.Equals(Name.Trim(), StringComparison.OrdinalIgnoreCase));
            if (duplicate != null)
            {
                bool addAnyway = await Shell.Current.DisplayAlert("Duplicate Item",
                    $"An item named \"{Name}\" already exists. Add anyway?", "Add Anyway", "Cancel");
                if (!addAnyway) return;
            }
        }

        _item!.Name = Name.Trim();
        _item.Price = price;
        _item.Status = (ItemStatus)StatusIndex;
        _item.Rating = clampedRating;
        _item.Comment = Comment.Trim();
        _item.ImagePath = ImagePath;
        _item.CustomFieldValues.Clear();
        foreach (var editor in CustomFieldEditors)
            if (!string.IsNullOrEmpty(editor.Value))
                _item.CustomFieldValues[editor.FieldName] = editor.Value;

        if (_isNewItem)
            _collection!.Items.Add(_item);

        _storage.SaveCollection(_collection!);
        await Shell.Current.GoToAsync("..");
    }

    private async void Cancel() => await Shell.Current.GoToAsync("..");

    private async void DeleteItem()
    {
        if (_collection == null || _item == null || _isNewItem) return;
        bool confirm = await Shell.Current.DisplayAlert("Delete Item",
            $"Delete \"{_item.Name}\"? This cannot be undone.", "Delete", "Cancel");
        if (!confirm) return;
        _collection.Items.RemoveAll(i => i.Id == _item.Id);
        _storage.SaveCollection(_collection);
        await Shell.Current.GoToAsync("..");
    }

    private async void PickImage()
    {
        try
        {
            var result = await FilePicker.PickAsync(new PickOptions
            {
                PickerTitle = "Select image",
                FileTypes = FilePickerFileType.Images
            });
            if (result == null) return;
            var fileName = _storage.CopyImageToStorage(result.FullPath);
            if (fileName != null) ImagePath = fileName;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CMS] Image pick error: {ex.Message}");
        }
    }
}
