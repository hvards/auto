using System.CommandLine;

using Auto.Cli.Services;
using Auto.Models;

using CliCommand = System.CommandLine.Command;

namespace Auto.Cli.Commands;

internal static class ActionEditCommand
{
	private record Input(
		string NameOrId,
		int Index,
		string[]? Args,
		string? Variable
	);

	public static CliCommand Create(Func<ParseResult, CommandStore> resolveStore)
	{
		var command = new CliCommand("edit") { Description = "Edit an existing action in a command" }
			.AddArgument<string>("name-or-id", "Command name or ID", out var nameArg)
			.AddArgument<int>("index", "Action index to edit", out var indexArg)
			.AddOption<string[]>("--arg", "Replacement arguments", out var argOption)
			.AddOption<string>("--var", "Updated output variable name", out var varOption);

		command.SetActionWithErrorHandling(pr => Execute(
			resolveStore(pr),
			new Input(
				pr.GetValue(nameArg) ?? string.Empty,
				pr.GetValue(indexArg),
				pr.GetValue(argOption),
				pr.GetValue(varOption)
			)
		));

		return command;
	}

	private static void Execute(CommandStore store, Input input)
	{
		if ((input.Args == null || input.Args.Length == 0) && input.Variable == null)
			throw new ArgumentException("--arg or --var must be provided");

		var command = store.Update(input.NameOrId, target =>
		{
			var sorted = target.Actions.OrderBy(a => a.Order).ToArray();
			if (input.Index < 0 || input.Index >= sorted.Length)
				throw new ArgumentException($"Index {input.Index} out of range (0..{sorted.Length - 1})");

			var action = sorted[input.Index];

			if (input.Args != null)
			{
				action.Arguments = [.. input.Args.Select(ArgParser.ParsePluginArgument)];
			}

			if (input.Variable != null)
			{
				action.Variable = input.Variable == string.Empty ? null : input.Variable;
			}

			var orders = ActionValidator.ComputeOrder(target.Actions);
			foreach (var a in target.Actions)
				a.Order = orders[a];
		});

		Console.WriteLine($"Updated action for '{command.Name}'");
	}
}