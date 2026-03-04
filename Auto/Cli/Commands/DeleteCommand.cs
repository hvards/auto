using Auto.Cli.Services;
using System.CommandLine;
using CliCommand = System.CommandLine.Command;

namespace Auto.Cli.Commands;

public static class DeleteCommand
{
	public static CliCommand Create(Option<string> configDirOption)
	{
		var command = new CliCommand("delete") { Description = "Delete a command" }
			.AddArgument<string>("name-or-id", "Command name or ID", out var nameArg);

		command.SetAction(parseResult =>
		{
			var configDir = parseResult.GetValue(configDirOption);
			var nameOrId = parseResult.GetValue(nameArg);

			var store = new CommandStore(configDir);
			if (!store.FindCommand(nameOrId, out var found))
				return 1;
			var (file, cmd) = found;

			var commands = CommandStore.LoadFile(file);
			commands.RemoveAll(c => c.Id == cmd.Id);
			CommandStore.SaveFile(file, commands);
			Console.WriteLine($"Deleted '{cmd.Name}'");
			return 0;
		});

		return command;
	}
}