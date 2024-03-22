using System.IO;
using System.Reflection;
using System.Text.Json;
using Auto.Command;
using Auto.Handlers;
using AutoContracts;

namespace Auto.PluginLoader;

public static class PluginLoader
{
	private static Assembly LoadPlugin(string path)
	{
		var loadContext = new PluginLoadContext(path);
		return loadContext.LoadFromAssemblyName(new AssemblyName(Path.GetFileNameWithoutExtension(path)));
	}

	private static List<string> GetDllPaths()
	{
		var paths = new List<string>();
#if DEBUG
		var pluginDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "hvard",
			"Auto", "Plugins");
#else
        var pluginDirectory = Path.Combine(Directory.GetParent(Environment.ProcessPath!)!.FullName, "..", "Plugins");
#endif

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
			catch (Exception _)
			{
				// ignored
			}
		}

		return paths;
	}

	public static Dictionary<string, Plugin> CreateCommands()
	{
		var result = new Dictionary<string, Plugin>();

		foreach (var dllPath in GetDllPaths())
		{
			try
			{
				var assembly = LoadPlugin(dllPath);
				var staThread = assembly.GetReferencedAssemblies().Any(x => x.Name == "PresentationFramework");

				IEnumerable<(string, Plugin)> GetAssemblyPlugins() => GetPlugins(assembly, staThread);
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

	private static IEnumerable<(string, Plugin)> GetPlugins(Assembly assembly, bool staThread)
	{
		foreach (var type in assembly.GetTypes())
		{
			if (!typeof(ICommand).IsAssignableFrom(type)) continue;
			if (Activator.CreateInstance(type) is not ICommand command) continue;
			yield return (command.Id.ToString(), new Plugin
			{
				Action = command.Execute,
				StaThreadRequired = staThread,
				ArgumentTypes = command.ExpectedArguments.Select(x => x.Type).ToArray()
			});
		}
	}
}