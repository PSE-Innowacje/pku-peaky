using PKU.Domain.Enums;

namespace PKU.Domain.Entities;

public class DeclarationTemplate
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public FeeType FeeType { get; set; }
    public List<ContractorType> ContractorTypes { get; set; } = [];
    public List<TemplateField> Fields { get; set; } = [];
    public bool AllowComment { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
