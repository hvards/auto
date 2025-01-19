using Auto.Handlers;
using Auto.Interfaces;

namespace Auto.tasks;

public class PluginExecutor : IPluginExecutor
{
	private static Dictionary<string, Command.Plugin> _plugins;

	public PluginExecutor(IPluginLoader pluginLoader)
	{
		_plugins = pluginLoader.CreateCommands();
	}

	public object ExecutePlugin(string id, IEnumerable<object> args)
	{
		if (!_plugins.TryGetValue(id, out var plugin))
		{
			Log.Error($"Plugin {id} not available");
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
			Log.Error($"Error executing plugin {id}: {ex}");
			return null;
		}
	}
}