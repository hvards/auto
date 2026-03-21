using Auto.Cli.Services;
using Auto.Models;
using System.CommandLine;

using CliCommand = System.CommandLine.Command;

namespace Auto.Cli.Commands;

public static class ActionDeleteCommand
{
	private record Input(
		string NameOrId,
		int Index
	);

	public static CliCommand Create(Func<ParseResult, CommandStore> resolveStore)
	{
		var command = new CliCommand("delete") { Description = "Delete an action from a command" }
			.AddArgument<string>("name-or-id", "Command name or ID", out var nameArg)
			.AddArgument<int>("Index", "Delete by index", out var indexArg);

		command.SetActionWithErrorHandling(pr => Execute(
			resolveStore(pr),
			new Input(
				pr.GetValue(nameArg) ?? string.Empty,
				pr.GetValue(indexArg)
			)
		));

		return command;
	}	

	private static void Execute(CommandStore store, Input input)
	{
		var command = store.Update(input.NameOrId, target =>
		{
			var toRemove = GetActionToDelete(target.Actions, input.Index);
			var remaining = target.Actions.Where(a => a != toRemove).ToArray();

			var orders = ActionValidator.ComputeOrder(remaining);
			foreach (var a in remaining)
				a.Order = orders[a];

			target.Actions = remaining;
		});

		Console.WriteLine($"Deleted action from '{command.Name}'");
	}

	private static CommandAction GetActionToDelete(CommandAction[] actions, int index)
	{
		if (index < 0 || index >= actions.Length) 
			throw new ArgumentException($"Index {index} out of range (0..{actions.Length - 1})");

		return actions.OrderBy(a => a.Order).ToArray()[index];
	}
}
