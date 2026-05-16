using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace MyApp.SourceGeneration.Tests;

[TestClass]
public sealed class PluginRegistrationGeneratorTests
{
    // Minimal plugin infrastructure included in every test compilation
    private const string PluginInterfacesSource = """
        using System;

        namespace MyApp.Plugins
        {
            public interface ICommandPlugin { }

            public interface IStartupPlugin { }

            [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
            public sealed class PluginOrderAttribute : Attribute
            {
                public PluginOrderAttribute(int order) => Order = order;
                public int Order { get; }
            }
        }
        """;

    private static Compilation CreateCompilation(params string[] sources)
    {
        var syntaxTrees = new[] { PluginInterfacesSource }
            .Concat(sources)
            .Select(s => CSharpSyntaxTree.ParseText(s))
            .ToArray();

        var references = ((string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES"))!
            .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
            .Select(p => MetadataReference.CreateFromFile(p))
            .ToArray<MetadataReference>();

        return CSharpCompilation.Create(
            assemblyName: "TestAssembly",
            syntaxTrees: syntaxTrees,
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }

    private static IReadOnlyList<GeneratedSourceResult> RunGenerator(Compilation compilation)
    {
        var generator = new PluginRegistrationGenerator();
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out _);
        return [.. driver.GetRunResult().Results.SelectMany(r => r.GeneratedSources)];
    }

    private static string GetGeneratedSourceText(IReadOnlyList<GeneratedSourceResult> sources, string hintName)
    {
        var match = sources.SingleOrDefault(s => s.HintName == hintName);
        Assert.IsNotNull(
            match.SourceText,
            $"Generated source '{hintName}' was not found. Available: [{string.Join(", ", sources.Select(s => s.HintName))}]");
        return match.SourceText!.ToString();
    }

    [TestMethod]
    public void Generator_WithNoPlugins_GeneratesBothFilesWithEmptyBodies()
    {
        var compilation = CreateCompilation();

        var sources = RunGenerator(compilation);

        Assert.HasCount(2, sources);
        var commandSource = GetGeneratedSourceText(sources, "CommandPluginRegistration.g.cs");
        var startupSource = GetGeneratedSourceText(sources, "StartupPluginRegistration.g.cs");

        Assert.Contains("static partial void RegisterCommandPlugins(IServiceCollection services)", commandSource);
        Assert.DoesNotContain("AddSingleton", commandSource, "Expected no AddSingleton calls when no command plugins exist.");

        Assert.Contains("static partial void RegisterStartupPlugins(IServiceCollection services)", startupSource);
        Assert.DoesNotContain("AddSingleton", startupSource, "Expected no AddSingleton calls when no startup plugins exist.");
    }

    [TestMethod]
    public void Generator_WithCommandPlugin_RegistersCommandPlugin()
    {
        var compilation = CreateCompilation("""
            namespace MyApp.Plugins.Implementations
            {
                public sealed class MyCommandPlugin : ICommandPlugin { }
            }
            """);

        var sources = RunGenerator(compilation);

        var commandSource = GetGeneratedSourceText(sources, "CommandPluginRegistration.g.cs");

        Assert.Contains("services.AddSingleton<global::MyApp.Plugins.ICommandPlugin, global::MyApp.Plugins.Implementations.MyCommandPlugin>();", commandSource);
    }

    [TestMethod]
    public void Generator_WithStartupPlugin_RegistersStartupPlugin()
    {
        var compilation = CreateCompilation("""
            namespace MyApp.Plugins.Implementations
            {
                public sealed class MyStartupPlugin : IStartupPlugin { }
            }
            """);

        var sources = RunGenerator(compilation);

        var startupSource = GetGeneratedSourceText(sources, "StartupPluginRegistration.g.cs");

        Assert.Contains("services.AddSingleton<global::MyApp.Plugins.IStartupPlugin, global::MyApp.Plugins.Implementations.MyStartupPlugin>();", startupSource);
    }

    [TestMethod]
    public void Generator_WithOrderedCommandPlugins_RegistersInPluginOrderAttributeOrder()
    {
        var compilation = CreateCompilation("""
            namespace MyApp.Plugins.Implementations
            {
                [PluginOrder(1)]
                public sealed class BetaPlugin : ICommandPlugin { }

                [PluginOrder(0)]
                public sealed class AlphaPlugin : ICommandPlugin { }
            }
            """);

        var sources = RunGenerator(compilation);

        var commandSource = GetGeneratedSourceText(sources, "CommandPluginRegistration.g.cs");

        var alphaIndex = commandSource.IndexOf("AlphaPlugin", StringComparison.Ordinal);
        var betaIndex = commandSource.IndexOf("BetaPlugin", StringComparison.Ordinal);

        Assert.IsGreaterThanOrEqualTo(0, alphaIndex, "AlphaPlugin not found in generated source.");
        Assert.IsGreaterThanOrEqualTo(0, betaIndex, "BetaPlugin not found in generated source.");
        Assert.IsLessThan(betaIndex, alphaIndex, "AlphaPlugin (order 0) should be registered before BetaPlugin (order 1).");
    }

    [TestMethod]
    public void Generator_WithPluginsHavingSameOrder_RegistersAlphabetically()
    {
        var compilation = CreateCompilation("""
            namespace MyApp.Plugins.Implementations
            {
                [PluginOrder(0)]
                public sealed class ZebraPlugin : ICommandPlugin { }

                [PluginOrder(0)]
                public sealed class AardvarkPlugin : ICommandPlugin { }
            }
            """);

        var sources = RunGenerator(compilation);

        var commandSource = GetGeneratedSourceText(sources, "CommandPluginRegistration.g.cs");

        var aardvarkIndex = commandSource.IndexOf("AardvarkPlugin", StringComparison.Ordinal);
        var zebraIndex = commandSource.IndexOf("ZebraPlugin", StringComparison.Ordinal);

        Assert.IsTrue(aardvarkIndex >= 0 && zebraIndex >= 0);
        Assert.IsLessThan(zebraIndex, aardvarkIndex,
            "AardvarkPlugin should be registered before ZebraPlugin when both have the same PluginOrder.");
    }

    [TestMethod]
    public void Generator_WithPluginWithNoOrderAttribute_RegisteredAfterOrderedPlugins()
    {
        var compilation = CreateCompilation("""
            namespace MyApp.Plugins.Implementations
            {
                public sealed class UnorderedPlugin : ICommandPlugin { }

                [PluginOrder(0)]
                public sealed class FirstPlugin : ICommandPlugin { }
            }
            """);

        var sources = RunGenerator(compilation);

        var commandSource = GetGeneratedSourceText(sources, "CommandPluginRegistration.g.cs");

        var firstIndex = commandSource.IndexOf("FirstPlugin", StringComparison.Ordinal);
        var unorderedIndex = commandSource.IndexOf("UnorderedPlugin", StringComparison.Ordinal);

        Assert.IsTrue(firstIndex >= 0 && unorderedIndex >= 0);
        Assert.IsLessThan(unorderedIndex, firstIndex,
            "FirstPlugin (order 0) should be registered before UnorderedPlugin (no order = int.MaxValue).");
    }

    [TestMethod]
    public void Generator_WithAbstractCommandPlugin_DoesNotRegisterAbstractClass()
    {
        var compilation = CreateCompilation("""
            namespace MyApp.Plugins.Implementations
            {
                public abstract class AbstractPlugin : ICommandPlugin { }

                public sealed class ConcretePlugin : ICommandPlugin { }
            }
            """);

        var sources = RunGenerator(compilation);

        var commandSource = GetGeneratedSourceText(sources, "CommandPluginRegistration.g.cs");

        Assert.DoesNotContain("AbstractPlugin", commandSource,
            "Abstract classes should not be registered.");
        Assert.Contains("ConcretePlugin", commandSource);
    }

    [TestMethod]
    public void Generator_WithCommandPlugin_DoesNotRegisterAsStartupPlugin()
    {
        var compilation = CreateCompilation("""
            namespace MyApp.Plugins.Implementations
            {
                public sealed class MyCommandPlugin : ICommandPlugin { }
            }
            """);

        var sources = RunGenerator(compilation);

        var startupSource = GetGeneratedSourceText(sources, "StartupPluginRegistration.g.cs");

        Assert.DoesNotContain("MyCommandPlugin", startupSource,
            "A command plugin should not appear in the startup plugin registration.");
    }

    [TestMethod]
    public void Generator_GeneratedSources_ContainAutoGeneratedHeader()
    {
        var compilation = CreateCompilation();

        var sources = RunGenerator(compilation);

        foreach (var source in sources)
        {
            StringAssert.StartsWith(source.SourceText.ToString(), "// <auto-generated />");
        }
    }

    [TestMethod]
    public void Generator_GeneratedSources_AreInMyAppNamespace()
    {
        var compilation = CreateCompilation();

        var sources = RunGenerator(compilation);

        foreach (var source in sources)
        {
            Assert.Contains("namespace MyApp;", source.SourceText.ToString());
        }
    }

    [TestMethod]
    public void Generator_WithClassImplementingBothInterfaces_RegistersInBothFiles()
    {
        var compilation = CreateCompilation("""
            namespace MyApp.Plugins.Implementations
            {
                public sealed class MultiPlugin : ICommandPlugin, IStartupPlugin { }
            }
            """);

        var sources = RunGenerator(compilation);

        var commandSource = GetGeneratedSourceText(sources, "CommandPluginRegistration.g.cs");
        var startupSource = GetGeneratedSourceText(sources, "StartupPluginRegistration.g.cs");

        Assert.Contains("MultiPlugin", commandSource);
        Assert.Contains("MultiPlugin", startupSource);
    }
}
