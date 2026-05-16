namespace MyApp.Plugins.Implementations;

[PluginOrder(0)]
public sealed class HelloCommandPlugin(TimeProvider timeProvider) : ICommandPlugin
{
    public string Name => "hello";

    public void Execute()
    {
        Console.WriteLine("Hello from the plugin system!");
        Console.WriteLine($"Current time: {timeProvider.GetUtcNow():HH:mm:ss}");
    }
}