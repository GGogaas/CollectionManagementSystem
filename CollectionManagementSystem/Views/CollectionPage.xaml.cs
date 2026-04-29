using CollectionManagementSystem.Presentation;
using CollectionManagementSystem.ViewModels;

namespace CollectionManagementSystem.Views;

public partial class CollectionPage : ContentPage
{
    private readonly CollectionViewModel _viewModel;

    public CollectionPage(CollectionViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.Reload();
    }

    private void OnItemTapped(object sender, TappedEventArgs e)
    {
        if (sender is VisualElement el && el.BindingContext is CollectionItemDisplay item)
            _viewModel.EditItem(item);
    }

    private void OnEditSwipeInvoked(object sender, EventArgs e)
    {
        if (sender is SwipeItem swipe && swipe.BindingContext is CollectionItemDisplay item)
            _viewModel.EditItem(item);
    }

    private void OnDeleteItemSwipeInvoked(object sender, EventArgs e)
    {
        if (sender is SwipeItem swipe && swipe.BindingContext is CollectionItemDisplay item)
            _viewModel.DeleteItem(item);
    }
}
