using Auto.Cli.Serialization;
using Auto.Models;
using System.Text.Json;

namespace UnitTests.Cli;

[TestFixture]
public class CommandSerializationTests
{
	[Test]
	public void Serialize_DefaultPluginArguments_BecomesEmptyObject()
	{
		var cmd = new CommandEntry { Id = Guid.NewGuid(), Name = "Test", Description = "" };

		var json = CommandSerializer.Serialize([cmd]);
		var doc = JsonDocument.Parse(json);
		var pluginArgs = doc.RootElement[0].GetProperty("PluginArguments");
		Assert.That(pluginArgs.ValueKind, Is.EqualTo(JsonValueKind.Object));
		Assert.That(pluginArgs.EnumerateObject().Count(), Is.Zero);
	}

	[Test]
	public void Serialize_NullSequence_BecomesEmptyArray()
	{
		var cmd = CreateMinimalCommand();
		cmd.Trigger.Sequence = null!;

		var json = CommandSerializer.Serialize([cmd]);
		var doc = JsonDocument.Parse(json);
		var seq = doc.RootElement[0].GetProperty("Trigger").GetProperty("Sequence");
		Assert.That(seq.ValueKind, Is.EqualTo(JsonValueKind.Array));
		Assert.That(seq.GetArrayLength(), Is.Zero);
	}

	[Test]
	public void Serialize_EmptyActions_BecomesEmptyArray()
	{
		var cmd = CreateMinimalCommand();
		cmd.Actions = [];

		var json = CommandSerializer.Serialize([cmd]);
		var doc = JsonDocument.Parse(json);
		var actions = doc.RootElement[0].GetProperty("Actions");
		Assert.That(actions.GetArrayLength(), Is.Zero);
	}

	[Test]
	public void Serialize_SortsCommandsByName()
	{
		var cmds = new List<CommandEntry>
		{
			CreateMinimalCommand("Zebra"),
			CreateMinimalCommand("Alpha"),
			CreateMinimalCommand("Middle")
		};

		var json = CommandSerializer.Serialize(cmds);
		var doc = JsonDocument.Parse(json);
		var names = doc.RootElement.EnumerateArray()
			.Select(e => e.GetProperty("Name").GetString())
			.ToList();
		Assert.That(names, Is.EqualTo(new[] { "Alpha", "Middle", "Zebra" }));
	}

	[Test]
	public void Serialize_SortsDictionaryKeys()
	{
		var cmd = CreateMinimalCommand();
		cmd.PowerShellArguments = new Dictionary<string, CommandArgument[]>
		{
			["Zebra.ps1"] = [],
			["Alpha.ps1"] = [],
			["Middle.ps1"] = []
		};

		var json = CommandSerializer.Serialize([cmd]);
		var doc = JsonDocument.Parse(json);
		var keys = doc.RootElement[0].GetProperty("PowerShellArguments")
			.EnumerateObject().Select(p => p.Name).ToList();
		Assert.That(keys, Is.EqualTo(new[] { "Alpha.ps1", "Middle.ps1", "Zebra.ps1" }));
	}

	[Test]
	public void Serialize_PropertyOrder_MatchesAutoUI()
	{
		var cmd = CreateMinimalCommand();
		var json = CommandSerializer.Serialize([cmd]);
		var doc = JsonDocument.Parse(json);
		var props = doc.RootElement[0].EnumerateObject().Select(p => p.Name).ToList();
		Assert.That(props, Is.EqualTo(
		[
			"Trigger", "Actions", "PowerShellArguments", "PluginArguments",
			"Enabled", "Name", "Description", "Id"
		]));
	}

	[Test]
	public void Serialize_IsIndented()
	{
		var cmd = CreateMinimalCommand();
		var json = CommandSerializer.Serialize([cmd]);
		Assert.That(json, Does.Contain("\n  "));
	}

	[Test]
	public void Deserialize_UnicodeEscapes_ArePreserved()
	{
		var json = """[{"Trigger":{"Combination":[],"Sequence":[]},"Actions":[],"PowerShellArguments":{},"PluginArguments":{},"Enabled":true,"Name":"Dokument\u00E5","Description":"","Id":"00000000-0000-0000-0000-000000000003"}]""";
		var commands = CommandSerializer.Deserialize(json);
		Assert.That(commands[0].Name, Is.EqualTo("Dokumentå"));
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