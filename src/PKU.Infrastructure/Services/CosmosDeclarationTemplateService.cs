using Microsoft.Azure.Cosmos;
using PKU.Application.Interfaces;
using PKU.Domain.Entities;

namespace PKU.Infrastructure.Services;

public class CosmosDeclarationTemplateService : IDeclarationTemplateService
{
    private readonly Container _container;

    public CosmosDeclarationTemplateService(CosmosClient cosmosClient, CosmosDbSettings settings)
    {
        _container = cosmosClient.GetContainer(settings.DatabaseName, settings.TemplatesContainerName);
    }

    public async Task<IEnumerable<DeclarationTemplate>> GetAllAsync()
    {
        var query = _container.GetItemQueryIterator<DeclarationTemplate>();
        var results = new List<DeclarationTemplate>();
        while (query.HasMoreResults)
        {
            var response = await query.ReadNextAsync();
            results.AddRange(response);
        }
        return results;
    }

    public async Task<DeclarationTemplate?> GetByIdAsync(string id)
    {
        try
        {
            var response = await _container.ReadItemAsync<DeclarationTemplate>(id, new PartitionKey(id));
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task CreateAsync(DeclarationTemplate template)
    {
        await _container.CreateItemAsync(template, new PartitionKey(template.Id));
    }

    public async Task UpdateAsync(DeclarationTemplate template)
    {
        await _container.UpsertItemAsync(template, new PartitionKey(template.Id));
    }

    public async Task DeleteAsync(string id)
    {
        await _container.DeleteItemAsync<DeclarationTemplate>(id, new PartitionKey(id));
    }
}
