using Auto.Cli.Services;
using System.CommandLine;
using CliCommand = System.CommandLine.Command;

namespace Auto.Cli.Commands;

public static class EnableDisableCommand
{
	private record EnableDisableInput(string ConfigDir, string NameOrId, bool Enable);

	public static CliCommand CreateEnable(Option<string> configDirOption) => Create(configDirOption, true);
	public static CliCommand CreateDisable(Option<string> configDirOption) => Create(configDirOption, false);

	private static CliCommand Create(Option<string> configDirOption, bool enable)
	{
		var verb = enable ? "enable" : "disable";
		var command = new CliCommand(verb) { Description = $"{(enable ? "Enable" : "Disable")} a command" }
			.AddArgument<string>("name-or-id", "Command name or ID", out var nameArg);

		command.SetActionWithErrorHandling(pr =>
			Execute(new EnableDisableInput(pr.GetValue(configDirOption), pr.GetValue(nameArg), enable))
		);

		return command;
	}

	private static void Execute(EnableDisableInput input)
	{
		var store = new CommandStore(input.ConfigDir);
		var (file, cmd) = store.GetCommand(input.NameOrId);
		var commands = CommandStore.LoadFile(file);
		var target = commands.First(c => c.Id == cmd.Id);
		target.Enabled = input.Enable;
		CommandStore.SaveFile(file, commands);
		Console.WriteLine($"{(input.Enable ? "Enabled" : "Disabled")} '{cmd.Name}'");
	}
}