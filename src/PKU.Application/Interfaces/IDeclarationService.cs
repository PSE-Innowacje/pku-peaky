using PKU.Domain.Entities;

namespace PKU.Application.Interfaces;

public interface IDeclarationService
{
    Task<IEnumerable<Declaration>> GetAllAsync();
    Task<Declaration?> GetByIdAsync(string id);
    Task<IEnumerable<Declaration>> GetPendingAsync();
    Task CreateAsync(Declaration declaration);
    Task UpdateStatusAsync(string id, DeclarationStatus status);
}
