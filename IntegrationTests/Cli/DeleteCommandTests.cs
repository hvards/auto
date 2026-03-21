namespace IntegrationTests.Cli;

[TestFixture]
public class DeleteCommandTests : CliTestBase
{
	[Test]
	public async Task Delete_RemovesCommand()
	{
		// Arrange
		SeedCommand();

		// Act
		var (exit, stdout, _) = await InvokeAsync("delete", "Test");

		// Assert
		Assert.That(exit, Is.Zero);
		Assert.That(stdout.TrimEnd(), Is.EqualTo("Deleted 'Test'"));
		Assert.That(TestCommandStore.Find("Test"), Is.Null);
	}

	[Test]
	public async Task Delete_NotFound_ReturnsError()
	{
		// Act
		var (exit, _, stderr) = await InvokeAsync("delete", "Nonexistent");

		// Assert
		Assert.That(exit, Is.EqualTo(1));
		Assert.That(stderr.TrimEnd(), Is.EqualTo("Command not found: Nonexistent"));
	}
}