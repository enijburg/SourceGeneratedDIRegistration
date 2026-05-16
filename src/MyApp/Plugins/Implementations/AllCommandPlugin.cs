using Microsoft.Extensions.DependencyInjection;

namespace MyApp.Plugins.Implementations;

public sealed class AllCommandPlugin(
    IServiceProvider serviceProvider)
    : ICommandPlugin
{
    public string Name => "all";

    public void Execute()
    {
        Console.WriteLine("Executing all command plugins...");

        foreach (var plugin in serviceProvider.GetServices<ICommandPlugin>())
        {
            if (plugin == this) continue; // Skip the "all" plugin itself
            plugin.Execute();
        }

        Console.WriteLine("All command plugins executed.");
    }
}