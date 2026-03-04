using Auto.Cli.Services;
using Auto.Models;

namespace UnitTests.Cli;

[TestFixture]
public class CommandStoreTests
{
	private string _tempDir = null!;
	private string _commandsDir = null!;

	[SetUp]
	public void SetUp()
	{
		_tempDir = Path.Combine(Path.GetTempPath(), "autocli-test-" + Guid.NewGuid().ToString("N")[..8]);
		_commandsDir = Path.Combine(_tempDir, "commands");
		Directory.CreateDirectory(_commandsDir);
	}

	[TearDown]
	public void TearDown()
	{
		if (Directory.Exists(_tempDir)) Directory.Delete(_tempDir, true);
	}

	[Test]
	public void LoadAll_EmptyDir_ReturnsEmpty()
	{
		var store = new CommandStore(_tempDir);
		Assert.That(store.LoadAll(), Is.Empty);
	}

	[Test]
	public void LoadAll_NonExistentDir_ReturnsEmpty()
	{
		var store = new CommandStore(Path.Combine(_tempDir, "nonexistent"));
		Assert.That(store.LoadAll(), Is.Empty);
	}

	[Test]
	public void SaveFile_CreatesDirectories()
	{
		var store = new CommandStore(_tempDir);
		var cmd = CreateCommand("Test");
		var path = store.ResolvePath("sub/folder/test.json");
		CommandStore.SaveFile(path, [cmd]);
		Assert.That(File.Exists(path));
	}

	[Test]
	public void SaveFile_ThenLoadFile_RoundTrips()
	{
		var store = new CommandStore(_tempDir);
		var cmd = CreateCommand("Test");
		var path = store.ResolvePath("test.json");
		CommandStore.SaveFile(path, [cmd]);

		var loaded = CommandStore.LoadFile(path);
		Assert.That(loaded, Has.Count.EqualTo(1));
		Assert.That(loaded[0].Name, Is.EqualTo("Test"));
		Assert.That(loaded[0].Id, Is.EqualTo(cmd.Id));
	}

	[Test]
	public void Find_ByName_CaseInsensitive()
	{
		var store = new CommandStore(_tempDir);
		var cmd = CreateCommand("MyCommand");
		CommandStore.SaveFile(store.ResolvePath("test.json"), [cmd]);

		var result = store.Find("mycommand");
		Assert.That(result, Is.Not.Null);
		Assert.That(result!.Value.Command.Name, Is.EqualTo("MyCommand"));
	}

	[Test]
	public void Find_ById()
	{
		var store = new CommandStore(_tempDir);
		var cmd = CreateCommand("Test");
		CommandStore.SaveFile(store.ResolvePath("test.json"), [cmd]);

		var result = store.Find(cmd.Id.ToString());
		Assert.That(result, Is.Not.Null);
		Assert.That(result!.Value.Command.Id, Is.EqualTo(cmd.Id));
	}

	[Test]
	public void Find_NotFound_ReturnsNull()
	{
		var store = new CommandStore(_tempDir);
		CommandStore.SaveFile(store.ResolvePath("test.json"), [CreateCommand("Existing")]);
		Assert.That(store.Find("NonExistent"), Is.Null);
	}

	[Test]
	public void GetFiles_FindsRecursive()
	{
		var store = new CommandStore(_tempDir);
		CommandStore.SaveFile(store.ResolvePath("a.json"), [CreateCommand("A")]);
		CommandStore.SaveFile(store.ResolvePath("sub/b.json"), [CreateCommand("B")]);

		var files = store.GetFiles().ToList();
		Assert.That(files, Has.Count.EqualTo(2));
	}

	[Test]
	public void LoadAll_AcrossMultipleFiles()
	{
		var store = new CommandStore(_tempDir);
		CommandStore.SaveFile(store.ResolvePath("a.json"), [CreateCommand("A1"), CreateCommand("A2")]);
		CommandStore.SaveFile(store.ResolvePath("b.json"), [CreateCommand("B1")]);

		var all = store.LoadAll();
		Assert.That(all, Has.Count.EqualTo(3));
	}


	[Test]
	public void GetRelativePath_ReturnsForwardSlashes()
	{
		var store = new CommandStore(_tempDir);
		var abs = Path.Combine(_commandsDir, "sub", "test.json");
		var rel = store.GetRelativePath(abs);
		Assert.That(rel, Does.Not.Contain("\\"));
		Assert.That(rel, Is.EqualTo("sub/test.json"));
	}

	private static CommandEntry CreateCommand(string name) => new()
	{
		Id = Guid.NewGuid(),
		Name = name,
		Description = "",
		Enabled = true,
		Trigger = new Trigger { Combination = [162, 91], Sequence = [] },
		Actions = [],
		PowerShellArguments = [],
		PluginArguments = []
	};
}