using Auto.Handlers;
using Microsoft.Extensions.Logging;

namespace Auto.PluginUtils;

public interface IPluginExecutor
{
	object ExecutePlugin(string id, IEnumerable<object> args);
}

public partial class PluginExecutor : IPluginExecutor
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
			LogPluginIdNotAvailable(id);
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
			LogErrorExecutingPlugin(ex, id);
			return null;
		}
	}

	[LoggerMessage(LogLevel.Error, "Error executing plugin {Id}")]
	public partial void LogErrorExecutingPlugin(Exception ex, string id);

	[LoggerMessage(LogLevel.Error, "Plugin {Id} not available")]
	public partial void LogPluginIdNotAvailable(string id);
}