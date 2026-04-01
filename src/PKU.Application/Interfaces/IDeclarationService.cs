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
    /// Creates missing declarations for a contractor for a given month/year and returns all declarations for that period.
    /// </summary>
    Task<IEnumerable<Declaration>> CreateMissingDeclarationsAsync(string userId, int year, int month);
}
