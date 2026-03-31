using PKU.Domain.Entities;

namespace PKU.Application.Interfaces;

public interface IDeclarationTemplateService
{
    Task<IEnumerable<DeclarationTemplate>> GetAllAsync();
    Task<DeclarationTemplate?> GetByIdAsync(string id);
    Task CreateAsync(DeclarationTemplate template);
    Task UpdateAsync(DeclarationTemplate template);
    Task DeleteAsync(string id);
}
