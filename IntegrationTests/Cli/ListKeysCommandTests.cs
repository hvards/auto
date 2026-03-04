namespace IntegrationTests.Cli;

[TestFixture]
public class ListKeysCommandTests : CliTestBase
{
	[Test]
	public async Task ListKeys_ReturnsZeroAndShowsAliases()
	{
		// Act
		var (exit, stdout, _) = await InvokeAsync("list-keys");

		// Assert
		Assert.That(exit, Is.Zero);
		Assert.That(stdout, Does.Contain("Aliases:"));
		Assert.That(stdout, Does.Contain("Ctrl"));
		Assert.That(stdout, Does.Contain("Alt"));
		Assert.That(stdout, Does.Contain("Shift"));
		Assert.That(stdout, Does.Contain("Win"));
	}

	[Test]
	public async Task ListKeys_ShowsEnumKeys()
	{
		// Act
		var (exit, stdout, _) = await InvokeAsync("list-keys");

		// Assert
		Assert.That(exit, Is.Zero);
		Assert.That(stdout, Does.Contain("Keys:"));
		Assert.That(stdout, Does.Contain("Space"));
		Assert.That(stdout, Does.Contain("F1"));
		Assert.That(stdout, Does.Contain("A"));
	}
}
