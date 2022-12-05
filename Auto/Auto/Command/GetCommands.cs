using System.Text.Json;

namespace Auto.Command;

public class GetCommands
{
    public static IEnumerable<Command> Execute(IEnumerable<string> folders) => DeserializeFileContent(folders.SelectMany(x => Directory.GetFiles(x, "*.auto", new EnumerationOptions
    {
        RecurseSubdirectories = true
    }))).Where(x => x.Action != "RemapKey");

    public static IEnumerable<Command> GetRemappedKeys(IEnumerable<string> folders) => DeserializeFileContent(folders.SelectMany(x => Directory.GetFiles(x, "*.auto", new EnumerationOptions
    {
        RecurseSubdirectories = true
    }))).Where(x => x.Action == "RemapKey");

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