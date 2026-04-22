using Auto.Cli.Services;

using CliCommand = System.CommandLine.Command;
using ICommandExecutor = Auto.Commands.ICommandExecutor;

namespace Auto.Cli.Commands;

internal class ExecuteCommand(ICommandStoreFactory storeFactory, ICommandExecutor commandExecutor) : ICliCommand
{
	private record ExecuteInput(string NameOrId, string? Clipboard, string? Highlighted);

	public CliCommand Build()
	{
		var command = new CliCommand("execute") { Description = "Test execution of command actions" }
			.AddArgument<string>("name-or-id", "Command name or ID", out var nameArg)
			.AddOption<string>("clipboard", "Clipboard variable value", out var clipboardText)
			.AddOption<string>("highlighted", "Highlighted text variable value", out var highlightedText);

		command.SetActionWithErrorHandling(pr => Execute(
			storeFactory.Create(pr),
			new ExecuteInput(
				pr.GetValue(nameArg) ?? string.Empty,
				pr.GetValue(clipboardText),
				pr.GetValue(highlightedText)
			)
		));

		return command;
	}

	private void Execute(CommandStore store, ExecuteInput input)
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
