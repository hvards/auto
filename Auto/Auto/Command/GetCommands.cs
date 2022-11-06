using System.Text.Json;

namespace Auto.Command;

public class GetCommands
{
    public static IEnumerable<Command> Execute(string[] files) =>
        files.SelectMany(file =>
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