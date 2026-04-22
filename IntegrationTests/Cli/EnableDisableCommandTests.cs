namespace IntegrationTests.Cli;

[TestFixture]
internal class EnableDisableCommandTests : CliTestBase
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
		Assert.That(TestCommandStore.Find("Test")!.Value.Command.Enabled, Is.True);
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
		Assert.That(TestCommandStore.Find("Test")!.Value.Command.Enabled, Is.False);
	}
}
