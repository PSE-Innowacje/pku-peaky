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
    /// Returns declarations for a contractor for a given month/year.
    /// If declarations don't exist yet, creates them with NotSubmitted status.
    /// </summary>
    Task<IEnumerable<Declaration>> GetForContractorMonthAsync(string userId, int year, int month);
}
