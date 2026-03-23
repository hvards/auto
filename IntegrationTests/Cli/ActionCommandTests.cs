using Auto.Cli.Services;
using Auto.Models;
using Auto.PluginUtils;

namespace IntegrationTests.Cli;

[TestFixture]
internal class ActionCommandTests : CliTestBase
{
	[Test]
	public async Task ActionAdd_AppendsAction()
	{
		// Arrange
		await InvokeAsync("add", "Test", "--combination", "LCtrl", "T");

		// Act
		var (exit, stdout, _) = await InvokeAsync(
			"action", "add", "Test",
			"--plugin", "StartProgram",
			"--arg", "https://example.com");

		// Assert
		Assert.That(exit, Is.Zero);
		Assert.That(stdout, Does.Contain("Added action"));

		var cmd = TestCommandStore.GetCommand("Test").Command;
		Assert.That(cmd.Actions, Has.Length.EqualTo(1));
		Assert.That(cmd.Actions[0].Target, Is.EqualTo(PluginLoader.ResolvePlugin("StartProgram")));
		Assert.That(cmd.Actions[0].Arguments[0].Tokens[0].Value, Is.EqualTo("https://example.com"));
	}

	[Test]
	public async Task ActionAdd_WithVariable()
	{
		// Arrange
		await InvokeAsync("add", "Test", "--combination", "LCtrl", "T");

		// Act
		var (exit, _, _) = await InvokeAsync(
			"action", "add", "Test",
			"--plugin", "StartProgram",
			"--arg", "https://example.com",
			"--var", "TestCommandResult");

		// Assert
		Assert.That(exit, Is.Zero);

		var cmd = TestCommandStore.GetCommand("Test").Command;
		Assert.That(cmd.Actions[0].Variable, Is.EqualTo("TestCommandResult"));
	}

	[Test]
	public async Task ActionAdd_VariableReference()
	{
		// Arrange
		await InvokeAsync("add", "Test", "--combination", "LCtrl", "T");
		await InvokeAsync("action", "add", "Test",
			"--plugin", "StartProgram", "--arg", "https://example.com", "--var", "TestCommandResult");

		// Act
		var (exit, _, _) = await InvokeAsync(
			"action", "add", "Test",
			"--plugin", "KeyboardInput",
			"--arg", "%{TestCommandResult}");

		// Assert
		Assert.That(exit, Is.Zero);

		var cmd = TestCommandStore.GetCommand("Test").Command;
		Assert.That(cmd.Actions, Has.Length.EqualTo(2));
		Assert.That(cmd.Actions[1].Arguments[0].Tokens[0].Type, Is.EqualTo(ArgumentType.Variable));
		Assert.That(cmd.Actions[1].Arguments[0].Tokens[0].Value, Is.EqualTo("TestCommandResult"));
	}

	[Test]
	public async Task ActionAdd_ForwardReference_Fails()
	{
		// Arrange
		await InvokeAsync("add", "Test", "--combination", "LCtrl", "T");

		// Act
		var (exit, _, stderr) = await InvokeAsync(
			"action", "add", "Test",
			"--plugin", "StartProgram",
			"--arg", "%{NonExistent}");

		// Assert
		Assert.That(exit, Is.EqualTo(1));
		Assert.That(stderr, Does.Contain("NonExistent"));
	}

	[Test]
	public async Task ActionAdd_PowerShellWithPsArg()
	{
		// Arrange
		await InvokeAsync("add", "Test", "--combination", "LCtrl", "T");

		// Act
		var (exit, _, _) = await InvokeAsync(
			"action", "add", "Test",
			"--powershell", "test.ps1",
			"--arg", @"Path=C:\scripts");

		// Assert
		Assert.That(exit, Is.Zero);

		var cmd = TestCommandStore.GetCommand("Test").Command;
		Assert.That(cmd.Actions[0].Type, Is.EqualTo(ActionType.PowerShell));
		Assert.That(cmd.Actions[0].Target, Is.EqualTo("test.ps1"));
		Assert.That(cmd.Actions[0].Arguments[0].ParameterName, Is.EqualTo("Path"));
		Assert.That(cmd.Actions[0].Arguments[0].Tokens[0].Value, Is.EqualTo(@"C:\scripts"));
	}

	[Test]
	public async Task ActionAdd_PowerShellMultiplePsArgs()
	{
		// Arrange
		await InvokeAsync("add", "Test", "--combination", "LCtrl", "T");

		// Act
		var (exit, _, _) = await InvokeAsync(
			"action", "add", "Test",
			"--powershell", "test.ps1",
			"--arg", @"Path=C:\scripts", "Name=foo");

		// Assert
		Assert.That(exit, Is.Zero);

		var cmd = TestCommandStore.GetCommand("Test").Command;
		Assert.That(cmd.Actions[0].Arguments, Has.Length.EqualTo(2));
		Assert.That(cmd.Actions[0].Arguments[0].ParameterName, Is.EqualTo("Path"));
		Assert.That(cmd.Actions[0].Arguments[1].ParameterName, Is.EqualTo("Name"));
		Assert.That(cmd.Actions[0].Arguments[1].Tokens[0].Value, Is.EqualTo("foo"));
	}

	[Test]
	public async Task ActionAdd_RequiresPluginOrPowershell()
	{
		// Arrange
		await InvokeAsync("add", "Test", "--combination", "LCtrl", "T");

		// Act
		var (exit, _, stderr) = await InvokeAsync("action", "add", "Test");

		// Assert
		Assert.That(exit, Is.EqualTo(1));
		Assert.That(stderr, Does.Contain("--plugin").And.Contains("--powershell"));
	}

	[Test]
	public async Task ActionAdd_PluginAndPowershell_MutuallyExclusive()
	{
		// Arrange
		await InvokeAsync("add", "Test", "--combination", "LCtrl", "T");

		// Act
		var (exit, _, stderr) = await InvokeAsync(
			"action", "add", "Test",
			"--plugin", "StartProgram",
			"--powershell", "test.ps1");

		// Assert
		Assert.That(exit, Is.EqualTo(1));
		Assert.That(stderr, Does.Contain("mutually exclusive"));
	}

	[Test]
	public async Task ActionDelete_DanglingReference_Fails()
	{
		// Arrange
		await InvokeAsync("add", "Test", "--combination", "LCtrl", "T");
		await InvokeAsync("action", "add", "Test",
			"--plugin", "StartProgram", "--arg", "https://example.com", "--var", "Result");
		await InvokeAsync("action", "add", "Test",
			"--plugin", "KeyboardInput", "--arg", "%{Result}");

		// Act
		var (exit, _, stderr) = await InvokeAsync("action", "delete", "Test", "0");

		// Assert
		Assert.That(exit, Is.EqualTo(1));
		Assert.That(stderr, Does.Contain("Result").And.Contain("is not available"));
	}

	[Test]
	public async Task ActionDelete_ByIndex()
	{
		// Arrange
		await InvokeAsync("add", "Test", "--combination", "LCtrl", "T");
		await InvokeAsync("action", "add", "Test", "--plugin", "StartProgram", "--arg", "a");
		await InvokeAsync("action", "add", "Test", "--plugin", "StartProgram", "--arg", "b");

		// Act — delete the first action (index 0)
		var (exit, _, _) = await InvokeAsync("action", "delete", "Test", "0");

		// Assert
		Assert.That(exit, Is.Zero);

		var cmd = TestCommandStore.GetCommand("Test").Command;
		Assert.That(cmd.Actions, Has.Length.EqualTo(1));
		Assert.That(cmd.Actions[0].Arguments[0].Tokens[0].Value, Is.EqualTo("b"));
	}

	[Test]
	public async Task ActionDelete_IndexOutOfRange()
	{
		// Arrange
		await InvokeAsync("add", "Test", "--combination", "LCtrl", "T");
		await InvokeAsync("action", "add", "Test", "--plugin", "StartProgram", "--arg", "a");

		// Act
		var (exit, _, stderr) = await InvokeAsync("action", "delete", "Test", "5");

		// Assert
		Assert.That(exit, Is.EqualTo(1));
		Assert.That(stderr, Does.Contain("out of range"));
	}

	[Test]
	public async Task ActionEdit_ChangeArgs()
	{
		// Arrange
		await InvokeAsync("add", "Test", "--combination", "LCtrl", "T");
		await InvokeAsync("action", "add", "Test", "--plugin", "StartProgram", "--arg", "a");

		// Act
		var (exit, stdout, _) = await InvokeAsync("action", "edit", "Test", "0", "--arg", "b");

		// Assert
		Assert.That(exit, Is.Zero);
		Assert.That(stdout, Does.Contain("Updated action"));

		var cmd = TestCommandStore.GetCommand("Test").Command;
		Assert.That(cmd.Actions[0].Arguments[0].Tokens[0].Value, Is.EqualTo("b"));
	}

	[Test]
	public async Task ActionEdit_SetVariable()
	{
		// Arrange
		await InvokeAsync("add", "Test", "--combination", "LCtrl", "T");
		await InvokeAsync("action", "add", "Test", "--plugin", "StartProgram", "--arg", "a");

		// Act
		var (exit, _, _) = await InvokeAsync("action", "edit", "Test", "0", "--var", "Result");

		// Assert
		Assert.That(exit, Is.Zero);

		var cmd = TestCommandStore.GetCommand("Test").Command;
		Assert.That(cmd.Actions[0].Variable, Is.EqualTo("Result"));
	}

	[Test]
	public async Task ActionEdit_ClearVariable()
	{
		// Arrange
		await InvokeAsync("add", "Test", "--combination", "LCtrl", "T");
		await InvokeAsync("action", "add", "Test", "--plugin", "StartProgram", "--arg", "a", "--var", "Result");

		// Act
		var (exit, _, _) = await InvokeAsync("action", "edit", "Test", "0", "--var", "");

		// Assert
		Assert.That(exit, Is.Zero);

		var cmd = TestCommandStore.GetCommand("Test").Command;
		Assert.That(cmd.Actions[0].Variable, Is.Null);
	}

	[Test]
	public async Task ActionEdit_ClearVar_DanglingReference_Fails()
	{
		// Arrange
		await InvokeAsync("add", "Test", "--combination", "LCtrl", "T");
		await InvokeAsync("action", "add", "Test",
			"--plugin", "StartProgram", "--arg", "a", "--var", "Result");
		await InvokeAsync("action", "add", "Test",
			"--plugin", "KeyboardInput", "--arg", "%{Result}");

		// Act
		var (exit, _, stderr) = await InvokeAsync("action", "edit", "Test", "0", "--var", "");

		// Assert
		Assert.That(exit, Is.EqualTo(1));
		Assert.That(stderr, Does.Contain("Result").And.Contain("is not available"));
	}

	[Test]
	public async Task ActionEdit_IndexOutOfRange()
	{
		// Arrange
		await InvokeAsync("add", "Test", "--combination", "LCtrl", "T");
		await InvokeAsync("action", "add", "Test", "--plugin", "StartProgram", "--arg", "a");

		// Act
		var (exit, _, stderr) = await InvokeAsync("action", "edit", "Test", "5", "--arg", "b");

		// Assert
		Assert.That(exit, Is.EqualTo(1));
		Assert.That(stderr, Does.Contain("out of range"));
	}

	[Test]
	public async Task ActionEdit_RequiresChange()
	{
		// Arrange
		await InvokeAsync("add", "Test", "--combination", "LCtrl", "T");
		await InvokeAsync("action", "add", "Test", "--plugin", "StartProgram", "--arg", "a");

		// Act
		var (exit, _, stderr) = await InvokeAsync("action", "edit", "Test", "0");

		// Assert
		Assert.That(exit, Is.EqualTo(1));
		Assert.That(stderr, Does.Contain("--arg").Or.Contains("--var"));
	}

	[Test]
	public async Task FullPipeline_MultipleActions()
	{
		// Arrange
		var startProgramGuid = PluginLoader.ResolvePlugin("StartProgram");
		var keyboardInputGuid = PluginLoader.ResolvePlugin("KeyboardInput");

		// Act
		await InvokeAsync("add", "Format and Paste", "--file", "cmds.json",
			"--combination", "LCtrl", "LAlt", "V");
		await InvokeAsync("action", "add", "Format and Paste",
			"--plugin", "StartProgram", "--arg", "https://example.com", "--var", "FormattedText");
		var (exit, _, stderr) = await InvokeAsync("action", "add", "Format and Paste",
			"--plugin", "KeyboardInput", "--arg", "%{FormattedText}");

		// Assert
		Assert.That(exit, Is.Zero, () => stderr);

		var commands = CommandStore.LoadFile(TestCommandStore.ResolvePath("cmds.json"));
		Assert.That(commands[0].Actions, Has.Length.EqualTo(2));
		Assert.That(commands[0].Actions[0].Target, Is.EqualTo(startProgramGuid));
		Assert.That(commands[0].Actions[0].Variable, Is.EqualTo("FormattedText"));
		Assert.That(commands[0].Actions[1].Target, Is.EqualTo(keyboardInputGuid));
		Assert.That(commands[0].Actions[1].Arguments[0].Tokens[0].Type, Is.EqualTo(ArgumentType.Variable));
		Assert.That(commands[0].Actions[1].Arguments[0].Tokens[0].Value, Is.EqualTo("FormattedText"));

		// Verify order
		Assert.That(commands[0].Actions[0].Order, Is.LessThan(commands[0].Actions[1].Order));
	}
}