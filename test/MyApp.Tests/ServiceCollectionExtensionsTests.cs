using Microsoft.Extensions.DependencyInjection;
using MyApp.Plugins;
using MyApp.Plugins.Implementations;

namespace MyApp.Tests;

[TestClass]
public sealed class ServiceCollectionExtensionsTests
{
    private static ServiceProvider BuildProvider()
        => new ServiceCollection().AddPlugins().BuildServiceProvider();

    [TestMethod]
    public void AddPlugins_RegistersAllThreeCommandPlugins()
    {
        using var provider = BuildProvider();

        var commandPlugins = provider.GetServices<ICommandPlugin>().ToList();

        Assert.HasCount(3, commandPlugins);
    }

    [TestMethod]
    public void AddPlugins_CommandPlugins_AreRegisteredInPluginOrderAttributeOrder()
    {
        using var provider = BuildProvider();

        var commandPlugins = provider.GetServices<ICommandPlugin>().ToList();

        // HelloCommandPlugin has [PluginOrder(0)], GoodByeCommandPlugin has [PluginOrder(1)],
        // AllCommandPlugin has no attribute → int.MaxValue, so it comes last.
        Assert.IsInstanceOfType<HelloCommandPlugin>(commandPlugins[0]);
        Assert.IsInstanceOfType<GoodByeCommandPlugin>(commandPlugins[1]);
        Assert.IsInstanceOfType<AllCommandPlugin>(commandPlugins[2]);
    }

    [TestMethod]
    public void AddPlugins_RegistersHelloCommandPlugin()
    {
        using var provider = BuildProvider();

        var plugin = provider.GetServices<ICommandPlugin>()
            .OfType<HelloCommandPlugin>()
            .SingleOrDefault();

        Assert.IsNotNull(plugin);
    }

    [TestMethod]
    public void AddPlugins_RegistersGoodByeCommandPlugin()
    {
        using var provider = BuildProvider();

        var plugin = provider.GetServices<ICommandPlugin>()
            .OfType<GoodByeCommandPlugin>()
            .SingleOrDefault();

        Assert.IsNotNull(plugin);
    }

    [TestMethod]
    public void AddPlugins_RegistersAllCommandPlugin()
    {
        using var provider = BuildProvider();

        var plugin = provider.GetServices<ICommandPlugin>()
            .OfType<AllCommandPlugin>()
            .SingleOrDefault();

        Assert.IsNotNull(plugin);
    }

    [TestMethod]
    public void AddPlugins_CommandPlugins_AreRegisteredAsSingletons()
    {
        using var provider = BuildProvider();

        var first = provider.GetServices<ICommandPlugin>().ToList();
        var second = provider.GetServices<ICommandPlugin>().ToList();

        for (var i = 0; i < first.Count; i++)
        {
            Assert.AreSame(first[i], second[i],
                $"Command plugin at index {i} should be a singleton.");
        }
    }

    [TestMethod]
    public void AddPlugins_RegistersStartupPlugin()
    {
        using var provider = BuildProvider();

        var startupPlugins = provider.GetServices<IStartupPlugin>().ToList();

        Assert.HasCount(1, startupPlugins);
        Assert.IsInstanceOfType<StartupPlugin>(startupPlugins[0]);
    }

    [TestMethod]
    public void AddPlugins_StartupPlugin_IsRegisteredAsSingleton()
    {
        using var provider = BuildProvider();

        var first = provider.GetService<IStartupPlugin>();
        var second = provider.GetService<IStartupPlugin>();

        Assert.AreSame(first, second);
    }

    [TestMethod]
    public void AddPlugins_HelloCommandPlugin_HasCorrectName()
    {
        using var provider = BuildProvider();

        var plugin = provider.GetServices<ICommandPlugin>().OfType<HelloCommandPlugin>().Single();

        Assert.AreEqual("hello", plugin.Name);
    }

    [TestMethod]
    public void AddPlugins_GoodByeCommandPlugin_HasCorrectName()
    {
        using var provider = BuildProvider();

        var plugin = provider.GetServices<ICommandPlugin>().OfType<GoodByeCommandPlugin>().Single();

        Assert.AreEqual("goodbye", plugin.Name);
    }

    [TestMethod]
    public void AddPlugins_AllCommandPlugin_HasCorrectName()
    {
        using var provider = BuildProvider();

        var plugin = provider.GetServices<ICommandPlugin>().OfType<AllCommandPlugin>().Single();

        Assert.AreEqual("all", plugin.Name);
    }
}
