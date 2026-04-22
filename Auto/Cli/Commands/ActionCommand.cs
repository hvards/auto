using CliCommand = System.CommandLine.Command;

namespace Auto.Cli.Commands;

internal class ActionCommand(
	ActionAddCommand addCmd,
	ActionEditCommand editCmd,
	ActionDeleteCommand deleteCmd) : ICliCommand
{
	public CliCommand Build()
	{
		var command = new CliCommand("action") { Description = "Manage command actions" };
		command.Subcommands.Add(addCmd.Build());
		command.Subcommands.Add(editCmd.Build());
		command.Subcommands.Add(deleteCmd.Build());
		return command;
	}
}
