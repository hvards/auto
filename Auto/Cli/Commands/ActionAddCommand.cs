using System.CommandLine;

using Auto.Cli.Services;
using Auto.Models;
using Auto.PluginUtils;

using CliCommand = System.CommandLine.Command;

namespace Auto.Cli.Commands;

internal static class ActionAddCommand
{
	private record ActionAddInput(
		string NameOrId,
		string? Plugin,
		string? PowerShell,
		string[] Args,
		string? Variable
	);

	public static CliCommand Create(Func<ParseResult, CommandStore> resolveStore)
	{
		var command = new CliCommand("add") { Description = "Add an action to a command" }
			.AddArgument<string>("name-or-id", "Command name or ID", out var nameArg)
			.AddOption<string>("--plugin", "Plugin name or GUID", out var pluginOption)
			.AddOption<string>("--powershell", "PowerShell script path", out var psOption)
			.AddOption<string[]>("--arg", "Action arguments", out var argOption)
			.AddOption<string>("--var", "Output variable name", out var varOption);

		command.SetActionWithErrorHandling(pr => Execute(
			resolveStore(pr),
			new ActionAddInput(
				pr.GetValue(nameArg) ?? string.Empty,
				pr.GetValue(pluginOption),
				pr.GetValue(psOption),
				pr.GetValue(argOption) ?? [],
				pr.GetValue(varOption)
			)
		));

		return command;
	}

	private static void Execute(CommandStore store, ActionAddInput input)
	{
		if (input.Plugin == null && input.PowerShell == null)
			throw new ArgumentException("Either --plugin or --powershell is required");
		if (input.Plugin != null && input.PowerShell != null)
			throw new ArgumentException("--plugin and --powershell are mutually exclusive");

		var action = BuildAction(input);

		store.Update(input.NameOrId, target =>
		{
			target.Actions = [.. target.Actions, action];
			var orders = ActionValidator.ComputeOrder(target.Actions);
			foreach (var a in target.Actions)
				a.Order = orders[a];
		});

		Console.WriteLine($"Added action to '{input.NameOrId}'");
	}

	private static CommandAction BuildAction(ActionAddInput input)
	{
		var result = new CommandAction { Variable = input.Variable };

		if (input.Plugin != null)
		{
			result.Type = ActionType.Plugin;
			result.Target = PluginLoader.ResolvePlugin(input.Plugin);
			result.Arguments = [.. input.Args.Select(ArgParser.ParsePluginArgument)];
		}
		else if (input.PowerShell != null)
		{
			result.Type = ActionType.PowerShell;
			result.Target = input.PowerShell;
			result.Arguments = [.. input.Args.Select(ArgParser.ParsePowerShellArgument)];
		}

		return result;
	}
}