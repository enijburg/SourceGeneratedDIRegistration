namespace MyApp.Plugins.Implementations;

public sealed class HelloCommandPlugin : ICommandPlugin
{
    public string Name => "hello";

    public void Execute()
    {
        Console.WriteLine("Hello from the plugin system!");
        Console.WriteLine($"Current time: {DateTime.Now:HH:mm:ss}");
    }
}