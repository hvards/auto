using Auto.Cli.Services;
using System.CommandLine;
using CliCommand = System.CommandLine.Command;

namespace Auto.Cli.Commands;

public static class EnableDisableCommand
{
	public static CliCommand CreateEnable(Option<string> configDirOption) => Create(configDirOption, true);
	public static CliCommand CreateDisable(Option<string> configDirOption) => Create(configDirOption, false);

	private static CliCommand Create(Option<string> configDirOption, bool enable)
	{
		var verb = enable ? "enable" : "disable";
		var command = new CliCommand(verb) { Description = $"{(enable ? "Enable" : "Disable")} a command" }
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
			var target = commands.First(c => c.Id == cmd.Id);
			target.Enabled = enable;
			CommandStore.SaveFile(file, commands);
			Console.WriteLine($"{(enable ? "Enabled" : "Disabled")} '{cmd.Name}'");
			return 0;
		});

		return command;
	}
}