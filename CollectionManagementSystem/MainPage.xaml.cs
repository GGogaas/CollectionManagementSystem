using CollectionManagementSystem.Models;
using CollectionManagementSystem.ViewModels;

namespace CollectionManagementSystem;

public partial class MainPage : ContentPage
{
    private readonly MainViewModel _viewModel;

    public MainPage(MainViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.LoadCollections();
    }

    private void OnCollectionTapped(object sender, TappedEventArgs e)
    {
        if (sender is VisualElement element && element.BindingContext is Collection collection)
            _viewModel.OpenCollection(collection);
    }

    private void OnDeleteSwipeInvoked(object sender, EventArgs e)
    {
        if (sender is SwipeItem swipe && swipe.BindingContext is Collection collection)
            _viewModel.DeleteCollection(collection);
    }

    private void OnDeleteButtonClicked(object sender, EventArgs e)
    {
        if (sender is VisualElement el && el.BindingContext is Collection collection)
            _viewModel.DeleteCollection(collection);
    }
}
