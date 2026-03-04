using Auto.Cli.Serialization;
using Auto.Cli.Services;
using Auto.Models;

namespace IntegrationTests;

[TestFixture]
public class CommandStoreIntegrationTests
{
	private string _tempDir = null!;
	private CommandStore _store = null!;

	[SetUp]
	public void SetUp()
	{
		_tempDir = Path.Combine(Path.GetTempPath(), "autocli-integ-" + Guid.NewGuid().ToString("N")[..8]);
		Directory.CreateDirectory(Path.Combine(_tempDir, "commands"));
		_store = new CommandStore(_tempDir);
	}

	[TearDown]
	public void TearDown()
	{
		if (Directory.Exists(_tempDir))
			Directory.Delete(_tempDir, true);
	}

	private static CommandEntry MakeCommand(
		string name = "Test URL",
		string trigger = "LCtrl+LWin+T",
		bool enabled = true,
		ArgumentToken[]? actions = null,
		Dictionary<string, CommandArgument[]>? pluginArgs = null,
		Dictionary<string, CommandArgument[]>? psArgs = null,
		string? sequence = null) => new()
		{
			Id = Guid.NewGuid(),
			Name = name,
			Description = "",
			Enabled = enabled,
			Trigger = new Trigger
			{
				Combination = sequence == null ? KeyNameResolver.ParseCombination(trigger) : [],
				Sequence = sequence != null ? KeyNameResolver.ParseSequence(sequence) : []
			},
			Actions = actions ?? [],
			PowerShellArguments = psArgs ?? [],
			PluginArguments = pluginArgs ?? []
		};

	private static ArgumentToken PluginAction(string id) => new() { Type = ArgumentType.Plugin, Value = id };
	private static ArgumentToken PsAction(string script) => new() { Type = ArgumentType.PowerShell, Value = script };
	private static CommandArgument TextArg(string value) => new()
	{
		Tokens = [new ArgumentToken { Type = ArgumentType.Text, Value = value }]
	};

	private (CommandEntry Command, string Path) SaveTestCommand(string name = "Test URL")
	{
		var cmd = MakeCommand(name,
			actions: [PluginAction("21092f13-5366-4cba-90df-66bd123e66a5")],
			pluginArgs: new() { ["21092f13-5366-4cba-90df-66bd123e66a5"] = [TextArg("https://example.com")] });
		var path = _store.ResolvePath("test.json");
		CommandStore.SaveFile(path, [cmd]);
		return (cmd, path);
	}

	[Test]
	public void Add_SaveFile_LoadAll_ReturnsCommand()
	{
		// Arrange
		SaveTestCommand();

		// Act
		var result = _store.LoadAll();

		// Assert
		Assert.That(result, Has.Count.EqualTo(1));
		Assert.That(result[0].Command.Name, Is.EqualTo("Test URL"));
	}

	[Test]
	public void Find_ByName_ReturnsMatch()
	{
		// Arrange
		var (cmd, _) = SaveTestCommand();

		// Act
		var result = _store.Find("Test URL");

		// Assert
		Assert.That(result, Is.Not.Null);
		Assert.That(result!.Value.Command.Id, Is.EqualTo(cmd.Id));
	}

	[Test]
	public void Find_ById_ReturnsMatch()
	{
		// Arrange
		var (cmd, _) = SaveTestCommand();

		// Act
		var result = _store.Find(cmd.Id.ToString());

		// Assert
		Assert.That(result, Is.Not.Null);
		Assert.That(result!.Value.Command.Name, Is.EqualTo("Test URL"));
	}

	[Test]
	public void Edit_Trigger_Persists()
	{
		// Arrange
		var (cmd, path) = SaveTestCommand();
		var commands = CommandStore.LoadFile(path);
		commands.First(c => c.Id == cmd.Id).Trigger.Combination = KeyNameResolver.ParseCombination("LCtrl+LWin+R");

		// Act
		CommandStore.SaveFile(path, commands);

		// Assert
		var result = _store.Find("Test URL");
		Assert.That(result!.Value.Command.Trigger.Combination, Does.Contain((ushort)82));
	}

	[Test]
	public void Disable_Command_Persists()
	{
		// Arrange
		var (cmd, path) = SaveTestCommand();
		var commands = CommandStore.LoadFile(path);
		commands.First(c => c.Id == cmd.Id).Enabled = false;

		// Act
		CommandStore.SaveFile(path, commands);

		// Assert
		Assert.That(_store.Find("Test URL")!.Value.Command.Enabled, Is.False);
	}

	[Test]
	public void Enable_Command_Persists()
	{
		// Arrange
		var (cmd, path) = SaveTestCommand();
		var commands = CommandStore.LoadFile(path);
		commands.First(c => c.Id == cmd.Id).Enabled = false;
		CommandStore.SaveFile(path, commands);
		commands = CommandStore.LoadFile(path);
		commands.First(c => c.Id == cmd.Id).Enabled = true;

		// Act
		CommandStore.SaveFile(path, commands);

		// Assert
		Assert.That(_store.Find("Test URL")!.Value.Command.Enabled, Is.True);
	}

	[Test]
	public void Delete_Command_RemovesFromFile()
	{
		// Arrange
		var (cmd, path) = SaveTestCommand();
		var commands = CommandStore.LoadFile(path);
		commands.RemoveAll(c => c.Id == cmd.Id);

		// Act
		CommandStore.SaveFile(path, commands);

		// Assert
		Assert.That(_store.Find("Test URL"), Is.Null);
		Assert.That(CommandStore.LoadFile(path), Is.Empty);
	}

	[Test]
	public void Add_OpenUrl_CreatesCorrectStructure()
	{
		// Arrange
		const string pluginId = "21092f13-5366-4cba-90df-66bd123e66a5";
		var cmd = MakeCommand("Open Example", "LCtrl+LWin+LAlt+R",
			actions: [PluginAction(pluginId)],
			pluginArgs: new() { [pluginId] = [TextArg("https://example.com")] });
		var path = _store.ResolvePath("test.json");

		// Act
		CommandStore.SaveFile(path, [cmd]);

		// Assert
		var result = CommandSerializer.Deserialize(File.ReadAllText(path))[0];
		Assert.That(result.Actions[0].Type, Is.EqualTo(ArgumentType.Plugin));
		Assert.That(result.Actions[0].Value, Is.EqualTo(pluginId));
		Assert.That(result.PluginArguments![pluginId][0].Tokens[0].Value, Is.EqualTo("https://example.com"));
	}

	[Test]
	public void Add_SendKeys_CreatesCorrectStructure()
	{
		// Arrange
		const string pluginId = "902a5fec-8684-4b83-a959-453d81de5479";
		var cmd = MakeCommand("Paste Enter", "LCtrl+LWin+E",
			actions: [PluginAction(pluginId)],
			pluginArgs: new() { [pluginId] = [TextArg("{Enter}")] });
		var path = _store.ResolvePath("test.json");

		// Act
		CommandStore.SaveFile(path, [cmd]);

		// Assert
		var result = CommandSerializer.Deserialize(File.ReadAllText(path))[0];
		Assert.That(result.PluginArguments![pluginId][0].Tokens[0].Value, Is.EqualTo("{Enter}"));
	}

	[Test]
	public void Add_PowerShell_CreatesCorrectStructure()
	{
		// Arrange
		var cmd = MakeCommand("Run Script", "LCtrl+LWin+LAlt+K",
			actions: [PsAction("StopProcs.ps1")],
			psArgs: new() { ["StopProcs.ps1"] = [TextArg(@"C:\a\App")] });
		var path = _store.ResolvePath("test.json");

		// Act
		CommandStore.SaveFile(path, [cmd]);

		// Assert
		var result = CommandSerializer.Deserialize(File.ReadAllText(path))[0];
		Assert.That(result.Actions[0].Type, Is.EqualTo(ArgumentType.PowerShell));
		Assert.That(result.Actions[0].Value, Is.EqualTo("StopProcs.ps1"));
		Assert.That(result.PowerShellArguments["StopProcs.ps1"][0].Tokens[0].Value, Is.EqualTo(@"C:\a\App"));
	}

	[Test]
	public void Add_SequenceTrigger_CreatesCorrectStructure()
	{
		// Arrange
		const string pluginId = "902a5fec-8684-4b83-a959-453d81de5479";
		var cmd = MakeCommand("Sequence Command", sequence: "A,S,F,C,E",
			actions: [PluginAction(pluginId)],
			pluginArgs: new() { [pluginId] = [TextArg("test")] });
		var path = _store.ResolvePath("test.json");

		// Act
		CommandStore.SaveFile(path, [cmd]);

		// Assert
		var result = CommandSerializer.Deserialize(File.ReadAllText(path))[0];
		Assert.That(result.Trigger.Sequence, Is.EqualTo(new ushort[] { 65, 83, 70, 67, 69 }));
		Assert.That(result.Trigger.Combination, Is.Empty);
	}

	[Test]
	public void Export_Import_RoundTrips()
	{
		// Arrange
		var cmd1 = MakeCommand("Cmd1", enabled: true);
		var cmd2 = MakeCommand("Cmd2", trigger: "LAlt+LWin", enabled: false);
		CommandStore.SaveFile(_store.ResolvePath("source.json"), [cmd1, cmd2]);

		// Act
		var exported = CommandSerializer.Serialize([.. _store.LoadAll().Select(x => x.Command)]);
		var importFile = Path.Combine(_tempDir, "export.json");
		File.WriteAllText(importFile, exported);
		var imported = CommandSerializer.Deserialize(File.ReadAllText(importFile));

		// Assert
		Assert.That(imported, Has.Count.EqualTo(2));
		Assert.That(imported.Any(c => c.Id == cmd1.Id));
		Assert.That(imported.Any(c => c.Id == cmd2.Id));
	}

	[Test]
	public void MultipleFiles_FindAcrossAll()
	{
		// Arrange
		var cmd1 = MakeCommand("InFileA");
		var cmd2 = MakeCommand("InFileB", trigger: "LAlt+LWin");
		CommandStore.SaveFile(_store.ResolvePath("a.json"), [cmd1]);
		CommandStore.SaveFile(_store.ResolvePath("sub/b.json"), [cmd2]);

		// Assert
		Assert.That(_store.Find("InFileA"), Is.Not.Null);
		Assert.That(_store.Find("InFileB"), Is.Not.Null);
		Assert.That(_store.Find(cmd2.Id.ToString())!.Value.Command.Name, Is.EqualTo("InFileB"));
	}
}