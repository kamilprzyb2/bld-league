using Microsoft.Extensions.DependencyInjection;

namespace BldLeague.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBldLeagueApplication(
        this IServiceCollection services,
        string? mediatrLicenseKey = null)
    {
        var assembly = typeof(IAssemblyMarker).Assembly;

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            if (!string.IsNullOrWhiteSpace(mediatrLicenseKey))
                cfg.LicenseKey = mediatrLicenseKey;
        });

        return services;
    }
}