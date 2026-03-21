using Auto.Cli.Services;
using System.CommandLine;
using System.CommandLine.Parsing;

using CliCommand = System.CommandLine.Command;

namespace Auto.Cli.Commands;

public static class ActionCommand
{
	public static CliCommand Create(Func<ParseResult, CommandStore> resolveStore)
	{
		var command = new CliCommand("action") { Description = "Manage command actions" };
		command.Subcommands.Add(ActionAddCommand.Create(resolveStore));
		command.Subcommands.Add(ActionEditCommand.Create(resolveStore));
		command.Subcommands.Add(ActionDeleteCommand.Create(resolveStore));
		return command;
	}
}
