using CollectionManagementSystem.Helpers;
using CollectionManagementSystem.Models;
using CollectionManagementSystem.Services;

namespace CollectionManagementSystem.Presentation;


public class CollectionItemDisplay : BaseViewModel
{
    private readonly FileStorageService _storage;

    public CollectionItemDisplay(CollectionItem item, FileStorageService storage)
    {
        Item = item;
        _storage = storage;
    }

    public CollectionItem Item { get; }
    public Guid Id => Item.Id;
    public string Name => Item.Name;
    public decimal Price => Item.Price;
    public ItemStatus Status => Item.Status;
    public int Rating => Item.Rating;
    public string Comment => Item.Comment;
    public bool IsSold => Item.Status == ItemStatus.Sold;
    public double Opacity => IsSold ? 0.38 : 1.0;

    public string StatusText => Item.Status switch
    {
        ItemStatus.New => "New",
        ItemStatus.Used => "Used",
        ItemStatus.ForSale => "For Sale",
        ItemStatus.Sold => "Sold",
        ItemStatus.WantToBuy => "Want to Buy",
        _ => "Unknown"
    };

    public Color StatusColor => Item.Status switch
    {
        ItemStatus.New => Color.FromArgb("#10B981"),
        ItemStatus.Used => Color.FromArgb("#F59E0B"),
        ItemStatus.ForSale => Color.FromArgb("#2563EB"),
        ItemStatus.Sold => Color.FromArgb("#94A3B8"),
        ItemStatus.WantToBuy => Color.FromArgb("#8B5CF6"),
        _ => Color.FromArgb("#94A3B8")
    };

    public string RatingDisplay => $"{Item.Rating}/10";
    public string PriceText => Item.Price == 0 ? "—" : $"${Item.Price:F2}";

    public string CustomFieldsDisplay
    {
        get
        {
            if (Item.CustomFieldValues.Count == 0) return string.Empty;
            return string.Join("  •  ", Item.CustomFieldValues.Select(kv => $"{kv.Key}: {kv.Value}"));
        }
    }

    public bool HasImage => !string.IsNullOrEmpty(Item.ImagePath);
    public bool HasComment => !string.IsNullOrEmpty(Item.Comment);
    public bool HasCustomFields => Item.CustomFieldValues.Count > 0;

    public ImageSource? ImageSource
    {
        get
        {
            if (string.IsNullOrEmpty(Item.ImagePath)) return null;
            var path = _storage.GetFullImagePath(Item.ImagePath);
            return File.Exists(path) ? ImageSource.FromFile(path) : null;
        }
    }

    public void Refresh()
    {
        OnPropertyChanged(nameof(Name));
        OnPropertyChanged(nameof(Price));
        OnPropertyChanged(nameof(PriceText));
        OnPropertyChanged(nameof(Status));
        OnPropertyChanged(nameof(Rating));
        OnPropertyChanged(nameof(RatingDisplay));
        OnPropertyChanged(nameof(Comment));
        OnPropertyChanged(nameof(IsSold));
        OnPropertyChanged(nameof(Opacity));
        OnPropertyChanged(nameof(StatusText));
        OnPropertyChanged(nameof(StatusColor));
        OnPropertyChanged(nameof(CustomFieldsDisplay));
        OnPropertyChanged(nameof(HasImage));
        OnPropertyChanged(nameof(HasComment));
        OnPropertyChanged(nameof(HasCustomFields));
        OnPropertyChanged(nameof(ImageSource));
    }
}
