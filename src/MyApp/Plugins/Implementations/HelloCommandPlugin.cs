namespace MyApp.Plugins.Implementations;

[PluginOrder(0)]
public sealed class HelloCommandPlugin : ICommandPlugin
{
    public string Name => "hello";

    public void Execute()
    {
        Console.WriteLine("Hello from the plugin system!");
        Console.WriteLine($"Current time: {DateTime.Now:HH:mm:ss}");
    }
}