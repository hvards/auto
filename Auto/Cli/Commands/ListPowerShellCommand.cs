using System.IO;
using System.Management.Automation.Language;
using System.Text.Json;

using CliCommand = System.CommandLine.Command;

namespace Auto.Cli.Commands;

internal static class ListPowerShellCommand
{
	private static readonly JsonSerializerOptions WriteOptions = new() { WriteIndented = true };

	public static CliCommand Create()
	{
		var command = new CliCommand("list-powershell") { Description = "List available PowerShell scripts" }
			.AddOption<bool>("--json", "Output as JSON", out var jsonOption);

		command.SetActionWithErrorHandling(pr =>
		{
			var scripts = GetScripts();
			if (pr.GetValue(jsonOption))
				PrintJsonResult(scripts);
			else
				PrintTableResult(scripts);
		});

		return command;
	}

	private record ScriptInfo(string Name, List<ScriptParameter> Parameters);
	internal record ScriptParameter(string Name, string Type);

	private static IEnumerable<ScriptInfo> GetScripts()
	{
		var folder = Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config", "auto", "powershell"
		);

		if (!Directory.Exists(folder))
			yield break;

		foreach (var file in Directory.EnumerateFiles(folder, "*.ps1"))
		{
			var content = File.ReadAllText(file);
			yield return new ScriptInfo(Path.GetFileName(file), ParseParameters(content));
		}
	}

	internal static List<ScriptParameter> ParseParameters(string content)
	{
		var ast = Parser.ParseInput(content, out _, out _);
		if (ast.Find(a => a is ParamBlockAst, false) is not ParamBlockAst paramBlock)
			return [];

		return [.. paramBlock.Parameters
			.Select(p => new ScriptParameter(
				p.Name.VariablePath.UserPath,
				p.StaticType != typeof(object) ? p.StaticType.Name : "object"))];
	}

	private static void PrintJsonResult(IEnumerable<ScriptInfo> scripts)
	{
		var projected = scripts.Select(s => new
		{
			s.Name,
			Arguments = s.Parameters.Select(p => new { p.Name, p.Type }).ToArray()
		});
		Console.WriteLine(JsonSerializer.Serialize(projected, WriteOptions));
	}

	private static void PrintTableResult(IEnumerable<ScriptInfo> scripts)
	{
		foreach (var s in scripts)
		{
			var args = string.Join(", ", s.Parameters.Select(p => $"{p.Name}:{p.Type}"));
			Console.WriteLine($"  {s.Name,-30} {args}");
		}
	}
}