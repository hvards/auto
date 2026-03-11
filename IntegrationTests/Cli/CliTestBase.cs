using Auto;
using Auto.Cli.Serialization;
using Auto.Cli.Services;
using Auto.Models;
using Auto.PluginUtils;

namespace IntegrationTests.Cli;

public abstract class CliTestBase
{
	protected string TempDir = null!;

	[SetUp]
	public void SetUp()
	{
		TempDir = Path.Combine(Path.GetTempPath(), "autocli-cmd-" + Guid.NewGuid().ToString("N")[..8]);
		Directory.CreateDirectory(Path.Combine(TempDir, "commands"));
	}

	[TearDown]
	public void TearDown()
	{
		if (Directory.Exists(TempDir)) Directory.Delete(TempDir, true);
	}

	protected async Task<(int ExitCode, string Stdout, string Stderr)> InvokeAsync(params string[] args)
	{
		var root = Program.BuildCli();
		var fullArgs = new List<string>(args) { "--config-dir", TempDir };

		var stdOut = new StringWriter();
		var stdErr = new StringWriter();
		Console.SetOut(stdOut);
		Console.SetError(stdErr);
		var exitCode = await root.Parse([.. fullArgs]).InvokeAsync();
		return (exitCode, stdOut.ToString(), stdErr.ToString());
	}

	protected static CommandEntry GetCommand(string name = "Test", bool enabled = true, Guid? id = null) => new()
	{
		Id = id ?? Guid.NewGuid(),
		Name = name,
		Description = "Test description",
		Enabled = enabled,
		Trigger = new Trigger
		{
			Combination = KeyNameResolver.ParseCombination(["LCtrl", "LWin", "T"]),
			Sequence = []
		},
		Actions = [new ArgumentToken { Type = ArgumentType.Plugin, Value = PluginLoader.ResolvePlugin("StartProgram") }],
		PowerShellArguments = [],
		PluginArguments = new()
		{
			[PluginLoader.ResolvePlugin("StartProgram")] =
			[
				new CommandArgument
				{
					Tokens = [new ArgumentToken { Type = ArgumentType.Text, Value = "https://example.com" }]
				}
			]
		}
	};

	protected CommandEntry SeedCommand(string name = "Test", bool enabled = true)
	{
		var cmd = GetCommand(name, enabled);
		var store = new CommandStore(TempDir);
		CommandStore.SaveFile(store.ResolvePath("test.json"), [cmd]);
		return cmd;
	}

	protected string CreateImportSource(string name, Guid? id = null)
	{
		var cmd = GetCommand(name, id: id ?? Guid.NewGuid());
		var path = Path.Combine(TempDir, $"import-{Guid.NewGuid():N}.json");
		File.WriteAllText(path, CommandSerializer.Serialize([cmd]));
		return path;
	}
}