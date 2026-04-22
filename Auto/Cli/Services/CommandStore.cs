using System.IO;

using Auto.Cli.Serialization;
using Auto.Models;

namespace Auto.Cli.Services;

internal class CommandStore(string configDir)
{
	private readonly string _commandsDir = Path.Combine(configDir, "commands");

	public IEnumerable<string> GetFiles()
	{
		if (!Directory.Exists(_commandsDir))
			return [];
		return Directory.GetFiles(_commandsDir, "*.json", SearchOption.AllDirectories);
	}

	public string GetRelativePath(string absolutePath)
		=> Path.GetRelativePath(_commandsDir, absolutePath);

	public List<(string File, CommandEntry Command)> LoadAll()
		=> [.. GetFiles().SelectMany(f => LoadFile(f).Select(c => (f, c)))];

	public (string File, CommandEntry Command)? Find(string nameOrId)
	{
		var all = LoadAll();
		if (Guid.TryParse(nameOrId, out var id))
		{
			var byId = all.FirstOrDefault(x => x.Command.Id == id);
			if (byId.Command != null) return byId;
		}
		var byName = all.FirstOrDefault(x => string.Equals(x.Command.Name, nameOrId));
		return byName.Command != null ? byName : null;
	}

	public (string File, CommandEntry Command) GetCommand(string nameOrId)
	{
		var found = Find(nameOrId);
		return found ?? throw new ArgumentException($"Command not found: {nameOrId}");
	}

	public CommandEntry Update(string nameOrId, Action<CommandEntry> update)
	{
		var (file, cmd) = GetCommand(nameOrId);
		var commands = LoadFile(file);
		var target = commands.First(c => c.Id == cmd.Id);
		update(target);
		SaveFile(file, commands);
		return target;
	}
	public string ResolvePath(string relativePath)
		=> Path.GetFullPath(Path.Combine(_commandsDir, relativePath));

	public static List<CommandEntry> LoadFile(string path)
		=> CommandSerializer.Deserialize(File.ReadAllText(path));

	public static void SaveFile(string path, List<CommandEntry> commands)
	{
		var dir = Path.GetDirectoryName(path);
		if (dir != null && !Directory.Exists(dir))
			Directory.CreateDirectory(dir);
		File.WriteAllText(path, CommandSerializer.Serialize(commands));
	}

}
