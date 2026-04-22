using System.CommandLine;

using Auto.Cli.Services;

using CliCommand = System.CommandLine.Command;
using ICommandExecutor = Auto.Commands.ICommandExecutor;

namespace Auto.Cli.Commands;

internal static class ExecuteCommand
{
	private record ExecuteInput(string NameOrId, string? Clipboard, string? Highlighted);

	internal static CliCommand Create(Func<ParseResult, CommandStore> resolveStore, ICommandExecutor commandExecutor)
	{
		var command = new CliCommand("execute") { Description = "Test execution of command actions" }
			.AddArgument<string>("name-or-id", "Command name or ID", out var nameArg)
			.AddOption<string>("clipboard", "Clipboard variable value", out var clipboardText)
			.AddOption<string>("highlighted", "Highlighted text variable value", out var highlightedText);

		command.SetActionWithErrorHandling(pr => Execute(
			resolveStore(pr),
			commandExecutor,
			new ExecuteInput(
				pr.GetValue(nameArg) ?? string.Empty,
				pr.GetValue(clipboardText),
				pr.GetValue(highlightedText)
			)
		));

		return command;
	}

	private static void Execute(CommandStore store, ICommandExecutor commandExecutor, ExecuteInput input)
	{
		var (_, commandEntry) = store.GetCommand(input.NameOrId);
		var command = new Models.Command
		{
			Actions = commandEntry.Actions
		};

		AdminCheck.WarnIfNotAdmin();

		Console.WriteLine($"Executing command actions for \"{commandEntry.Name}\" ({commandEntry.Id}).");
		commandExecutor.ExecuteCommand(command, input.Clipboard, input.Highlighted);
		Console.WriteLine("Command action execution finished.");
	}
}