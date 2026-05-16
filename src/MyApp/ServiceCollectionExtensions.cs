using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MyApp;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddPlugins(this IServiceCollection services)
    {
        services.TryAddSingleton(TimeProvider.System);

        RegisterCommandPlugins(services);
        RegisterStartupPlugins(services);

        return services;
    }

    static partial void RegisterCommandPlugins(IServiceCollection services);
    static partial void RegisterStartupPlugins(IServiceCollection services);
}
