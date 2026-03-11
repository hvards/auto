using Auto.Cli.Serialization;
using Auto.Cli.Services;
using Auto.Models;
using Auto.PluginUtils;
using System.Reflection;

namespace IntegrationTests.Cli;

[TestFixture]
public class AddCommandTests : CliTestBase
{
	private static string LoadResource(string name)
	{
		using var stream = Assembly.GetExecutingAssembly()
			.GetManifestResourceStream($"IntegrationTests.Cli.TestData.{name}")!;
		using var reader = new StreamReader(stream);
		return reader.ReadToEnd();
	}

	[Test]
	public async Task Add_CreatesCommand()
	{
		var (exit, stdout, _) = await InvokeAsync(
			"add", "Test", "--file", "test.json",
			"--combination", "LCtrl", "LWin", "T",
			"--plugin", "StartProgram",
			"--arg", "StartProgram", "https://example.com");

		Assert.That(exit, Is.Zero);
		Assert.That(stdout, Does.Match(@"^Added 'Test' to test\.json \(id: [0-9a-f\-]+\)\r?\n$"));

		var store = new CommandStore(TempDir);
		var all = store.LoadAll();
		Assert.That(all, Has.Count.EqualTo(1));
		Assert.That(all[0].Command.Name, Is.EqualTo("Test"));
	}

	[Test]
	public async Task Add_NoFileOption_UsesDefaultJson()
	{
		var (exit, stdout, _) = await InvokeAsync(
			"add", "Test", "--combination", "LCtrl", "T",
			"--plugin", "StartProgram",
			"--arg", "StartProgram", "https://example.com");

		Assert.That(exit, Is.Zero);
		Assert.That(stdout, Does.Contain("default.json"));

		var store = new CommandStore(TempDir);
		var commands = CommandStore.LoadFile(store.ResolvePath("default.json"));
		Assert.That(commands, Has.Count.EqualTo(1));
		Assert.That(commands[0].Name, Is.EqualTo("Test"));
	}

	[Test]
	public async Task Add_PluginWithNestedPluginArgs()
	{
		const string pluginA = "3a40e921-8007-4f18-8f78-4ff8ba10abfe";
		const string pluginB = "854b6621-9ba9-4eae-bafd-89613cac9c5b";
		const string pluginC = "2a85d89b-234f-483a-99fd-0bc1b98f34ac";
		const string cmdName = "Set default playback device from list";

		var (exit1, _, _) = await InvokeAsync(
			"add", cmdName, "--file", "cmds.json",
			"--combination", "LCtrl", "LAlt", "LShift", "RCtrl", "RAlt", "RShift", "49",
			"--plugin", pluginA,
			"--arg", pluginA, "0", $"%{{plugin:{pluginB}}}");
		Assert.That(exit1, Is.Zero);

		var (exit2, _, _) = await InvokeAsync(
			"edit", cmdName,
			"--arg", pluginB, "Devices", $"%{{plugin:{pluginC}}}");
		Assert.That(exit2, Is.Zero);

		var (exit3, _, _) = await InvokeAsync(
			"edit", cmdName,
			"--arg", pluginC, "0");
		Assert.That(exit3, Is.Zero);

		var store = new CommandStore(TempDir);
		var commands = CommandStore.LoadFile(store.ResolvePath("cmds.json"));
		commands[0].Id = Guid.Parse("e4d0f13a-ce81-4438-b7c0-6bee2e121493");

		var actual = CommandSerializer.Serialize(commands);
		var expected = LoadResource("NestedPluginArgs.json");
		Assert.That(actual, Is.EqualTo(expected));
	}

	[Test]
	public async Task Add_MultiplePlugins()
	{
		var startProgramGuid = PluginLoader.ResolvePlugin("StartProgram")!;
		var keyboardInputGuid = PluginLoader.ResolvePlugin("KeyboardInput")!;

		var (exit, _, _) = await InvokeAsync(
			"add", "Multi", "--file", "multi.json",
			"--combination", "LCtrl", "LWin", "M",
			"--plugin", "StartProgram",
			"--plugin", "KeyboardInput",
			"--arg", "StartProgram", "https://example.com");

		Assert.That(exit, Is.Zero);

		var store = new CommandStore(TempDir);
		var commands = CommandStore.LoadFile(store.ResolvePath("multi.json"));
		Assert.That(commands[0].Actions, Has.Length.EqualTo(2));
		Assert.That(commands[0].Actions[0].Value, Is.EqualTo(startProgramGuid));
		Assert.That(commands[0].Actions[1].Value, Is.EqualTo(keyboardInputGuid));
		Assert.That(commands[0].PluginArguments[startProgramGuid][0].Tokens[0].Value,
			Is.EqualTo("https://example.com"));
		Assert.That(commands[0].PluginArguments.ContainsKey(keyboardInputGuid), Is.False);
	}

	[Test]
	public async Task Add_PowerShellWithPsArg()
	{
		var (exit, _, _) = await InvokeAsync(
			"add", "PSTest", "--file", "ps.json",
			"--combination", "LCtrl", "LWin", "P",
			"--powershell", "test.ps1",
			"--ps-arg", "test.ps1", "Path", "C:\\scripts");

		Assert.That(exit, Is.Zero);

		var store = new CommandStore(TempDir);
		var commands = CommandStore.LoadFile(store.ResolvePath("ps.json"));
		Assert.That(commands[0].Actions, Has.Length.EqualTo(1));
		Assert.That(commands[0].Actions[0].Type, Is.EqualTo(ArgumentType.PowerShell));
		Assert.That(commands[0].Actions[0].Value, Is.EqualTo("test.ps1"));
		Assert.That(commands[0].PowerShellArguments["test.ps1"][0].ParameterName, Is.EqualTo("Path"));
		Assert.That(commands[0].PowerShellArguments["test.ps1"][0].Tokens[0].Value, Is.EqualTo("C:\\scripts"));
	}

	[Test]
	public async Task Add_PowerShellWithMultiplePsArgs()
	{
		var (exit, _, _) = await InvokeAsync(
			"add", "PSTest2", "--file", "ps2.json",
			"--combination", "LCtrl", "LWin", "Q",
			"--powershell", "test.ps1",
			"--ps-arg", "test.ps1", "Path", "C:\\scripts", "Name", "foo");

		Assert.That(exit, Is.Zero);

		var store = new CommandStore(TempDir);
		var commands = CommandStore.LoadFile(store.ResolvePath("ps2.json"));
		Assert.That(commands[0].PowerShellArguments["test.ps1"], Has.Length.EqualTo(2));
		Assert.That(commands[0].PowerShellArguments["test.ps1"][0].ParameterName, Is.EqualTo("Path"));
		Assert.That(commands[0].PowerShellArguments["test.ps1"][0].Tokens[0].Value, Is.EqualTo("C:\\scripts"));
		Assert.That(commands[0].PowerShellArguments["test.ps1"][1].ParameterName, Is.EqualTo("Name"));
		Assert.That(commands[0].PowerShellArguments["test.ps1"][1].Tokens[0].Value, Is.EqualTo("foo"));
	}
}
