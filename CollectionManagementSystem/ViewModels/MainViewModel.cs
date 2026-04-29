using CollectionManagementSystem.Helpers;
using CollectionManagementSystem.Models;
using CollectionManagementSystem.Services;
using System.Collections.ObjectModel;

namespace CollectionManagementSystem.ViewModels;

public class MainViewModel : BaseViewModel
{
    private readonly FileStorageService _storage;

    public ObservableCollection<Collection> Collections { get; } = new();

    public RelayCommand AddCollectionCommand { get; }
    public RelayCommand<Collection> OpenCollectionCommand { get; }
    public RelayCommand<Collection> DeleteCollectionCommand { get; }

    public MainViewModel(FileStorageService storage)
    {
        _storage = storage;
        AddCollectionCommand = new RelayCommand(AddCollection);
        OpenCollectionCommand = new RelayCommand<Collection>(OpenCollection);
        DeleteCollectionCommand = new RelayCommand<Collection>(DeleteCollection);
    }

    public void LoadCollections()
    {
        Collections.Clear();
        foreach (var c in _storage.LoadAllCollections())
            Collections.Add(c);
    }

    private async void AddCollection()
        => await Shell.Current.GoToAsync("addcollection");

    public async void OpenCollection(Collection? collection)
    {
        if (collection == null) return;
        await Shell.Current.GoToAsync($"collection?collectionId={collection.Id}");
    }

    public async void DeleteCollection(Collection? collection)
    {
        if (collection == null) return;
        bool confirm = await Shell.Current.DisplayAlert("Delete Collection",
            $"Delete \"{collection.Name}\"? This cannot be undone.", "Delete", "Cancel");
        if (!confirm) return;
        _storage.DeleteCollection(collection.Id);
        Collections.Remove(collection);
    }
}
