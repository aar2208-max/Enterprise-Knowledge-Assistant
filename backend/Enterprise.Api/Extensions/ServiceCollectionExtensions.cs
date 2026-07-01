using Enterprise.Application;
using Enterprise.Infrastructure;

namespace Enterprise.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddProjectServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddApplication();

        services.AddInfrastructure(configuration);

        services.AddControllers();

        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen();

        return services;
    }
}