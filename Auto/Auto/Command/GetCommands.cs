using System.IO;
using System.Text.Json;

namespace Auto.Command;

public static class GetCommands
{
	public static List<Command> Execute(IEnumerable<string> folders)
	{
		var commands = GetEnabledCommands(folders).ToList();
		foreach (var command in commands)
		{
			command.ClipboardTextRequired = command.PowerShellArguments.Select(x => x.Value)
				                                .Any(x => x.Any(y => y.ClipboardTextRequired)) ||
			                                (command.PluginArguments?.Select(x => x.Value)
				                                .Any(x => x.Any(y => y.ClipboardTextRequired)) ?? false);
			command.HighlightedTextRequired = command.PowerShellArguments.Select(x => x.Value)
				                                  .Any(x => x.Any(y => y.HighlightedTextRequired)) ||
			                                  (command.PluginArguments?.Select(x => x.Value)
				                                  .Any(x => x.Any(y => y.HighlightedTextRequired)) ?? false);
		}

		return commands;
	}

	private static IEnumerable<Command> GetEnabledCommands(IEnumerable<string> folders)
	{
		return DeserializeFileContent(folders.SelectMany(x => Directory.GetFiles(x, "*.auto", new EnumerationOptions
		{
			RecurseSubdirectories = true
		}))).Where(x => x.Enabled);
	}

	private static IEnumerable<Command> DeserializeFileContent(IEnumerable<string> files) => files.SelectMany(file =>
	{
		try
		{
			return JsonSerializer.Deserialize<List<Command>>(File.ReadAllText(file));
		}
		catch (Exception ex)
		{
			Log.Error($"Error loading commands {file}: {ex}");
			return null;
		}
	});
}