using System.Text.Json;

namespace Auto.CommandJson;

public class GetCommands
{
    public static IEnumerable<Command> Execute(string[] files) =>
        files.SelectMany(file =>
        {
            try
            {
                return JsonSerializer.Deserialize<List<Command>>(File.ReadAllText(file), new JsonSerializerOptions
                {
                    Converters = {new UshortSerializerConverter()}
                });
            }
            catch (Exception ex)
            {
                Log.Error($"Error loading commands {file}: {ex}");
                return null;
            }
        });
}