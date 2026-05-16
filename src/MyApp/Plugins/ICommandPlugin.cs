namespace MyApp.Plugins;

public interface ICommandPlugin
{
    string Name { get; }

    void Execute();
}
