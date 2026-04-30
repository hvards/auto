using System.Text.Json;

using Auto.Cli.Services;

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

	[Test]
	public async Task List_FiltersByCombinationKeys()
	{
		// Arrange
		var match = SeedCommand("Match", combination: ["LCtrl", "LWin", "B"]);
		SeedCommand("Other", combination: ["LCtrl", "T"]);

		// Act
		var (exit, stdout, _) = await InvokeAsync("list", "--combination", "LCtrl", "LWin", "B");

		// Assert
		Assert.That(exit, Is.Zero);
		Assert.That(stdout, Does.Contain(match.Id.ToString()));
		Assert.That(stdout, Does.Contain("1 command(s)"));
	}

	[Test]
	public async Task List_FiltersBySequenceKeys()
	{
		// Arrange
		var match = SeedCommand("Match", sequence: ["S", "T", "O", "P"]);
		SeedCommand("Other", sequence: ["A", "B", "C"]);
		SeedCommand("Combo");

		// Act
		var (exit, stdout, _) = await InvokeAsync("list", "--sequence", "S", "T", "O", "P");

		// Assert
		Assert.That(exit, Is.Zero);
		Assert.That(stdout, Does.Contain(match.Id.ToString()));
		Assert.That(stdout, Does.Contain("1 command(s)"));
	}

	[Test]
	public async Task List_CombinationFlag_NoArgs_RecordsInteractively()
	{
		// Arrange
		TestKeyRecorder.NextCombination = [.. KeyNameResolver.ParseInput(["LCtrl", "LWin", "B"])];
		var match = SeedCommand("Match", combination: ["LCtrl", "LWin", "B"]);
		SeedCommand("Other", combination: ["LCtrl", "T"]);

		// Act
		var (exit, stdout, _) = await InvokeAsync("list", "--combination");

		// Assert
		Assert.That(exit, Is.Zero);
		Assert.That(stdout, Does.Contain(match.Id.ToString()));
		Assert.That(stdout, Does.Contain("1 command(s)"));
	}

	[Test]
	public async Task List_SequenceFlag_NoArgs_RecordsInteractively()
	{
		// Arrange
		TestKeyRecorder.NextSequence = [.. KeyNameResolver.ParseInput(["S", "T", "O", "P"])];
		var match = SeedCommand("Match", sequence: ["S", "T", "O", "P"]);
		SeedCommand("Other", sequence: ["A", "B"]);

		// Act
		var (exit, stdout, _) = await InvokeAsync("list", "--sequence");

		// Assert
		Assert.That(exit, Is.Zero);
		Assert.That(stdout, Does.Contain(match.Id.ToString()));
		Assert.That(stdout, Does.Contain("1 command(s)"));
	}

	[Test]
	public async Task List_InvalidKey_ReturnsError()
	{
		// Arrange
		SeedCommand();

		// Act
		var (exit, _, stderr) = await InvokeAsync("list", "--combination", "NotAKey");

		// Assert
		Assert.That(exit, Is.EqualTo(1));
		Assert.That(stderr, Does.Contain("Unknown key name"));
	}
}
