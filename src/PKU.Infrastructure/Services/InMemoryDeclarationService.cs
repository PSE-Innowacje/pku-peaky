using PKU.Application.Interfaces;
using PKU.Domain.Entities;

namespace PKU.Infrastructure.Services;

public class InMemoryDeclarationService : IDeclarationService
{
    private readonly List<Declaration> _declarations = [];

    public Task<IEnumerable<Declaration>> GetAllAsync() =>
        Task.FromResult<IEnumerable<Declaration>>(_declarations);

    public Task<Declaration?> GetByIdAsync(string id) =>
        Task.FromResult(_declarations.FirstOrDefault(d => d.Id == id));

    public Task<IEnumerable<Declaration>> GetPendingAsync() =>
        Task.FromResult<IEnumerable<Declaration>>(
            _declarations.Where(d => d.Status == DeclarationStatus.Submitted).ToList());

    public Task CreateAsync(Declaration declaration)
    {
        _declarations.Add(declaration);
        return Task.CompletedTask;
    }

    public Task UpdateStatusAsync(string id, DeclarationStatus status)
    {
        var declaration = _declarations.FirstOrDefault(d => d.Id == id);
        if (declaration is not null) declaration.Status = status;
        return Task.CompletedTask;
    }
}
