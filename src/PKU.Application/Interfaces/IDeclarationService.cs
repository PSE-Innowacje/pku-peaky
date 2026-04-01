using PKU.Domain.Entities;

namespace PKU.Application.Interfaces;

public interface IDeclarationService
{
    Task<IEnumerable<Declaration>> GetAllAsync();
    Task<Declaration?> GetByIdAsync(string id);
    Task<IEnumerable<Declaration>> GetPendingAsync();
    Task CreateAsync(Declaration declaration);
    Task UpdateStatusAsync(string id, DeclarationStatus status);

    Task UpdateAsync(Declaration declaration);

    /// <summary>
    /// Returns existing declarations for a contractor for a given month/year.
    /// </summary>
    Task<IEnumerable<Declaration>> GetForContractorMonthAsync(string userId, int year, int month);

    /// <summary>
    /// Returns existing declarations for a contractor for a given year (all months).
    /// </summary>
    Task<IEnumerable<Declaration>> GetForContractorYearAsync(string userId, int year);

    /// <summary>
    /// Creates missing declarations for a contractor for a given month/year and returns all declarations for that period.
    /// </summary>
    Task<IEnumerable<Declaration>> CreateMissingDeclarationsAsync(string userId, int year, int month);

    /// <summary>
    /// Serializes a declaration to JSON bytes for export.
    /// </summary>
    byte[] ExportToJson(Declaration declaration);
}
