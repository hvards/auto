using Auto.Cli.Serialization;
using Auto.Cli.Services;
using System.CommandLine;

using CliCommand = System.CommandLine.Command;

namespace Auto.Cli.Commands;

public static class ListCommand
{
	private record ListInput(string? File, bool Enabled, bool Disabled, string? Search, bool Json);
	public static CliCommand Create(Func<ParseResult, CommandStore> resolveStore)
	{
		var command = new CliCommand("list") { Description = "List all commands" }
			.AddOption<string>("--file", "Filter by file path", out var fileOption)
			.AddOption<bool>("--enabled", "Show only enabled commands", out var enabledOption)
			.AddOption<bool>("--disabled", "Show only disabled commands", out var disabledOption)
			.AddOption<string>("--search", "Filter by name/description", out var searchOption)
			.AddOption<bool>("--json", "Output as JSON", out var jsonOption);

		command.SetActionWithErrorHandling(pr => Execute(
			resolveStore(pr),
			new ListInput(
				pr.GetValue(fileOption),
				pr.GetValue(enabledOption),
				pr.GetValue(disabledOption),
				pr.GetValue(searchOption),
				pr.GetValue(jsonOption)
			)
		));

		return command;
	}

	private static void Execute(CommandStore store, ListInput input)
	{
		var all = store.LoadAll();

		if (input.File != null)
		{
			var resolved = store.ResolvePath(input.File);
			all = [.. all.Where(x => x.File.Equals(resolved, StringComparison.OrdinalIgnoreCase))];
		}
		if (input.Enabled || input.Disabled)
			all = [.. all.Where(x => (input.Enabled && x.Command.Enabled) || (input.Disabled && !x.Command.Enabled))];
		if (input.Search != null)
			all = [.. all.Where(x =>
				x.Command.Name.Contains(input.Search, StringComparison.OrdinalIgnoreCase) ||
				x.Command.Description.Contains(input.Search, StringComparison.OrdinalIgnoreCase))];

		var sorted = all.OrderBy(x => x.Command.Name).ToList();

		if (input.Json)
		{
			PrintJsonResult(sorted);
		}
		else
		{
			PrintTableResult(sorted, store, all.Count);
		}
	}

	private static void PrintJsonResult(List<(string File, Models.CommandEntry Command)> result)
	{
		Console.WriteLine(CommandSerializer.Serialize(result.Select(x => x.Command)));
	}

	private static void PrintTableResult(
		List<(string File, Models.CommandEntry command)> result, 
		CommandStore store, 
		int totalCount)
	{
		foreach (var (f, cmd) in result)
		{
			var status = cmd.Enabled ? "on " : "off";
			var rel = store.GetRelativePath(f);
			Console.WriteLine($"  [{status}]  {cmd.Id}  {cmd.Name,-40} {rel}");
		}

		Console.WriteLine();
		Console.WriteLine($"{totalCount} command(s)");
	}
}