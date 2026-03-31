using Microsoft.Azure.Cosmos;
using PKU.Application.Interfaces;
using PKU.Domain.Entities;

namespace PKU.Infrastructure.Services;

public class CosmosScheduleService : IScheduleService
{
    private readonly Container _container;

    public CosmosScheduleService(CosmosClient cosmosClient, CosmosDbSettings settings)
    {
        _container = cosmosClient.GetContainer(settings.DatabaseName, settings.SchedulesContainerName);
    }

    public async Task<IEnumerable<Schedule>> GetAllAsync()
    {
        var query = _container.GetItemQueryIterator<Schedule>();
        var results = new List<Schedule>();

        while (query.HasMoreResults)
        {
            var response = await query.ReadNextAsync();
            results.AddRange(response);
        }

        return results;
    }

    public async Task<Schedule?> GetByIdAsync(string id)
    {
        try
        {
            var response = await _container.ReadItemAsync<Schedule>(id, new PartitionKey(id));
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task CreateAsync(Schedule schedule)
    {
        await _container.CreateItemAsync(schedule, new PartitionKey(schedule.Id));
    }

    public async Task UpdateAsync(Schedule schedule)
    {
        await _container.UpsertItemAsync(schedule, new PartitionKey(schedule.Id));
    }

    public async Task DeleteAsync(string id)
    {
        await _container.DeleteItemAsync<Schedule>(id, new PartitionKey(id));
    }
}
