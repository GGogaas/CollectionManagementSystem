namespace CollectionManagementSystem.Models;

public class CollectionItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public ItemStatus Status { get; set; } = ItemStatus.New;
    public int Rating { get; set; } = 5;
    public string Comment { get; set; } = string.Empty;
    public string ImagePath { get; set; } = string.Empty;
    public Dictionary<string, string> CustomFieldValues { get; set; } = new();
}
