using Microsoft.Extensions.DependencyInjection;

namespace MyApp;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddPlugins(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);
        RegisterCommandPlugins(services);
        RegisterStartupPlugins(services);

        return services;
    }

    static partial void RegisterCommandPlugins(IServiceCollection services);
    static partial void RegisterStartupPlugins(IServiceCollection services);
}
