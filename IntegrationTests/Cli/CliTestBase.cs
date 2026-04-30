using Auto;
using Auto.Cli.Services;
using Auto.Models;
using Auto.PluginUtils;

using IntegrationTests.Stubs;

using Microsoft.Extensions.DependencyInjection;

namespace IntegrationTests.Cli;

internal abstract class CliTestBase
{
	private string _tempDir = null!;
	protected CommandStore TestCommandStore => new(_tempDir);

	protected static IPluginLoader TestPluginLoader => TestServicesFixture.Services.GetRequiredService<IPluginLoader>();
	protected static KeyRecorderStub TestKeyRecorder => (KeyRecorderStub)TestServicesFixture.Services.GetRequiredService<IKeyRecorder>();

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
		TestKeyRecorder.Reset();
	}

	protected async Task<(int ExitCode, string Stdout, string Stderr)> InvokeAsync(params string[] args)
	{
		var root = Program.BuildCli(TestServicesFixture.Services);
		var fullArgs = new List<string>(args) { "--config-dir", _tempDir };

		var stdOut = new StringWriter();
		var stdErr = new StringWriter();
		Console.SetOut(stdOut);
		Console.SetError(stdErr);
		var exitCode = await root.Parse([.. fullArgs]).InvokeAsync();
		return (exitCode, stdOut.ToString(), stdErr.ToString());
	}

	protected static CommandEntry GetCommand(
		string name = "Test",
		bool enabled = true,
		Guid? id = null,
		string[]? combination = null,
		string[]? sequence = null)
		=> new()
		{
			Id = id ?? Guid.NewGuid(),
			Name = name,
			Description = "Test description",
			Enabled = enabled,
			Trigger = new Trigger
			{
				Combination = [.. KeyNameResolver.ParseInput(combination ?? ["LCtrl", "LWin", "T"])],
				Sequence = sequence != null ? [.. KeyNameResolver.ParseInput(sequence)] : []
			},
			Actions =
			[
				new CommandAction
				{
					Target = TestPluginLoader.ResolvePlugin("StartProgram"),
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

	protected CommandEntry SeedCommand(
		string name = "Test",
		bool enabled = true,
		string[]? combination = null,
		string[]? sequence = null,
		string file = "test.json")
	{
		var cmd = GetCommand(name, enabled, combination: combination, sequence: sequence);
		var store = new CommandStore(_tempDir);
		var path = store.ResolvePath(file);
		var existing = File.Exists(path) ? CommandStore.LoadFile(path) : [];
		existing.Add(cmd);
		CommandStore.SaveFile(path, existing);
		return cmd;
	}
}
