using System.CommandLine;

using Auto.Cli.Services;

using CliCommand = System.CommandLine.Command;

namespace Auto.Cli.Commands;

internal static class EditCommand
{
	private record EditInput(
		string NameOrId,
		string[]? Combination,
		string[]? Sequence,
		string? Description,
		string? NewName
	);

	public static CliCommand Create(
		Func<ParseResult, CommandStore> resolveStore,
		ITriggerCreator triggerCreator)
	{
		var command = new CliCommand("edit") { Description = "Modify an existing command" }
			.AddArgument<string>("name-or-id", "Command name or ID", out var nameArg)
			.AddOption<string[]>("--combination", "New key combination, or omit value to record interactively",
				out var combinationOption, argumentRequired: false)
			.AddOption<string[]>("--sequence", "New key sequence, or omit value to record interactively",
				out var sequenceOption, argumentRequired: false)
			.AddOption<string>("--description", "New description", out var descOption)
			.AddOption<string>("--name", "New name", out var renameOption);

		command.SetActionWithErrorHandling(pr => Execute(
			resolveStore(pr),
			triggerCreator,
			new EditInput(
				pr.GetValue(nameArg) ?? string.Empty,
				pr.GetResult(combinationOption) != null ? pr.GetValue(combinationOption) : null,
				pr.GetResult(sequenceOption) != null ? pr.GetValue(sequenceOption) : null,
				pr.GetValue(descOption),
				pr.GetValue(renameOption)
			)
		));

		return command;
	}

	private static void Execute(CommandStore store, ITriggerCreator triggerCreator, EditInput input)
	{
		var cmd = store.Update(input.NameOrId, target =>
		{
			if (input.Combination != null)
			{
				target.Trigger.Combination = triggerCreator.GetCombination(input.Combination);
			}
			if (input.Sequence != null)
			{
				target.Trigger.Sequence = triggerCreator.GetSequence(input.Sequence);
			}

			target.Description = input.Description ?? target.Description;
			target.Name = input.NewName ?? target.Name;
		});

		Console.WriteLine($"Updated '{cmd.Name}'");
	}
}