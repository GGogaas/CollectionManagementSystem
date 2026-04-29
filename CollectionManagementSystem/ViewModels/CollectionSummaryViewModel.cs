using CollectionManagementSystem.Helpers;
using CollectionManagementSystem.Models;
using CollectionManagementSystem.Services;

namespace CollectionManagementSystem.ViewModels;

[QueryProperty(nameof(CollectionId), "collectionId")]
public class CollectionSummaryViewModel : BaseViewModel
{
    private readonly FileStorageService _storage;

    private string _collectionId = string.Empty;
    public string CollectionId
    {
        get => _collectionId;
        set { _collectionId = value; LoadSummary(); }
    }

    private string _collectionName = string.Empty;
    public string CollectionName { get => _collectionName; set => SetProperty(ref _collectionName, value); }

    private string _collectionType = string.Empty;
    public string CollectionType { get => _collectionType; set => SetProperty(ref _collectionType, value); }

    private int _totalItems;
    public int TotalItems { get => _totalItems; set => SetProperty(ref _totalItems, value); }

    private int _ownedItems;
    public int OwnedItems { get => _ownedItems; set => SetProperty(ref _ownedItems, value); }

    private int _soldItems;
    public int SoldItems { get => _soldItems; set => SetProperty(ref _soldItems, value); }

    private int _wantToBuyItems;
    public int WantToBuyItems { get => _wantToBuyItems; set => SetProperty(ref _wantToBuyItems, value); }

    private int _forSaleItems;
    public int ForSaleItems { get => _forSaleItems; set => SetProperty(ref _forSaleItems, value); }

    private decimal _totalValue;
    public decimal TotalValue { get => _totalValue; set => SetProperty(ref _totalValue, value); }

    private string _averageRating = "—";
    public string AverageRating { get => _averageRating; set => SetProperty(ref _averageRating, value); }

    public RelayCommand BackCommand { get; }

    public CollectionSummaryViewModel(FileStorageService storage)
    {
        _storage = storage;
        BackCommand = new RelayCommand(async () => await Shell.Current.GoToAsync(".."));
    }

    private void LoadSummary()
    {
        if (!Guid.TryParse(_collectionId, out var id)) return;
        var collection = _storage.LoadAllCollections().FirstOrDefault(c => c.Id == id);
        if (collection == null) return;

        CollectionName = collection.Name;
        CollectionType = collection.CollectionType;
        TotalItems = collection.Items.Count;
        OwnedItems = collection.Items.Count(i => i.Status is ItemStatus.New or ItemStatus.Used);
        SoldItems = collection.Items.Count(i => i.Status == ItemStatus.Sold);
        WantToBuyItems = collection.Items.Count(i => i.Status == ItemStatus.WantToBuy);
        ForSaleItems = collection.Items.Count(i => i.Status == ItemStatus.ForSale);
        TotalValue = collection.Items
            .Where(i => i.Status is not ItemStatus.Sold)
            .Sum(i => i.Price);
        AverageRating = collection.Items.Count > 0
            ? $"{collection.Items.Average(i => i.Rating):F1}/10"
            : "—";
    }
}
