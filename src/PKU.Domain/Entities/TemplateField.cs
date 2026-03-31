namespace PKU.Domain.Entities;

public class TemplateField
{
    public string Number { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string DataType { get; set; } = "Number";
    public bool IsRequired { get; set; } = true;
    public string Unit { get; set; } = string.Empty;
}
