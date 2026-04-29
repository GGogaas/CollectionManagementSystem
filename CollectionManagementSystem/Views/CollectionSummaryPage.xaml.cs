using CollectionManagementSystem.ViewModels;

namespace CollectionManagementSystem.Views;

public partial class CollectionSummaryPage : ContentPage
{
    public CollectionSummaryPage(CollectionSummaryViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
