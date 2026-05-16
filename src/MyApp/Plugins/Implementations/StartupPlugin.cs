namespace MyApp.Plugins.Implementations;

public sealed class StartupPlugin : IStartupPlugin
{
    public bool Started { get; private set; }

    public void OnStartup()
    {
        Started = true;
        Console.WriteLine("[Startup] complete.");
    }
}