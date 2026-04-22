using System.IO;

using Auto.Cli.Services;
using Auto.Models;

using CliCommand = System.CommandLine.Command;

namespace Auto.Cli.Commands;

internal class AddCommand(ICommandStoreFactory storeFactory, ITriggerCreator triggerCreator) : ICliCommand
{
	private record AddInput(
		string Name,
		string File,
		string? Description,
		bool Disabled,
		string[]? Combination,
		string[]? Sequence
	);

	public CliCommand Build()
	{
		var command = new CliCommand("add") { Description = "Create a new command" }
			.AddArgument<string>("name", "Command name", out var nameArg)
			.AddOption("--file", "Target JSON file (relative to commands dir, default: default.json)",
				out var fileOption, defaultValue: "default.json")
			.AddOption<string[]>("--combination", "Key combination, or omit value to record interactively",
				out var combinationOption, argumentRequired: false)
			.AddOption<string[]>("--sequence", "Key sequence, or omit value to record interactively",
				out var sequenceOption, argumentRequired: false)
			.AddOption<string>("--description", "Command description", out var descOption)
			.AddOption<bool>("--disabled", "Create as disabled", out var disabledOption);

		command.SetActionWithErrorHandling(pr => Execute(
			storeFactory.Create(pr),
			new AddInput(
				pr.GetValue(nameArg) ?? string.Empty,
				pr.GetValue(fileOption) ?? string.Empty,
				pr.GetValue(descOption),
				pr.GetValue(disabledOption),
				pr.GetResult(combinationOption) != null ? pr.GetValue(combinationOption) : null,
				pr.GetResult(sequenceOption) != null ? pr.GetValue(sequenceOption) : null
			)
		));

		return command;
	}

	private void Execute(CommandStore store, AddInput input)
	{
		var path = store.ResolvePath(input.File);

		var trigger = triggerCreator.CreateTrigger(input.Combination, input.Sequence);

		var cmd = new CommandEntry
		{
			Id = Guid.NewGuid(),
			Name = input.Name,
			Description = input.Description ?? string.Empty,
			Enabled = !input.Disabled,
			Trigger = trigger,
			Actions = []
		};

		var duplicate = store.LoadAll().FirstOrDefault(x => string.Equals(x.Command.Name, input.Name));
		if (duplicate.Command != null)
			Console.Error.WriteLine($"Warning: '{duplicate.Command.Name}' already exists (id: {duplicate.Command.Id})");

		var existing = File.Exists(path) ? CommandStore.LoadFile(path) : [];
		existing.Add(cmd);
		CommandStore.SaveFile(path, existing);

		Console.WriteLine($"Added '{input.Name}' to {input.File} (id: {cmd.Id})");
	}
}
