using PKU.Domain.Enums;

namespace PKU.Domain.Entities;

public class User
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Kontrahent;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Contractor-specific fields
    public string ContractorAbbreviation { get; set; } = string.Empty;
    public string ContractorFullName { get; set; } = string.Empty;
    public string ContractorShortName { get; set; } = string.Empty;
    public string KRS { get; set; } = string.Empty;
    public string NIP { get; set; } = string.Empty;
    public string HeadquartersAddress { get; set; } = string.Empty;
    public string ContractorCode { get; set; } = string.Empty;
    public List<ContractorType> ContractorTypes { get; set; } = [];
    public string ContractNumber { get; set; } = string.Empty;
    public DateTime? ContractStartDate { get; set; }
    public DateTime? ContractEndDate { get; set; }

    public string FullName => $"{FirstName} {LastName}".Trim();
}
