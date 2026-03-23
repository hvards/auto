using System.CommandLine;

using Auto.Cli.Services;

using CliCommand = System.CommandLine.Command;

namespace Auto.Cli.Commands;

internal static class EditCommand
{
	private record EditInput(
		string NameOrId,
		string[] Combination,
		string[] Sequence,
		string? Description,
		string? NewName
	);

	public static CliCommand Create(Func<ParseResult, CommandStore> resolveStore)
	{
		var command = new CliCommand("edit") { Description = "Modify an existing command" }
			.AddArgument<string>("name-or-id", "Command name or ID", out var nameArg)
			.AddOption<string[]>("--combination", "New key combination", out var combinationOption)
			.AddOption<string[]>("--sequence", "New key sequence", out var sequenceOption)
			.AddOption<string>("--description", "New description", out var descOption)
			.AddOption<string>("--name", "New name", out var renameOption);

		command.SetActionWithErrorHandling(pr => Execute(
			resolveStore(pr),
			new EditInput(
				pr.GetValue(nameArg) ?? string.Empty,
				pr.GetValue(combinationOption) ?? [],
				pr.GetValue(sequenceOption) ?? [],
				pr.GetValue(descOption),
				pr.GetValue(renameOption)
			)
		));

		return command;
	}

	private static void Execute(CommandStore store, EditInput input)
	{
		var cmd = store.Update(input.NameOrId, target =>
		{
			if (input.Combination.Length > 0)
				target.Trigger.Combination = KeyNameResolver.ParseCombination(input.Combination);
			if (input.Sequence.Length > 0)
				target.Trigger.Sequence = KeyNameResolver.ParseSequence(input.Sequence);
			target.Description = input.Description ?? target.Description;
			target.Name = input.NewName ?? target.Name;
		});

		Console.WriteLine($"Updated '{cmd.Name}'");
	}
}