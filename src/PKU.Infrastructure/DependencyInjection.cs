using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PKU.Application.Interfaces;
using PKU.Infrastructure.Services;

namespace PKU.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var cosmosSettings = configuration.GetSection("CosmosDb").Get<CosmosDbSettings>()!;
        services.AddSingleton(cosmosSettings);

        var cosmosClientOptions = new CosmosClientOptions
        {
            SerializerOptions = new CosmosSerializationOptions
            {
                PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
            },
            HttpClientFactory = () =>
            {
                return new HttpClient(new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback =
                        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                });
            },
            ConnectionMode = ConnectionMode.Gateway
        };

        var cosmosClient = new CosmosClient(cosmosSettings.Endpoint, cosmosSettings.PrimaryKey, cosmosClientOptions);
        services.AddSingleton(cosmosClient);

        services.AddSingleton<IUserService, CosmosUserService>();
        services.AddSingleton<IDeclarationTemplateService, InMemoryDeclarationTemplateService>();
        services.AddSingleton<IScheduleService, InMemoryScheduleService>();
        services.AddSingleton<IDeclarationService, InMemoryDeclarationService>();
        services.AddScoped<IAuthService, AuthService>();
        return services;
    }
}
