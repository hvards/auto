namespace Auto.PluginLoader;

public class PluginInfo
{
    public Guid Id { get; init; }
    public string EntryPoint { get; init; } = string.Empty;
}