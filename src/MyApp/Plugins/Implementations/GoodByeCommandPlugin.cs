namespace MyApp.Plugins.Implementations;

[PluginOrder(1)]
public sealed class GoodByeCommandPlugin(TimeProvider timeProvider) : ICommandPlugin
{
    public string Name => "goodbye";

    public void Execute()
    {
        Console.WriteLine("Goodbye from the plugin system!");
        Console.WriteLine($"Current time: {timeProvider.GetUtcNow():HH:mm:ss}");
    }
}