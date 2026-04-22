using System.Text.Json;

using Auto.Cli.Services;
using Auto.PluginUtils;

namespace IntegrationTests.Cli;

[TestFixture]
internal class GetCommandTests : CliTestBase
{
	[Test]
	public async Task Get_ShowsDetail()
	{
		// Arrange
		var cmd = SeedCommand();

		// Act
		var (exit, stdout, _) = await InvokeAsync("get", "Test");

		// Assert
		Assert.That(exit, Is.Zero);
		var trigger = KeyNameResolver.FormatCombination(cmd.Trigger.Combination);
		var startProgramGuid = TestPluginLoader.ResolvePlugin("StartProgram");
		var expected = string.Join(Environment.NewLine,
			$"Name:        Test",
			$"Description: Test description",
			$"Id:          {cmd.Id}",
			$"Enabled:     True",
			$"File:        test.json",
			$"Trigger:",
			$"  Combination:     {trigger}",
			$"Actions:",
			$"  [0] StartProgram ({startProgramGuid})",
			$"    Args:",
			$"      https://example.com",
			"");
		Assert.That(stdout, Is.EqualTo(expected));
	}

	[Test]
	public async Task Get_Json_Format()
	{
		// Arrange
		SeedCommand();

		// Act
		var (exit, stdout, _) = await InvokeAsync("get", "Test", "--json");

		// Assert
		Assert.That(exit, Is.Zero);
		var doc = JsonDocument.Parse(stdout);
		Assert.That(doc.RootElement.ValueKind, Is.EqualTo(JsonValueKind.Object));
	}

	[Test]
	public async Task Get_NotFound_ReturnsError()
	{
		// Act
		var (exit, _, stderr) = await InvokeAsync("get", "Nonexistent");

		// Assert
		Assert.That(exit, Is.EqualTo(1));
		Assert.That(stderr.TrimEnd(), Is.EqualTo("Command not found: Nonexistent"));
	}
}
