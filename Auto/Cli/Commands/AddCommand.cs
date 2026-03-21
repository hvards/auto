using Auto.Cli.Services;
using Auto.Models;
using System.CommandLine;
using System.IO;

using CliCommand = System.CommandLine.Command;

namespace Auto.Cli.Commands;

public static class AddCommand
{
	private record AddInput(
		string Name,
		string File,
		string? Description,
		bool Disabled,
		string[] Combination,
		string[] Sequence
	);

	public static CliCommand Create(Func<ParseResult, CommandStore> resolveStore)
	{
		var command = new CliCommand("add") { Description = "Create a new command" }
			.AddArgument<string>("name", "Command name", out var nameArg)
			.AddOption("--file", "Target JSON file (relative to commands dir, default: default.json)",
				out var fileOption, defaultValue: "default.json")
			.AddOption<string[]>("--combination", "Key combination (e.g. LCtrl LWin LAlt R)", out var combinationOption)
			.AddOption<string[]>("--sequence", "Key sequence (e.g. E X A M P L E)", out var sequenceOption)
			.AddOption<string>("--description", "Command description", out var descOption)
			.AddOption<bool>("--disabled", "Create as disabled", out var disabledOption);

		command.SetActionWithErrorHandling(pr => Execute(
			resolveStore(pr),
			new AddInput(
				pr.GetValue(nameArg) ?? string.Empty,
				pr.GetValue(fileOption) ?? string.Empty,
				pr.GetValue(descOption),
				pr.GetValue(disabledOption),
				pr.GetValue(combinationOption) ?? [],
				pr.GetValue(sequenceOption) ?? []
			)
		));

		return command;
	}

	private static void Execute(CommandStore store, AddInput input)
	{
		var path = store.ResolvePath(input.File);

		var trigger = KeyNameResolver.ParseTrigger(input.Combination, input.Sequence);

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
