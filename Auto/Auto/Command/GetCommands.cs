using System.Text.Json;

namespace Auto.Command;

public class GetCommands
{
    public static IEnumerable<Command> Execute(IEnumerable<string> folders)
    {
        var commands = DeserializeFileContent(folders.SelectMany(x => Directory.GetFiles(x, "*.auto",
            new EnumerationOptions
            {
                RecurseSubdirectories = true
            }))).Where(x => x.Action != "RemapKey").ToList();
        foreach (var command in commands)
        {
            command.ClipboardTextRequired = command.Arguments.Any(x => x.ClipboardTextRequired) ||
                                            command.PowerShellArguments.Select(x => x.Value)
                                                .Any(x => x.Any(x => x.ClipboardTextRequired));
            command.HighlightedTextRequired = command.Arguments.Any(x => x.HighlightedTextRequired) ||
                                              command.PowerShellArguments.Select(x => x.Value)
                                                  .Any(x => x.Any(x => x.HighlightedTextRequired));
        }
        return commands;
    }

    public static IEnumerable<Command> GetRemappedKeys(IEnumerable<string> folders)
    {
        return DeserializeFileContent(folders.SelectMany(x => Directory.GetFiles(x, "*.auto", new EnumerationOptions
        {
            RecurseSubdirectories = true
        }))).Where(x => x.Action == "RemapKey");
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