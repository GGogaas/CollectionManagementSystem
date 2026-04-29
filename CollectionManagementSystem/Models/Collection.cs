namespace CollectionManagementSystem.Models;

public class Collection
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string CollectionType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<CollectionItem> Items { get; set; } = new();
    public List<CustomField> CustomFields { get; set; } = new();
}
