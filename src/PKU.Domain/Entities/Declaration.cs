using PKU.Domain.Enums;

namespace PKU.Domain.Entities;

public class Declaration
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = string.Empty;
    public ContractorType ContractorType { get; set; }
    public FeeType FeeType { get; set; }
    public FeeCategory FeeCategory { get; set; }

    // Billing period
    public int BillingYear { get; set; }
    public int BillingMonth { get; set; }

    // Declaration number (generated)
    public string DeclarationNumber { get; set; } = string.Empty;

    public DeclarationStatus Status { get; set; } = DeclarationStatus.NotSubmitted;
    public DateTime? SubmittedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Schedule deadline
    public DateTime? Deadline { get; set; }

    // Field values filled by contractor (key = TemplateField.Code, value = entered value)
    public Dictionary<string, string> FieldValues { get; set; } = new();

    // Optional comment from contractor
    public string? Comment { get; set; }

    // Correction tracking
    public int CorrectionNumber { get; set; } = 0;
    public string? OriginalDeclarationId { get; set; }
}
