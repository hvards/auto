using System.Text.Json;

using Auto.PluginUtils;

using CliCommand = System.CommandLine.Command;

namespace Auto.Cli.Commands;

internal static class ListPluginsCommand
{
	private static readonly JsonSerializerOptions WriteOptions = new() { WriteIndented = true };

	public static CliCommand Create(IPluginLoader pluginLoader)
	{
		var command = new CliCommand("list-plugins") { Description = "List available plugins" }
			.AddOption<bool>("--json", "Output as JSON", out var jsonOption);

		command.SetActionWithErrorHandling(pr =>
		{
			var plugins = pluginLoader.GetAvailablePluginDetails().ToList();
			if (pr.GetValue(jsonOption))
			{
				PrintJsonResult(plugins);
			}
			else
			{
				PrintTableResult(plugins);
			}
		});

		return command;
	}

	private static void PrintJsonResult(IEnumerable<PluginDetail> plugins)
	{
		var projected = plugins.Select(p => new
		{
			p.Id,
			p.Name,
			p.Description,
			Arguments = p.ExpectedArguments.Select(a => new { a.Name, Type = a.Type.Name }).ToArray()
		});
		Console.WriteLine(JsonSerializer.Serialize(projected, WriteOptions));
	}

	private static void PrintTableResult(IEnumerable<PluginDetail> plugins)
	{
		foreach (var p in plugins)
		{
			var args = string.Join(", ", p.ExpectedArguments.Select(a => $"{a.Name}:{a.Type.Name}"));
			Console.WriteLine($"  {p.Id}  {p.Name,-20} {args}");
		}
	}
}
