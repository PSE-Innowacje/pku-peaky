namespace PKU.Domain.Entities;

public class Declaration
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string TemplateId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DeclarationStatus Status { get; set; } = DeclarationStatus.Draft;
    public DateTime SubmittedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
