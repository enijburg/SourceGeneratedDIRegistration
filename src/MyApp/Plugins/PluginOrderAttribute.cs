namespace MyApp.Plugins;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class PluginOrderAttribute(int order) : Attribute
{
    public int Order { get; } = order;
}
