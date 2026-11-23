using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TesteSada.Application.Interfaces;
using TesteSada.Infrastructure.Persistence;
using TesteSada.Infrastructure.Repositories;

namespace TesteSada.Infrastructure;

/// <summary>
/// Registra os serviços da camada de infraestrutura (banco de dados e repositórios).
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var databaseName = configuration["Database:InMemoryName"] ?? "TesteSadaDb";

        services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(databaseName));
        services.AddScoped<ITaskRepository, TaskRepository>();
        return services;
    }
}
