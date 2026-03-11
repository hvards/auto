using Auto.Cli.Services;
using System.CommandLine;
using CliCommand = System.CommandLine.Command;

namespace Auto.Cli.Commands;

public static class DeleteCommand
{
	private record DeleteInput(string ConfigDir, string NameOrId);

	public static CliCommand Create(Option<string> configDirOption)
	{
		var command = new CliCommand("delete") { Description = "Delete a command" }
			.AddArgument<string>("name-or-id", "Command name or ID", out var nameArg);

		command.SetActionWithErrorHandling(
			pr => Execute(
				new DeleteInput(pr.GetValue(configDirOption) ?? string.Empty, pr.GetValue(nameArg) ?? string.Empty)
			)
		);

		return command;
	}

	private static void Execute(DeleteInput input)
	{
		var store = new CommandStore(input.ConfigDir);
		var (file, cmd) = store.GetCommand(input.NameOrId);

		var commands = CommandStore.LoadFile(file);
		commands.RemoveAll(c => c.Id == cmd.Id);
		CommandStore.SaveFile(file, commands);
		Console.WriteLine($"Deleted '{cmd.Name}'");
	}
}