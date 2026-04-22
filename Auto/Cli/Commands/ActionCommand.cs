using System.CommandLine;

using Auto.Cli.Services;
using Auto.PluginUtils;

using CliCommand = System.CommandLine.Command;

namespace Auto.Cli.Commands;

internal static class ActionCommand
{
	public static CliCommand Create(Func<ParseResult, CommandStore> resolveStore, IPluginLoader pluginLoader)
	{
		var command = new CliCommand("action") { Description = "Manage command actions" };
		command.Subcommands.Add(ActionAddCommand.Create(resolveStore, pluginLoader));
		command.Subcommands.Add(ActionEditCommand.Create(resolveStore));
		command.Subcommands.Add(ActionDeleteCommand.Create(resolveStore));
		return command;
	}
}
