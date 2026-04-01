using System.Text.Json;
using Microsoft.Azure.Cosmos;
using PKU.Application.Interfaces;
using PKU.Domain.Entities;
using PKU.Domain.Enums;
using PKU.Domain.Services;

namespace PKU.Infrastructure.Services;

public class CosmosDeclarationService : IDeclarationService
{
    private readonly Container _container;
    private readonly IUserService _userService;
    private readonly IDeclarationTemplateService _templateService;

    public CosmosDeclarationService(CosmosClient cosmosClient, CosmosDbSettings settings, IUserService userService, IDeclarationTemplateService templateService)
    {
        _container = cosmosClient.GetContainer(settings.DatabaseName, settings.DeclarationsContainerName);
        _userService = userService;
        _templateService = templateService;
    }

    public async Task<IEnumerable<Declaration>> GetAllAsync()
    {
        var query = _container.GetItemQueryIterator<Declaration>();
        var results = new List<Declaration>();
        while (query.HasMoreResults)
        {
            var response = await query.ReadNextAsync();
            results.AddRange(response);
        }
        return results;
    }

    public async Task<Declaration?> GetByIdAsync(string id)
    {
        try
        {
            var response = await _container.ReadItemAsync<Declaration>(id, new PartitionKey(id));
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<IEnumerable<Declaration>> GetPendingAsync()
    {
        var sql = "SELECT * FROM c WHERE c.status = @status";
        var queryDef = new QueryDefinition(sql).WithParameter("@status", (int)DeclarationStatus.Submitted);
        var query = _container.GetItemQueryIterator<Declaration>(queryDef);
        var results = new List<Declaration>();
        while (query.HasMoreResults)
        {
            var response = await query.ReadNextAsync();
            results.AddRange(response);
        }
        return results;
    }

    public async Task CreateAsync(Declaration declaration)
    {
        await _container.CreateItemAsync(declaration, new PartitionKey(declaration.Id));
    }

    public async Task UpdateAsync(Declaration declaration)
    {
        await _container.UpsertItemAsync(declaration, new PartitionKey(declaration.Id));
    }

    public async Task UpdateStatusAsync(string id, DeclarationStatus status)
    {
        try
        {
            var response = await _container.ReadItemAsync<Declaration>(id, new PartitionKey(id));
            var declaration = response.Resource;
            declaration.Status = status;
            await _container.UpsertItemAsync(declaration, new PartitionKey(id));
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
        }
    }

    public async Task<IEnumerable<Declaration>> GetForContractorMonthAsync(string userId, int year, int month)
    {
        var user = await _userService.GetByIdAsync(userId);
        if (user is null || user.Role != UserRole.Kontrahent)
            return [];

        var sql = "SELECT * FROM c WHERE c.userId = @userId AND c.billingYear = @year AND c.billingMonth = @month";
        var queryDef = new QueryDefinition(sql)
            .WithParameter("@userId", userId)
            .WithParameter("@year", year)
            .WithParameter("@month", month);
        var query = _container.GetItemQueryIterator<Declaration>(queryDef);
        var results = new List<Declaration>();
        while (query.HasMoreResults)
        {
            var response = await query.ReadNextAsync();
            results.AddRange(response);
        }

        return results;
    }

    public async Task<IEnumerable<Declaration>> GetForContractorYearAsync(string userId, int year)
    {
        var user = await _userService.GetByIdAsync(userId);
        if (user is null || user.Role != UserRole.Kontrahent)
            return [];

        var sql = "SELECT * FROM c WHERE c.userId = @userId AND c.billingYear = @year";
        var queryDef = new QueryDefinition(sql)
            .WithParameter("@userId", userId)
            .WithParameter("@year", year);
        var query = _container.GetItemQueryIterator<Declaration>(queryDef);
        var results = new List<Declaration>();
        while (query.HasMoreResults)
        {
            var response = await query.ReadNextAsync();
            results.AddRange(response);
        }

        return results;
    }

    public async Task<IEnumerable<Declaration>> CreateMissingDeclarationsAsync(string userId, int year, int month)
    {
        var user = await _userService.GetByIdAsync(userId);
        if (user is null || user.Role != UserRole.Kontrahent)
            return [];

        var feeTypes = user.ContractorTypes
            .SelectMany(ContractorFeeMapping.GetFeeTypesForContractor)
            .Distinct()
            .ToArray();

        var existing = (await GetForContractorMonthAsync(userId, year, month)).ToList();
        var templates = (await _templateService.GetAllAsync()).Where(t => t.IsActive).ToList();

        foreach (var feeType in feeTypes)
        {
            if (existing.Any(d => d.FeeType == feeType))
                continue;

            var contractorType = user.ContractorTypes.First(ct =>
                ContractorFeeMapping.GetFeeTypesForContractor(ct).Contains(feeType));

            var template = templates.FirstOrDefault(t =>
                t.FeeType == feeType &&
                t.ContractorTypes.Contains(contractorType));

            if (template is null)
                continue;

            var feeCategory = DeclarationNumberGenerator.GetFeeCategory(feeType);
            var declaration = new Declaration
            {
                UserId = userId,
                ContractorType = contractorType,
                FeeType = feeType,
                FeeCategory = feeCategory,
                BillingYear = year,
                BillingMonth = month,
                Status = DeclarationStatus.NotSubmitted,
                Deadline = new DateTime(year, month, 1).AddMonths(1).AddDays(9)
            };
            await _container.CreateItemAsync(declaration, new PartitionKey(declaration.Id));
            existing.Add(declaration);
        }

        return existing;
    }

    private static readonly JsonSerializerOptions ExportJsonOptions = new()
    {
        WriteIndented = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public byte[] ExportToJson(Declaration declaration)
    {
        return JsonSerializer.SerializeToUtf8Bytes(declaration, ExportJsonOptions);
    }
}
