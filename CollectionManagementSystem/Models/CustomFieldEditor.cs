using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CollectionManagementSystem.Models;

public class CustomFieldEditor : INotifyPropertyChanged
{
    private string _value;

    public CustomFieldEditor(CustomField field, string currentValue)
    {
        Field = field;
        _value = currentValue;
    }

    public CustomField Field { get; }
    public string FieldName => Field.Name;
    public CustomFieldType FieldType => Field.FieldType;
    public List<string> DropdownOptions => Field.DropdownOptions;

    public bool IsText => Field.FieldType == CustomFieldType.Text;
    public bool IsNumber => Field.FieldType == CustomFieldType.Number;
    public bool IsDropdown => Field.FieldType == CustomFieldType.Dropdown;

    public string Value
    {
        get => _value;
        set { _value = value; OnPropertyChanged(); }
    }

    public int DropdownSelectedIndex
    {
        get
        {
            var idx = Field.DropdownOptions.IndexOf(_value);
            return idx >= 0 ? idx : 0;
        }
        set
        {
            if (value >= 0 && value < Field.DropdownOptions.Count)
            {
                _value = Field.DropdownOptions[value];
                OnPropertyChanged(nameof(Value));
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
