using Microsoft.Extensions.DependencyInjection;
using MyApp;
using MyApp.Plugins;

var services = new ServiceCollection()
    .AddPlugins()
    .BuildServiceProvider();

// Run all startup plugins
foreach (var startup in services.GetServices<IStartupPlugin>())
    startup.OnStartup();

// Dispatch a command from args (defaults to "hello" if none given)
var commandName = args.FirstOrDefault() ?? "hello";
var commands = services.GetServices<ICommandPlugin>().ToList();
var command = commands.FirstOrDefault(c => c.Name.Equals(commandName, StringComparison.OrdinalIgnoreCase));

if (command is not null)
{
    command.Execute();
}
else
{
    Console.WriteLine($"Unknown command '{commandName}'. Available commands:");
    foreach (var cmd in commands)
        Console.WriteLine($"  {cmd.Name}");
}
