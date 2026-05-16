namespace MyApp.Plugins.Implementations;

public sealed class GoodByeCommandPlugin : ICommandPlugin
{
    public string Name => "goodbye";

    public void Execute()
    {
        Console.WriteLine("Goodbye from the plugin system!");
        Console.WriteLine($"Current time: {DateTime.Now:HH:mm:ss}");
    }
}