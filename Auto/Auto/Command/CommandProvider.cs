using System.IO;
using System.Text.Json;
using Auto.Interfaces;

namespace Auto.Command;

public class CommandProvider : ICommandProvider
{
	private List<Command> _commands;

	public CommandProvider()
	{
		InitializeCommands();
	}

	public bool TryGetCommand(HashSet<ushort> pressedKeys, ushort vkCode, out Command command)
	{
        command = _commands.FirstOrDefault(x => x.Trigger.Check(pressedKeys, vkCode));
		return command != null;
	}

	private void InitializeCommands()
	{
        var commandFolders =
            (Environment.GetEnvironmentVariable("Auto", EnvironmentVariableTarget.Machine) ??
             throw new Exception("Missing auto folders")).Split(";")
            .Where(Directory.Exists).ToList();

		var commands = GetEnabledCommands(commandFolders).ToList();
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

		_commands = commands;
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