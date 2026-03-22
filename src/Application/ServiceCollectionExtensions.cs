using Microsoft.Extensions.DependencyInjection;

namespace BldLeague.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBldLeagueApplication(
        this IServiceCollection services)
    {
        var assembly = typeof(IAssemblyMarker).Assembly;
        
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
        });
        
        return services;
    }
}