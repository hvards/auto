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

	private static IEnumerable<ICommand> GetBuiltInPluginInstances(IServiceProvider? serviceProvider = null)
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
				ArgumentTypes = [.. command.ExpectedArguments.Select(x => x.Type)],
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

				foreach (var (id, plugin) in plugins ?? [])
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
			var loadContext = new PluginLoadContext(dllPath);
			yield return loadContext.LoadFromAssemblyName(new AssemblyName(Path.GetFileNameWithoutExtension(dllPath)));
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
				ArgumentTypes = [.. command.ExpectedArguments.Select(x => x.Type)]
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

	private static IEnumerable<ICommand> GetAllCommands()
	{
		foreach (var command in GetBuiltInPluginInstances())
			yield return command;

		foreach (var assembly in GetDllAssemblies())
			foreach (var command in GetCommands(assembly))
				yield return command;
	}

	public static IEnumerable<PluginDetail> GetAvailablePluginDetails()
		=> GetAllCommands().Select(c => new PluginDetail(c.Id, c.Name, c.Description, c.ExpectedArguments));

	public static string GetPluginName(string guidString)
	{
		return !Guid.TryParse(guidString, out var guid)
			? string.Empty 
			: (GetAllCommands().FirstOrDefault(c => c.Id == guid)?.Name) ?? string.Empty;
	}

	public static string ResolvePlugin(string nameOrId)
	{
		if (Guid.TryParse(nameOrId, out var guid))
			return guid.ToString();

		return GetAllCommands().FirstOrDefault(c => c.Name.Equals(nameOrId))?.Id.ToString() 
			?? throw new ArgumentException($"Unknown plugin: {nameOrId}");
	}

	public static string? TryResolvePlugin(string nameOrId)
	{
		if (Guid.TryParse(nameOrId, out var guid))
			return guid.ToString();

		return GetAllCommands().FirstOrDefault(c => c.Name.Equals(nameOrId))?.Id.ToString();
	}
}