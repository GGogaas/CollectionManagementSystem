using CollectionManagementSystem.ViewModels;

namespace CollectionManagementSystem.Views;

public partial class AddCollectionPage : ContentPage
{
    public AddCollectionPage(AddCollectionViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
