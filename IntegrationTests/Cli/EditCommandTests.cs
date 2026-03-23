using Auto.Cli.Services;

namespace IntegrationTests.Cli;

[TestFixture]
internal class EditCommandTests : CliTestBase
{
	[Test]
	public async Task Edit_UpdatesTrigger()
	{
		// Arrange
		SeedCommand();

		// Act
		var (exit, stdout, _) = await InvokeAsync("edit", "Test", "--combination", "LCtrl", "LWin", "R");

		// Assert
		Assert.That(exit, Is.Zero);
		Assert.That(stdout.TrimEnd(), Is.EqualTo("Updated 'Test'"));

		var result = TestCommandStore.Find("Test");
		Assert.That(result, Is.Not.Null);
		var expected = KeyNameResolver.FormatCombination(KeyNameResolver.ParseCombination(["LCtrl", "LWin", "R"]));
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
		Assert.That(TestCommandStore.Find("Renamed"), Is.Not.Null);
		Assert.That(TestCommandStore.Find("Test"), Is.Null);
	}

	[Test]
	public async Task Edit_UpdatesDescription()
	{
		// Arrange
		SeedCommand();

		// Act
		var (exit, _, _) = await InvokeAsync("edit", "Test", "--description", "New desc");

		// Assert
		Assert.That(exit, Is.Zero);
		Assert.That(TestCommandStore.Find("Test")!.Value.Command.Description, Is.EqualTo("New desc"));
	}
}