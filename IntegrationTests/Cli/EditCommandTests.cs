using Auto.Cli.Services;
using Auto.Models;
using Auto.PluginUtils;

namespace IntegrationTests.Cli;

[TestFixture]
public class EditCommandTests : CliTestBase
{
	[Test]
	public async Task Edit_UpdatesTrigger()
	{
		SeedCommand();

		var (exit, stdout, _) = await InvokeAsync("edit", "Test", "--combination", "LCtrl", "LWin", "R");

		Assert.That(exit, Is.Zero);
		Assert.That(stdout.TrimEnd(), Is.EqualTo("Updated 'Test'"));

		var store = new CommandStore(TempDir);
		var result = store.Find("Test");
		Assert.That(result, Is.Not.Null);
		var expected = KeyNameResolver.FormatCombination(KeyNameResolver.ParseCombination(["LCtrl", "LWin", "R"]));
		Assert.That(KeyNameResolver.FormatCombination(result!.Value.Command.Trigger.Combination),
			Is.EqualTo(expected));
	}

	[Test]
	public async Task Edit_RenamesCommand()
	{
		SeedCommand();

		var (exit, stdout, _) = await InvokeAsync("edit", "Test", "--name", "Renamed");

		Assert.That(exit, Is.Zero);
		Assert.That(stdout.TrimEnd(), Is.EqualTo("Updated 'Renamed'"));

		var store = new CommandStore(TempDir);
		Assert.That(store.Find("Renamed"), Is.Not.Null);
		Assert.That(store.Find("Test"), Is.Null);
	}

	[Test]
	public async Task Edit_ReplacesActionsWithPowerShell()
	{
		SeedCommand();

		var (exit, stdout, _) = await InvokeAsync(
			"edit", "Test", "--powershell", "test.ps1");

		Assert.That(exit, Is.Zero);
		Assert.That(stdout.TrimEnd(), Is.EqualTo("Updated 'Test'"));

		var store = new CommandStore(TempDir);
		var result = store.Find("Test");
		Assert.That(result, Is.Not.Null);
		Assert.That(result!.Value.Command.Actions, Has.Length.EqualTo(1));
		Assert.That(result.Value.Command.Actions[0].Type, Is.EqualTo(ArgumentType.PowerShell));
		Assert.That(result.Value.Command.Actions[0].Value, Is.EqualTo("test.ps1"));
	}

	[Test]
	public async Task Edit_ArgWithoutContext_ReturnsError()
	{
		SeedCommand();

		var (exit, _, stderr) = await InvokeAsync(
			"edit", "Test", "--arg", "someguid", "value");

		Assert.That(exit, Is.EqualTo(1));
		Assert.That(stderr.TrimEnd(), Is.EqualTo("Unknown plugin: someguid"));
	}

	[Test]
	public async Task Edit_ReplacesWithMultiplePlugins()
	{
		SeedCommand();

		var startProgramGuid = PluginLoader.ResolvePlugin("StartProgram")!;
		var keyboardInputGuid = PluginLoader.ResolvePlugin("KeyboardInput")!;

		var (exit1, _, _) = await InvokeAsync(
			"edit", "Test",
			"--plugin", "StartProgram",
			"--plugin", "KeyboardInput",
			"--arg", "StartProgram", "https://new.example.com");
		Assert.That(exit1, Is.Zero);

		var (exit2, stdout, _) = await InvokeAsync(
			"edit", "Test",
			"--arg", "KeyboardInput", "typed text");

		Assert.That(exit2, Is.Zero);
		Assert.That(stdout.TrimEnd(), Is.EqualTo("Updated 'Test'"));

		var store = new CommandStore(TempDir);
		var result = store.Find("Test");
		Assert.That(result, Is.Not.Null);
		Assert.That(result!.Value.Command.Actions, Has.Length.EqualTo(2));
		Assert.That(result.Value.Command.Actions[0].Value, Is.EqualTo(startProgramGuid));
		Assert.That(result.Value.Command.Actions[1].Value, Is.EqualTo(keyboardInputGuid));
	}

	[Test]
	public async Task Edit_MergesPluginArgs()
	{
		SeedCommand();

		var startProgramGuid = PluginLoader.ResolvePlugin("StartProgram")!;
		var keyboardInputGuid = PluginLoader.ResolvePlugin("KeyboardInput")!;

		var (exit, stdout, _) = await InvokeAsync(
			"edit", "Test",
			"--arg", "KeyboardInput", "hello world");

		Assert.That(exit, Is.Zero);
		Assert.That(stdout.TrimEnd(), Is.EqualTo("Updated 'Test'"));

		var store = new CommandStore(TempDir);
		var result = store.Find("Test");
		Assert.That(result, Is.Not.Null);
		Assert.That(result!.Value.Command.PluginArguments[startProgramGuid][0].Tokens[0].Value,
			Is.EqualTo("https://example.com"));
		Assert.That(result.Value.Command.PluginArguments[keyboardInputGuid], Has.Length.EqualTo(1));
		Assert.That(result.Value.Command.PluginArguments[keyboardInputGuid][0].Tokens[0].Value,
			Is.EqualTo("hello world"));
	}
}
