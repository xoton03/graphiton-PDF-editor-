using System.Collections.Generic;

namespace Grafiton.Models;

public enum FormFieldType
{
    Text,
    CheckBox,
    Choice
}

public class FormItemModel
{
    public string Name { get; set; } = string.Empty;
    public FormFieldType FieldType { get; set; } = FormFieldType.Text;
    public string Value { get; set; } = string.Empty;
    public bool IsChecked { get; set; }
    public List<string> Options { get; set; } = new();
    public int PageNumber { get; set; } = 1;
}
