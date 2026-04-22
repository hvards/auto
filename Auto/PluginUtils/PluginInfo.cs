namespace Auto.PluginUtils;

internal class PluginInfo
{
	public Guid Id { get; init; }
	public string EntryPoint { get; init; } = string.Empty;
	public required string Name { get; init; }
}
