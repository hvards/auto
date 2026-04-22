using Auto.Commands;
using Auto.PluginUtils;

namespace IntegrationTests.Stubs;

internal sealed class PluginLoaderStub : IPluginLoader
{
	private static readonly Dictionary<string, Guid> PluginsByName = new()
	{
		["StartProgram"] = Guid.Parse("11111111-1111-1111-1111-111111111111"),
		["PowerShell"] = Guid.Parse("22222222-2222-2222-2222-222222222222"),
		["KeyboardInput"] = Guid.Parse("33333333-3333-3333-3333-333333333333"),
	};

	public Dictionary<string, Plugin> CreateCommands() => [];

	public IEnumerable<PluginDetail> GetAvailablePluginDetails()
		=> PluginsByName.Select(kvp => new PluginDetail(kvp.Value, kvp.Key, string.Empty, []));

	public string GetPluginName(string guidString)
	{
		if (!Guid.TryParse(guidString, out var guid))
			return string.Empty;
		return PluginsByName.FirstOrDefault(kvp => kvp.Value == guid).Key ?? string.Empty;
	}

	public string ResolvePlugin(string nameOrId)
		=> TryResolvePlugin(nameOrId) ?? throw new ArgumentException($"Unknown plugin: {nameOrId}");

	public string? TryResolvePlugin(string nameOrId)
	{
		if (Guid.TryParse(nameOrId, out var guid))
			return guid.ToString();
		return PluginsByName.TryGetValue(nameOrId, out var id) ? id.ToString() : null;
	}
}