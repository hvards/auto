using System.IO;
using System.Reflection;
using System.Text.Json;

using Auto.Commands;
using Auto.Handlers;
using Auto.Plugins;

using AutoContracts;

using Microsoft.Extensions.Logging;

namespace Auto.PluginUtils;

internal record PluginDetail(Guid Id, string Name, string Description, List<PluginArgument> ExpectedArguments);

internal interface IPluginLoader
{
	Dictionary<string, Plugin> CreateCommands();
	IEnumerable<PluginDetail> GetAvailablePluginDetails();
	string GetPluginName(string guidString);
	string ResolvePlugin(string nameOrId);
	string? TryResolvePlugin(string nameOrId);
}

internal partial class PluginLoader(IServiceProvider serviceProvider, ILogger<PluginLoader> logger) : IPluginLoader
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

	private IEnumerable<ICommand> GetBuiltInPluginInstances()
	{
		yield return new KeyboardInputPlugin(serviceProvider);
		yield return new PowerShellPlugin();
		yield return new StartProgramPlugin();
	}

	private Dictionary<string, Plugin> GetBuiltInCommands()
	{
		var result = new Dictionary<string, Plugin>();

		foreach (var command in GetBuiltInPluginInstances())
			result.Add(command.Id.ToString(), ToPlugin(command));

		return result;
	}

	public Dictionary<string, Plugin> CreateCommands()
	{
		var result = GetBuiltInCommands();
		foreach (var assembly in GetDllAssemblies())
		{
			try
			{
				foreach (var (id, plugin) in GetPluginsFromAssembly(assembly))
					result.Add(id, plugin);
			}
			catch (Exception ex)
			{
				LogPluginAssemblyLoadFailed(ex, assembly.FullName ?? "<unknown>");
			}
		}

		return result;
	}

	internal Plugin ToPlugin(ICommand command)
	{
		var initTask = RunInitAsync(command);

		return new Plugin
		{
			Id = command.Id,
			Action = args => { initTask.GetAwaiter().GetResult(); return command.Execute(args); },
			StaThreadRequired = command.RequiresSta,
			ArgumentTypes = [.. command.ExpectedArguments.Select(x => x.Type)]
		};
	}

	private Task RunInitAsync(ICommand command)
	{
		void Init()
		{
			try { command.Init(); }
			catch (Exception ex) { LogPluginInitFailed(ex, command.Name); }
		}

		if (!command.RequiresSta)
			return Task.Run(Init);

		// STA: Init/Execute order is enforced by the StaThread's FIFO queue, not by this task.
		StaHandler.Enqueue(command.Id, Init);
		return Task.CompletedTask;
	}

	private IEnumerable<Assembly> GetDllAssemblies()
	{
		foreach (var dllPath in GetDllPaths())
		{
			var loadContext = new PluginLoadContext(dllPath);
			yield return loadContext.LoadFromAssemblyName(new AssemblyName(Path.GetFileNameWithoutExtension(dllPath)));
		}
	}

	private IEnumerable<(string, Plugin)> GetPluginsFromAssembly(Assembly assembly)
	{
		foreach (var type in GetCommandTypes(assembly))
		{
			if (Activator.CreateInstance(type) is not ICommand command) continue;
			yield return (command.Id.ToString(), ToPlugin(command));
		}
	}

	private static IEnumerable<Type> GetCommandTypes(Assembly assembly)
	{
		foreach (var type in assembly.GetTypes())
			if (typeof(ICommand).IsAssignableFrom(type) && !type.IsAbstract)
				yield return type;
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

	private IEnumerable<ICommand> GetAllCommands()
	{
		foreach (var command in GetBuiltInPluginInstances())
			yield return command;

		foreach (var assembly in GetDllAssemblies())
			foreach (var command in GetCommands(assembly))
				yield return command;
	}

	public IEnumerable<PluginDetail> GetAvailablePluginDetails()
		=> GetAllCommands().Select(c => new PluginDetail(c.Id, c.Name, c.Description, c.ExpectedArguments));

	public string GetPluginName(string guidString)
	{
		return !Guid.TryParse(guidString, out var guid)
			? string.Empty
			: (GetAllCommands().FirstOrDefault(c => c.Id == guid)?.Name) ?? string.Empty;
	}

	public string ResolvePlugin(string nameOrId)
	{
		if (Guid.TryParse(nameOrId, out var guid))
			return guid.ToString();

		return GetAllCommands().FirstOrDefault(c => c.Name.Equals(nameOrId))?.Id.ToString()
			?? throw new ArgumentException($"Unknown plugin: {nameOrId}");
	}

	public string? TryResolvePlugin(string nameOrId)
	{
		if (Guid.TryParse(nameOrId, out var guid))
			return guid.ToString();

		return GetAllCommands().FirstOrDefault(c => c.Name.Equals(nameOrId))?.Id.ToString();
	}

	[LoggerMessage(LogLevel.Error, "Plugin {PluginName} Init failed")]
	private partial void LogPluginInitFailed(Exception ex, string pluginName);

	[LoggerMessage(LogLevel.Warning, "Failed to load plugins from assembly \"{Assembly}\"")]
	private partial void LogPluginAssemblyLoadFailed(Exception ex, string assembly);
}