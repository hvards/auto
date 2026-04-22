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
		string Plugin,
		string[] Args,
		string? Variable
	);

	public static CliCommand Create(Func<ParseResult, CommandStore> resolveStore, IPluginLoader pluginLoader)
	{
		var command = new CliCommand("add") { Description = "Add an action to a command" }
			.AddArgument<string>("name-or-id", "Command name or ID", out var nameArg)
			.AddArgument<string>("plugin", "Plugin name or GUID", out var pluginArg)
			.AddOption<string[]>("--arg", "Action arguments", out var argOption)
			.AddOption<string>("--var", "Output variable name", out var varOption);

		command.SetActionWithErrorHandling(pr => Execute(
			resolveStore(pr),
			pluginLoader,
			new ActionAddInput(
				pr.GetValue(nameArg) ?? string.Empty,
				pr.GetValue(pluginArg) ?? string.Empty,
				pr.GetValue(argOption) ?? [],
				pr.GetValue(varOption)
			)
		));

		return command;
	}

	private static void Execute(CommandStore store, IPluginLoader pluginLoader, ActionAddInput input)
	{
		var action = new CommandAction
		{
			Target = pluginLoader.ResolvePlugin(input.Plugin),
			Arguments = [.. input.Args.Select(ArgParser.ParsePluginArgument)],
			Variable = input.Variable
		};

		store.Update(input.NameOrId, target =>
		{
			target.Actions = [.. target.Actions, action];
			var orders = ActionValidator.ComputeOrder(target.Actions);
			foreach (var a in target.Actions)
				a.Order = orders[a];
		});

		Console.WriteLine($"Added action to '{input.NameOrId}'");
	}
}
