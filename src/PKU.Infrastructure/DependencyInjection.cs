using Microsoft.Extensions.DependencyInjection;
using PKU.Application.Interfaces;
using PKU.Infrastructure.Services;

namespace PKU.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IUserService, InMemoryUserService>();
        services.AddSingleton<IDeclarationTemplateService, InMemoryDeclarationTemplateService>();
        services.AddSingleton<IScheduleService, InMemoryScheduleService>();
        services.AddSingleton<IDeclarationService, InMemoryDeclarationService>();
        services.AddScoped<IAuthService, AuthService>();
        return services;
    }
}
