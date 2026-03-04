using Auto.Cli.Services;
using Auto.Models;

namespace IntegrationTests.Cli;

[TestFixture]
public class EditCommandTests : CliTestBase
{
	[Test]
	public async Task Edit_UpdatesTrigger()
	{
		// Arrange
		SeedCommand();

		// Act
		var (exit, stdout, _) = await InvokeAsync("edit", "Test", "--combination", "LCtrl+LWin+R");

		// Assert
		Assert.That(exit, Is.Zero);
		Assert.That(stdout.TrimEnd(), Is.EqualTo("Updated 'Test'"));

		var store = new CommandStore(TempDir);
		var result = store.Find("Test");
		Assert.That(result, Is.Not.Null);
		var expected = KeyNameResolver.FormatCombination(KeyNameResolver.ParseCombination("LCtrl+LWin+R"));
		Assert.That(KeyNameResolver.FormatCombination(result!.Value.Command.Trigger.Combination),
			Is.EqualTo(expected));
	}

	[Test]
	public async Task Edit_RenamesCommand()
	{
		// Arrange
		SeedCommand();

		// Act
		var (exit, stdout, _) = await InvokeAsync("edit", "Test", "--name", "Renamed");

		// Assert
		Assert.That(exit, Is.Zero);
		Assert.That(stdout.TrimEnd(), Is.EqualTo("Updated 'Renamed'"));

		var store = new CommandStore(TempDir);
		Assert.That(store.Find("Renamed"), Is.Not.Null);
		Assert.That(store.Find("Test"), Is.Null);
	}

	[Test]
	public async Task Edit_ReplacesActionsWithActionFlag()
	{
		// Arrange
		SeedCommand();
		var psScript = "test.ps1";

		// Act
		var (exit, stdout, _) = await InvokeAsync(
			"edit", "Test", "--action", $"ps:{psScript}");

		// Assert
		Assert.That(exit, Is.Zero);
		Assert.That(stdout.TrimEnd(), Is.EqualTo("Updated 'Test'"));

		var store = new CommandStore(TempDir);
		var result = store.Find("Test");
		Assert.That(result, Is.Not.Null);
		Assert.That(result!.Value.Command.Actions, Has.Length.EqualTo(1));
		Assert.That(result.Value.Command.Actions[0].Type, Is.EqualTo(ArgumentType.PowerShell));
		Assert.That(result.Value.Command.Actions[0].Value, Is.EqualTo(psScript));
	}

	[Test]
	public async Task Edit_PluginArgWithoutContext_ReturnsError()
	{
		// Arrange
		SeedCommand();

		// Act
		var (exit, _, stderr) = await InvokeAsync(
			"edit", "Test", "--plugin-arg", "someguid:value");

		// Assert
		Assert.That(exit, Is.EqualTo(1));
		Assert.That(stderr.TrimEnd(), Is.EqualTo("--plugin-arg and --ps-arg require --plugin, --powershell, or --action"));
	}
}