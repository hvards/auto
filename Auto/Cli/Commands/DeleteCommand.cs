using System.CommandLine;

using Auto.Cli.Services;

using CliCommand = System.CommandLine.Command;

namespace Auto.Cli.Commands;

internal static class DeleteCommand
{
	private record DeleteInput(string NameOrId);

	public static CliCommand Create(Func<ParseResult, CommandStore> resolveStore)
	{
		var command = new CliCommand("delete") { Description = "Delete a command" }
			.AddArgument<string>("name-or-id", "Command name or ID", out var nameArg);

		command.SetActionWithErrorHandling(
			pr => Execute(
				resolveStore(pr),
				new DeleteInput(pr.GetValue(nameArg) ?? string.Empty)
			)
		);

		return command;
	}

	private static void Execute(CommandStore store, DeleteInput input)
	{
		var (file, cmd) = store.GetCommand(input.NameOrId);

		var commands = CommandStore.LoadFile(file);
		commands.RemoveAll(c => c.Id == cmd.Id);
		CommandStore.SaveFile(file, commands);
		Console.WriteLine($"Deleted '{cmd.Name}'");
	}
}
