using Auto.Commands;
using Auto.Handlers;
using Auto.Plugins;
using AutoContracts;
using System.IO;
using System.Reflection;
using System.Text.Json;

namespace Auto.PluginUtils;

public record PluginDetail(Guid Id, string Name, string Description, List<PluginArgument> ExpectedArguments);

public interface IPluginLoader
{
	Dictionary<string, Plugin> CreateCommands();
}

public class PluginLoader(IServiceProvider serviceProvider) : IPluginLoader
{
	private static Assembly LoadPlugin(string path)
	{
		var loadContext = new PluginLoadContext(path);
		return loadContext.LoadFromAssemblyName(new AssemblyName(Path.GetFileNameWithoutExtension(path)));
	}

	private static List<string> GetDllPaths()
	{
		var paths = new List<string>();
		var pluginDirectory = Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config", "auto", "plugins"
		);

		if (!Directory.Exists(pluginDirectory))
			return paths;

		foreach (var directory in Directory.EnumerateDirectories(pluginDirectory))
		{
			try
			{
				var pluginInfo =
					JsonSerializer.Deserialize<PluginInfo>(File.ReadAllText(Path.Combine(directory, "plugin.json")));
				if (pluginInfo == null) continue;
				var dllPath = Path.Combine(directory, pluginInfo.EntryPoint);
				if (File.Exists(dllPath))
					paths.Add(dllPath);
			}
			catch
			{
				// ignored
			}
		}

		return paths;
	}

	private static IEnumerable<ICommand> GetBuiltInPluginInstances(IServiceProvider serviceProvider = null)
	{
		yield return new KeyboardInputPlugin(serviceProvider);
		yield return new StartProgramPlugin();
	}

	private Dictionary<string, Plugin> GetBuiltInCommands()
	{
		var result = new Dictionary<string, Plugin>();

		foreach (var command in GetBuiltInPluginInstances(serviceProvider))
		{
			result.Add(command.Id.ToString(), new Plugin
			{
				Action = command.Execute,
				ArgumentTypes = command.ExpectedArguments.Select(x => x.Type).ToArray(),
				StaThreadRequired = false
			});
		}

		return result;
	}

	public Dictionary<string, Plugin> CreateCommands()
	{
		var result = GetBuiltInCommands();
		foreach (var assembly in GetDllAssemblies())
		{
			try
			{
				var staThread = assembly.GetReferencedAssemblies().Any(x => x.Name == "PresentationFramework");

				IEnumerable<(string, Plugin)> GetAssemblyPlugins() => GetPluginsFromAssembly(assembly, staThread);
				var plugins = staThread
					? StaHandler.Execute(GetAssemblyPlugins)
					: GetAssemblyPlugins();

				foreach (var (id, plugin) in plugins)
					result.Add(id, plugin);
			}
			catch
			{
				// ignored
			}
		}

		return result;
	}

	private static IEnumerable<Assembly> GetDllAssemblies()
	{
		foreach (var dllPath in GetDllPaths())
		{
			yield return LoadPlugin(dllPath);
		}
	}

	private static IEnumerable<(string, Plugin)> GetPluginsFromAssembly(Assembly assembly, bool staThread)
	{
		foreach (var command in GetCommands(assembly))
		{
			yield return (command.Id.ToString(), new Plugin
			{
				Action = command.Execute,
				StaThreadRequired = staThread,
				ArgumentTypes = command.ExpectedArguments.Select(x => x.Type).ToArray()
			});
		}
	}

	private static IEnumerable<ICommand> GetCommands(Assembly assembly)
	{
		foreach (var type in assembly.GetTypes())
		{
			if (!typeof(ICommand).IsAssignableFrom(type)) continue;
			if (Activator.CreateInstance(type) is not ICommand command) continue;
			yield return command;
		}
	}

	private static IEnumerable<(Guid id, string name)> GetAvailablePlugins()
	{
		foreach (var command in GetBuiltInPluginInstances())
			yield return (command.Id, command.Name);

		foreach (var assembly in GetDllAssemblies())
			foreach (var command in GetCommands(assembly))
				yield return (command.Id, command.Name);
	}

	public static IEnumerable<PluginDetail> GetAvailablePluginDetails()
	{
		foreach (var command in GetBuiltInPluginInstances())
			yield return new PluginDetail(command.Id, command.Name,
				command.Description, command.ExpectedArguments);

		foreach (var assembly in GetDllAssemblies())
			foreach (var command in GetCommands(assembly))
				yield return new PluginDetail(command.Id, command.Name,
					command.Description, command.ExpectedArguments);
	}

	public static string GetPluginName(string guidString)
	{
		if (!Guid.TryParse(guidString, out var guid)) return null;
		return GetAvailablePlugins().FirstOrDefault(p => p.id == guid).name;
	}

	public static string ResolvePlugin(string nameOrId)
	{
		if (Guid.TryParse(nameOrId, out var guid))
			return guid.ToString();

		var (id, name) = GetAvailablePlugins()
			.FirstOrDefault(p => p.name.Equals(nameOrId, StringComparison.OrdinalIgnoreCase));

		return name != null ? id.ToString() : null;
	}
}