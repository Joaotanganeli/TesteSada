using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using TesteSada.Application.Interfaces;
using TesteSada.Application.Services;

namespace TesteSada.Application;

/// <summary>
/// Registra os serviços da camada de aplicação no container de DI.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddScoped<ITaskService, TaskService>();
        return services;
    }
}
