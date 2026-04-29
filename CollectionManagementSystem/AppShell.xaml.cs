using CollectionManagementSystem.Views;

namespace CollectionManagementSystem;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute("collection", typeof(CollectionPage));
        Routing.RegisterRoute("item", typeof(ItemDetailPage));
        Routing.RegisterRoute("addcollection", typeof(AddCollectionPage));
        Routing.RegisterRoute("summary", typeof(CollectionSummaryPage));
    }
}
