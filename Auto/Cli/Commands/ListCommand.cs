using Auto.Cli.Serialization;
using Auto.Cli.Services;
using System.CommandLine;
using CliCommand = System.CommandLine.Command;

namespace Auto.Cli.Commands;

public static class ListCommand
{
	public static CliCommand Create(Option<string> configDirOption)
	{
		var command = new CliCommand("list") { Description = "List all commands" }
			.AddOption<string>("--file", "Filter by file path", out var fileOption)
			.AddOption<bool>("--enabled", "Show only enabled commands", out var enabledOption)
			.AddOption<bool>("--disabled", "Show only disabled commands", out var disabledOption)
			.AddOption<string>("--search", "Filter by name/description", out var searchOption)
			.AddOption<bool>("--json", "Output as JSON", out var jsonOption);

		command.SetAction(parseResult =>
		{
			var configDir = parseResult.GetValue(configDirOption);
			var file = parseResult.GetValue(fileOption);
			var enabled = parseResult.GetValue(enabledOption);
			var disabled = parseResult.GetValue(disabledOption);
			var search = parseResult.GetValue(searchOption);
			var json = parseResult.GetValue(jsonOption);

			var store = new CommandStore(configDir);
			var all = store.LoadAll();

			if (file != null)
			{
				var resolved = store.ResolvePath(file);
				all = [.. all.Where(x => x.File.Equals(resolved, StringComparison.OrdinalIgnoreCase))];
			}
			if (enabled || disabled)
				all = [.. all.Where(x => (enabled && x.Command.Enabled) || (disabled && !x.Command.Enabled))];
			if (search != null)
				all = [.. all.Where(x =>
					x.Command.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
					x.Command.Description.Contains(search, StringComparison.OrdinalIgnoreCase))];

			var sorted = all.OrderBy(x => x.Command.Name).ToList();

			if (json)
			{
				PrintJsonResult(sorted);
			}
			else
			{
				PrintTableResult(sorted, store, all.Count);
			}

			return 0;
		});

		return command;
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