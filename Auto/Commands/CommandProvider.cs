using System.IO;
using System.Text.Json;

using Auto.Models;

using Microsoft.Extensions.Logging;

namespace Auto.Commands;

internal interface ICommandProvider
{
	public bool TryGetCommand(HashSet<ushort> pressedKeys, ushort vkCode, out Command? command);
}

internal partial class CommandProvider : ICommandProvider
{
	private readonly ILogger<CommandProvider> _logger;
	private List<Command> _commands = [];

	public CommandProvider(ILogger<CommandProvider> logger)
	{
		_logger = logger;
		InitializeCommands();
	}

	public bool TryGetCommand(HashSet<ushort> pressedKeys, ushort vkCode, out Command? command)
	{
		command = _commands.FirstOrDefault(x => x.Trigger.Check(pressedKeys, vkCode));
		return command != null;
	}

	private void InitializeCommands()
	{
		var configFolder = Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config", "auto"
		);

		var commands = GetEnabledCommands(configFolder).ToList();
		foreach (var command in commands)
		{
			command.ClipboardTextRequired = VariableReferenced(command, "Clipboard");
			command.HighlightedTextRequired = VariableReferenced(command, "Highlighted");
		}

		_commands = commands;

		bool VariableReferenced(Command command, string variableName)
			=> command.Actions.SelectMany(a => a.Arguments).SelectMany(a => a.Tokens)
				.Any(a => a.Type == ArgumentType.Variable && a.Value.Equals(variableName));
	}

	private IEnumerable<Command> GetEnabledCommands(string folder)
	{
		if (!Directory.Exists(folder))
			return [];

		return DeserializeFileContent(Directory.GetFiles(folder, "*.json", new EnumerationOptions
		{
			RecurseSubdirectories = true
		})).Where(x => x.Enabled);
	}

	private IEnumerable<Command> DeserializeFileContent(IEnumerable<string> files) => files.SelectMany(file =>
	{
		try
		{
			return JsonSerializer.Deserialize<List<Command>>(File.ReadAllText(file)) ?? [];
		}
		catch (Exception ex)
		{
			LogErrorLoadingCommands(ex, file);
			return [];
		}
	});

	[LoggerMessage(LogLevel.Error, Message = "Error loading commands from file: {file}")]
	public partial void LogErrorLoadingCommands(Exception ex, string file);
}