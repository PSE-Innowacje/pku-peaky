using Microsoft.Azure.Cosmos;
using PKU.Application.Interfaces;
using PKU.Domain.Entities;
using User = PKU.Domain.Entities.User;

namespace PKU.Infrastructure.Services;

public class CosmosUserService : IUserService
{
    private readonly Container _container;

    public CosmosUserService(CosmosClient cosmosClient, CosmosDbSettings settings)
    {
        _container = cosmosClient.GetContainer(settings.DatabaseName, settings.UsersContainerName);
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        var query = _container.GetItemQueryIterator<User>();
        var results = new List<User>();

        while (query.HasMoreResults)
        {
            var response = await query.ReadNextAsync();
            results.AddRange(response);
        }

        return results;
    }

    public async Task<User?> GetByIdAsync(string id)
    {
        try
        {
            var response = await _container.ReadItemAsync<User>(id, new PartitionKey(id));
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task CreateAsync(User user)
    {
        await _container.CreateItemAsync(user, new PartitionKey(user.Id));
    }

    public async Task UpdateAsync(User user)
    {
        await _container.UpsertItemAsync(user, new PartitionKey(user.Id));
    }

    public async Task DeleteAsync(string id)
    {
        await _container.DeleteItemAsync<User>(id, new PartitionKey(id));
    }
}
