using Auto.Cli.Services;

namespace IntegrationTests.Cli;

[TestFixture]
public class EnableDisableCommandTests : CliTestBase
{
	[Test]
	public async Task Enable_TogglesOn()
	{
		// Arrange
		SeedCommand(enabled: false);

		// Act
		var (exit, stdout, _) = await InvokeAsync("enable", "Test");

		// Assert
		Assert.That(exit, Is.Zero);
		Assert.That(stdout.TrimEnd(), Is.EqualTo("Enabled 'Test'"));

		var store = new CommandStore(TempDir);
		var result = store.Find("Test");
		Assert.That(result!.Value.Command.Enabled, Is.True);
	}

	[Test]
	public async Task Disable_TogglesOff()
	{
		// Arrange
		SeedCommand(enabled: true);

		// Act
		var (exit, stdout, _) = await InvokeAsync("disable", "Test");

		// Assert
		Assert.That(exit, Is.Zero);
		Assert.That(stdout.TrimEnd(), Is.EqualTo("Disabled 'Test'"));

		var store = new CommandStore(TempDir);
		var result = store.Find("Test");
		Assert.That(result!.Value.Command.Enabled, Is.False);
	}
}