using System.Text.Json;

namespace IntegrationTests.Cli;

[TestFixture]
internal class ListCommandTests : CliTestBase
{
	[Test]
	public async Task List_ShowsCommands()
	{
		// Arrange
		var cmd = SeedCommand();

		// Act
		var (exit, stdout, _) = await InvokeAsync("list");

		// Assert
		Assert.That(exit, Is.Zero);
		var expected =
			$"  [on ]  {cmd.Id}  {"Test",-40} test.json" + Environment.NewLine +
			Environment.NewLine +
			"1 command(s)" + Environment.NewLine;
		Assert.That(stdout, Is.EqualTo(expected));
	}

	[Test]
	public async Task List_Json_Format()
	{
		// Arrange
		SeedCommand();

		// Act
		var (exit, stdout, _) = await InvokeAsync("list", "--json");

		// Assert
		Assert.That(exit, Is.Zero);
		var doc = JsonDocument.Parse(stdout);
		Assert.That(doc.RootElement.GetArrayLength(), Is.EqualTo(1));
		Assert.That(doc.RootElement[0].GetProperty("Name").GetString(), Is.EqualTo("Test"));
	}
}