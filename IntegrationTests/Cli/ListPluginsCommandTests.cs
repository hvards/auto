using System.Text.Json;

namespace IntegrationTests.Cli;

[TestFixture]
internal class ListPluginsCommandTests : CliTestBase
{
	[Test]
	public async Task ListPlugins_ShowsBuiltInPlugins()
	{
		// Act
		var (exit, stdout, _) = await InvokeAsync("list-plugins");

		// Assert
		Assert.That(exit, Is.Zero);
		Assert.That(stdout, Does.Contain("KeyboardInput"));
		Assert.That(stdout, Does.Contain("StartProgram"));
	}

	[Test]
	public async Task ListPlugins_JsonFormat()
	{
		// Act
		var (exit, stdout, _) = await InvokeAsync("list-plugins", "--json");

		// Assert
		Assert.That(exit, Is.Zero);
		var doc = JsonDocument.Parse(stdout);
		Assert.That(doc.RootElement.ValueKind, Is.EqualTo(JsonValueKind.Array));
		Assert.That(doc.RootElement.GetArrayLength(), Is.GreaterThanOrEqualTo(2));

		var names = doc.RootElement.EnumerateArray()
			.Select(e => e.GetProperty("Name").GetString())
			.ToList();
		Assert.That(names, Does.Contain("KeyboardInput"));
		Assert.That(names, Does.Contain("StartProgram"));
	}
}