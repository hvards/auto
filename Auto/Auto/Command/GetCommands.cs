using System.Text.Json;

namespace Auto.Command;

public class GetCommands
{
    public static IList<Command> Execute(IEnumerable<string> folders)
    {
	    var commands = GetEnabledCommands(folders).ToList();
        foreach (var command in commands)
        {
            command.ClipboardTextRequired = command.Arguments.Any(x => x.ClipboardTextRequired) ||
                                            command.PowerShellArguments.Select(x => x.Value)
                                                .Any(x => x.Any(x => x.ClipboardTextRequired));
            command.HighlightedTextRequired = command.Arguments.Any(x => x.HighlightedTextRequired) ||
                                              command.PowerShellArguments.Select(x => x.Value)
                                                  .Any(x => x.Any(x => x.HighlightedTextRequired));
        }
        return commands.ToList();
    }

    public static IEnumerable<Command> GetActions(IEnumerable<Command> commands) =>
	    commands.Where(x => x.Action != "RemapKey" && x.Action != "BlockKey");

    public static IEnumerable<Command> GetRemappedKeys(IEnumerable<Command> commands) =>
	    commands.Where(x => x.Action == "RemapKey");

    public static IEnumerable<Command> GetBlockedKeys(IEnumerable<Command> commands) =>
	    commands.Where(x => x.Action == "BlockKey");

    private static IEnumerable<Command> GetEnabledCommands(IEnumerable<string> folders)
    {
	    return DeserializeFileContent(folders.SelectMany(x => Directory.GetFiles(x, "*.auto", new EnumerationOptions
	    {
		    RecurseSubdirectories = true
	    }))).Where(x => x.Enabled);
    }

    private static IEnumerable<Command> DeserializeFileContent(IEnumerable<string> files) => files.SelectMany(file =>
    {
        try
        {
            return JsonSerializer.Deserialize<List<Command>>(File.ReadAllText(file));
        }
        catch (Exception ex)
        {
            Log.Error($"Error loading commands {file}: {ex}");
            return null;
        }
    });
}