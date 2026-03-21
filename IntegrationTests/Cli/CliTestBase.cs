using Auto;
using Auto.Cli.Services;
using Auto.Models;
using Auto.PluginUtils;

namespace IntegrationTests.Cli;

public abstract class CliTestBase
{
	private string _tempDir = null!;
	protected CommandStore TestCommandStore => new(_tempDir);

	[SetUp]
	public void SetUp()
	{
		_tempDir = Path.Combine(Path.GetTempPath(), "autocli-cmd-" + Guid.NewGuid().ToString("N")[..8]);
		Directory.CreateDirectory(Path.Combine(_tempDir, "commands"));
	}

	[TearDown]
	public void TearDown()
	{
		if (Directory.Exists(_tempDir)) Directory.Delete(_tempDir, true);
	}

	protected async Task<(int ExitCode, string Stdout, string Stderr)> InvokeAsync(params string[] args)
	{
		var root = Program.BuildCli();
		var fullArgs = new List<string>(args) { "--config-dir", _tempDir };

		var stdOut = new StringWriter();
		var stdErr = new StringWriter();
		Console.SetOut(stdOut);
		Console.SetError(stdErr);
		var exitCode = await root.Parse([.. fullArgs]).InvokeAsync();
		return (exitCode, stdOut.ToString(), stdErr.ToString());
	}

	protected static CommandEntry GetCommand(string name = "Test", bool enabled = true, Guid? id = null)
		=> new()
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
			Actions =
			[
				new CommandAction
				{
					Type = ActionType.Plugin,
					Target = PluginLoader.ResolvePlugin("StartProgram"),
					Order = 0,
					Arguments =
					[
						new CommandArgument
						{
							Tokens = [new ArgumentToken { Type = ArgumentType.Text, Value = "https://example.com" }]
						}
					]
				}
			]
		};

	protected CommandEntry SeedCommand(string name = "Test", bool enabled = true)
	{
		var cmd = GetCommand(name, enabled);
		var store = new CommandStore(_tempDir);
		CommandStore.SaveFile(store.ResolvePath("test.json"), [cmd]);
		return cmd;
	}
}
