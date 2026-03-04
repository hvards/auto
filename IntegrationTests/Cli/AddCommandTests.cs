using Auto.Cli.Serialization;
using Auto.Cli.Services;
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
		// Act
		var (exit, stdout, _) = await InvokeAsync(
			"add", "Test", "--file", "test.json",
			"--combination", "LCtrl+LWin+T", "--plugin", "StartProgram", "--plugin-arg", "https://example.com");

		// Assert
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
		// Act
		var (exit, stdout, _) = await InvokeAsync(
			"add", "Test", "--combination", "LCtrl+T", "--plugin", "StartProgram", "--plugin-arg", "https://example.com");

		// Assert
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
		// Arrange
		const string pluginA = "3a40e921-8007-4f18-8f78-4ff8ba10abfe";
		const string pluginB = "854b6621-9ba9-4eae-bafd-89613cac9c5b";
		const string pluginC = "2a85d89b-234f-483a-99fd-0bc1b98f34ac";

		// Act
		var (exit, _, _) = await InvokeAsync(
			"add", "Set default playback device from list", "--file", "cmds.json",
			"--combination", "LCtrl+LAlt+LShift+RCtrl+RAlt+RShift+49",
			"--action", $"plugin:{pluginA}",
			"--plugin-arg", $"{pluginA}:0",
			"--plugin-arg", $"{pluginA}:%{{plugin:{pluginB}}}",
			"--plugin-arg", $"{pluginB}:Devices",
			"--plugin-arg", $"{pluginB}:%{{plugin:{pluginC}}}",
			"--plugin-arg", $"{pluginC}:0");

		// Assert
		Assert.That(exit, Is.Zero);

		var store = new CommandStore(TempDir);
		var commands = CommandStore.LoadFile(store.ResolvePath("cmds.json"));
		commands[0].Id = Guid.Parse("e4d0f13a-ce81-4438-b7c0-6bee2e121493");

		var actual = CommandSerializer.Serialize(commands);
		var expected = LoadResource("NestedPluginArgs.json");
		Assert.That(actual, Is.EqualTo(expected));
	}
}