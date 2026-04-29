namespace CollectionManagementSystem.Models;

public class CustomField
{
    public string Name { get; set; } = string.Empty;
    public CustomFieldType FieldType { get; set; } = CustomFieldType.Text;
    public List<string> DropdownOptions { get; set; } = new();
}
