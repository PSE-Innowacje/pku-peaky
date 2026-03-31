using PKU.Application.Interfaces;
using PKU.Domain.Entities;

namespace PKU.Infrastructure.Services;

public class InMemoryDeclarationTemplateService : IDeclarationTemplateService
{
    private readonly List<DeclarationTemplate> _templates = [];

    public Task<IEnumerable<DeclarationTemplate>> GetAllAsync() =>
        Task.FromResult<IEnumerable<DeclarationTemplate>>(_templates);

    public Task<DeclarationTemplate?> GetByIdAsync(string id) =>
        Task.FromResult(_templates.FirstOrDefault(t => t.Id == id));

    public Task CreateAsync(DeclarationTemplate template)
    {
        _templates.Add(template);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(DeclarationTemplate template)
    {
        var index = _templates.FindIndex(t => t.Id == template.Id);
        if (index >= 0) _templates[index] = template;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(string id)
    {
        _templates.RemoveAll(t => t.Id == id);
        return Task.CompletedTask;
    }
}
