using Auto.Cli.Services;
using Auto.Models;

namespace IntegrationTests.Cli;

[TestFixture]
internal class CommandStoreTests : CliTestBase
{
	[Test]
	public void LoadAll_EmptyDir_ReturnsEmpty()
	{
		// Act & assert
		Assert.That(TestCommandStore.LoadAll(), Is.Empty);
	}

	[Test]
	public void SaveFile_CreatesDirectories()
	{
		// Arrange
		var path = TestCommandStore.ResolvePath("sub/folder/test.json");

		// Act
		CommandStore.SaveFile(path, [MakeCommand("Test")]);

		// Assert
		Assert.That(File.Exists(path));
	}

	[Test]
	public void SaveFile_ThenLoadFile_RoundTrips()
	{
		// Arrange
		var cmd = MakeCommand("Test");
		var path = TestCommandStore.ResolvePath("test.json");

		// Act
		CommandStore.SaveFile(path, [cmd]);
		var loaded = CommandStore.LoadFile(path);

		// Assert
		Assert.That(loaded, Has.Count.EqualTo(1));
		Assert.That(loaded[0].Name, Is.EqualTo("Test"));
		Assert.That(loaded[0].Id, Is.EqualTo(cmd.Id));
	}

	[Test]
	public void Find_ByName_ReturnsMatch()
	{
		// Arrange
		var (cmd, _) = SaveTestCommand();

		// Act
		var result = TestCommandStore.Find("CommandName");

		// Assert
		Assert.That(result, Is.Not.Null);
		Assert.That(result!.Value.Command.Id, Is.EqualTo(cmd.Id));
	}

	[Test]
	public void Find_ByName_CaseSensitive()
	{
		// Arrange
		CommandStore.SaveFile(TestCommandStore.ResolvePath("test.json"), [MakeCommand("MyCommand")]);

		// Act & assert
		Assert.That(TestCommandStore.Find("mycommand"), Is.Null);
	}

	[Test]
	public void Find_ById_ReturnsMatch()
	{
		// Arrange
		var (cmd, _) = SaveTestCommand();

		// Act
		var result = TestCommandStore.Find(cmd.Id.ToString());

		// Assert
		Assert.That(result, Is.Not.Null);
		Assert.That(result!.Value.Command.Name, Is.EqualTo("CommandName"));
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
		Assert.That(TestCommandStore.Find("CommandName"), Is.Null);
		Assert.That(CommandStore.LoadFile(path), Is.Empty);
	}

	[Test]
	public void LoadAll_AcrossMultipleFiles()
	{
		// Arrange
		CommandStore.SaveFile(TestCommandStore.ResolvePath("a.json"), [MakeCommand("A1"), MakeCommand("A2")]);
		CommandStore.SaveFile(TestCommandStore.ResolvePath("b.json"), [MakeCommand("B1")]);

		// Act
		var all = TestCommandStore.LoadAll();

		// Assert
		Assert.That(all, Has.Count.EqualTo(3));
	}

	private static CommandEntry MakeCommand(
		string name = "CommandName",
		string[]? trigger = null,
		bool enabled = true,
		CommandAction[]? actions = null,
		string[]? sequence = null) => new()
		{
			Id = Guid.NewGuid(),
			Name = name,
			Description = "",
			Enabled = enabled,
			Trigger = new Trigger
			{
				Combination = sequence == null ? [.. KeyNameResolver.ParseInput(trigger ?? ["LCtrl", "LWin", "T"])] : [],
				Sequence = sequence != null ? [.. KeyNameResolver.ParseInput(sequence)] : []
			},
			Actions = actions ?? []
		};

	private static CommandAction PluginAction(string id, int order = 0, string? variable = null,
		params CommandArgument[] args) => new()
		{
			Type = ActionType.Plugin,
			Target = id,
			Order = order,
			Variable = variable,
			Arguments = args
		};

	private static CommandArgument TextArg(string value) => new()
	{
		Tokens = [new ArgumentToken { Type = ArgumentType.Text, Value = value }]
	};

	private (CommandEntry Command, string Path) SaveTestCommand(string name = "CommandName")
	{
		var cmd = MakeCommand(name,
			actions: [PluginAction("21092f13-5366-4cba-90df-66bd123e66a5", args: TextArg("https://example.com"))]);
		var path = TestCommandStore.ResolvePath("test.json");
		CommandStore.SaveFile(path, [cmd]);
		return (cmd, path);
	}
}