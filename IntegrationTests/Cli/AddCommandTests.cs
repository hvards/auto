using Auto.Cli.Services;
using Auto.PluginUtils;

namespace IntegrationTests.Cli;

[TestFixture]
internal class AddCommandTests : CliTestBase
{
	[Test]
	public async Task Add_CreatesCommand()
	{
		// Act
		var (exit, stdout, _) = await InvokeAsync(
			"add", "Test", "--file", "test.json",
			"--combination", "LCtrl", "LWin", "T");

		// Assert
		Assert.That(exit, Is.Zero);
		Assert.That(stdout, Does.Match(@"^Added 'Test' to test\.json \(id: [0-9a-f\-]+\)\r?\n$"));

		var all = TestCommandStore.LoadAll();
		Assert.That(all, Has.Count.EqualTo(1));
		Assert.That(all[0].Command.Name, Is.EqualTo("Test"));
		Assert.That(all[0].Command.Actions, Is.Empty);
	}

	[Test]
	public async Task Add_NoFileOption_UsesDefaultJson()
	{
		// Act
		var (exit, stdout, _) = await InvokeAsync(
			"add", "Test", "--combination", "LCtrl", "T");

		// Assert
		Assert.That(exit, Is.Zero);
		Assert.That(stdout, Does.Contain("default.json"));

		var commands = CommandStore.LoadFile(TestCommandStore.ResolvePath("default.json"));
		Assert.That(commands, Has.Count.EqualTo(1));
		Assert.That(commands[0].Name, Is.EqualTo("Test"));
	}

	[Test]
	public async Task Add_ThenActionAdd_CreatesCommandWithAction()
	{
		// Arrange
		var startProgramGuid = TestPluginLoader.ResolvePlugin("StartProgram");

		// Act
		await InvokeAsync(
			"add", "Test", "--file", "test.json",
			"--combination", "LCtrl", "LWin", "T");

		var (exit, _, _) = await InvokeAsync(
			"action", "add", "Test",
			"StartProgram",
			"--arg", "https://example.com");

		// Assert
		Assert.That(exit, Is.Zero);

		var commands = CommandStore.LoadFile(TestCommandStore.ResolvePath("test.json"));
		Assert.That(commands[0].Actions, Has.Length.EqualTo(1));
		Assert.That(commands[0].Actions[0].Target, Is.EqualTo(startProgramGuid));
		Assert.That(commands[0].Actions[0].Arguments[0].Tokens[0].Value,
			Is.EqualTo("https://example.com"));
	}

	[Test]
	public async Task Add_WithDescription()
	{
		// Act
		var (exit, _, _) = await InvokeAsync(
			"add", "Test", "--file", "test.json",
			"--combination", "LCtrl", "T",
			"--description", "A test command");

		// Assert
		Assert.That(exit, Is.Zero);

		var commands = CommandStore.LoadFile(TestCommandStore.ResolvePath("test.json"));
		Assert.That(commands[0].Description, Is.EqualTo("A test command"));
	}

	[Test]
	public async Task Add_Disabled()
	{
		// Act
		var (exit, _, _) = await InvokeAsync(
			"add", "Test", "--file", "test.json",
			"--combination", "LCtrl", "T",
			"--disabled");

		// Assert
		Assert.That(exit, Is.Zero);

		var commands = CommandStore.LoadFile(TestCommandStore.ResolvePath("test.json"));
		Assert.That(commands[0].Enabled, Is.False);
	}
}