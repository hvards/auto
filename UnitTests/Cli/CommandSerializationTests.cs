using System.Text.Json;

using Auto.Cli.Serialization;
using Auto.Models;

namespace UnitTests.Cli;

[TestFixture]
internal class CommandSerializationTests
{
	[Test]
	public void Serialize_SortsCommandsByName()
	{
		// Arrange
		var cmds = new List<CommandEntry>
		{
			CreateMinimalCommand("Zebra"),
			CreateMinimalCommand("Alpha"),
			CreateMinimalCommand("Middle")
		};

		// Act
		var json = CommandSerializer.Serialize(cmds);

		// Assert
		var doc = JsonDocument.Parse(json);
		var names = doc.RootElement.EnumerateArray()
			.Select(e => e.GetProperty("Name").GetString())
			.ToList();
		Assert.That(names, Is.EqualTo(["Alpha", "Middle", "Zebra"]));
	}

	[Test]
	public void Serialize_IsIndented()
	{
		// Arrange
		var cmd = CreateMinimalCommand();

		// Act
		var json = CommandSerializer.Serialize([cmd]);

		// Assert
		Assert.That(json, Does.Contain("\n  "));
	}

	private static CommandEntry CreateMinimalCommand(string name = "Test")
	{
		return new CommandEntry
		{
			Id = Guid.NewGuid(),
			Name = name,
			Description = "",
			Enabled = true
		};
	}
}