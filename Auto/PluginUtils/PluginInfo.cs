namespace Auto.PluginUtils;

public class PluginInfo
{
	public Guid Id { get; init; }
	public string EntryPoint { get; init; } = string.Empty;
	public string Name { get; init; }
}