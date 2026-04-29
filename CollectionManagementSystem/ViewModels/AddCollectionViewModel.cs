using CollectionManagementSystem.Helpers;
using CollectionManagementSystem.Models;
using CollectionManagementSystem.Services;

namespace CollectionManagementSystem.ViewModels;

public class AddCollectionViewModel : BaseViewModel
{
    private readonly FileStorageService _storage;

    private string _name = string.Empty;
    public string Name { get => _name; set => SetProperty(ref _name, value); }

    private string _selectedType = string.Empty;
    public string SelectedType { get => _selectedType; set => SetProperty(ref _selectedType, value); }

    private string _description = string.Empty;
    public string Description { get => _description; set => SetProperty(ref _description, value); }

    public List<string> CollectionTypes { get; } =
    [
        "Books", "Console Games", "Board Games", "LEGO Sets",
        "TCG Cards", "Music Albums", "Movies", "Comics",
        "Action Figures", "Stamps", "Coins", "Other"
    ];

    public RelayCommand SaveCommand { get; }
    public RelayCommand CancelCommand { get; }

    public AddCollectionViewModel(FileStorageService storage)
    {
        _storage = storage;
        SelectedType = CollectionTypes[0];
        SaveCommand = new RelayCommand(Save);
        CancelCommand = new RelayCommand(Cancel);
    }

    private async void Save()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            await Shell.Current.DisplayAlert("Validation", "Collection name is required.", "OK");
            return;
        }

        _storage.SaveCollection(new Collection
        {
            Name = Name.Trim(),
            CollectionType = SelectedType,
            Description = Description.Trim()
        });

        await Shell.Current.GoToAsync("..");
    }

    private async void Cancel() => await Shell.Current.GoToAsync("..");
}
