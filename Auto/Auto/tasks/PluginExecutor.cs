using Auto.Handlers;
using Auto.Interfaces;
using Microsoft.Extensions.Logging;

namespace Auto.tasks;

public class PluginExecutor : IPluginExecutor
{
	private readonly ILogger<PluginExecutor> _logger;
	private static Dictionary<string, Command.Plugin> _plugins;

	public PluginExecutor(IPluginLoader pluginLoader, ILogger<PluginExecutor> logger)
	{
		_logger = logger;
		_plugins = pluginLoader.CreateCommands();
	}

	public object ExecutePlugin(string id, IEnumerable<object> args)
	{
		if (!_plugins.TryGetValue(id, out var plugin))
		{
			_logger.LogError("Plugin {Id} not available", id);
			return null;
		}

		try
		{
			var arguments = new object[plugin.ArgumentTypes.Length];

			var i = 0;
			foreach (var argument in args)
			{
				var argumentType = plugin.ArgumentTypes[i];
				arguments[i++] = argumentType == typeof(string) || argument is not string arg
					? argument
					: TypeConverter.Convert(arg, argumentType);
			}

			return plugin.StaThreadRequired
				? StaHandler.Execute(() => plugin.Action(arguments))
				: plugin.Action(arguments);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error executing plugin {Id}", id);
			return null;
		}
	}
}