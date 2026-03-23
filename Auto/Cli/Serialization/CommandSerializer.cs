using System.Text.Json;

using Auto.Models;

namespace Auto.Cli.Serialization;

internal static class CommandSerializer
{
	private static readonly JsonSerializerOptions WriteOptions = new() { WriteIndented = true };

	public static List<CommandEntry> Deserialize(string json)
		=> JsonSerializer.Deserialize<List<CommandEntry>>(json) ?? [];

	public static string Serialize(IEnumerable<CommandEntry> commands)
	{
		var projected = commands
			.OrderBy(x => x.Name).ThenBy(x => x.Description).ThenBy(x => x.Id)
			.Select(ProjectEntry);
		return JsonSerializer.Serialize(projected, WriteOptions);
	}

	public static string SerializeSingle(CommandEntry entry)
		=> JsonSerializer.Serialize(ProjectEntry(entry), WriteOptions);

	private static object ProjectEntry(CommandEntry x) => new
	{
		Trigger = new { x.Trigger.Combination, Sequence = x.Trigger.Sequence ?? [] },
		x.Actions,
		x.Enabled,
		x.Name,
		x.Description,
		x.Id
	};
}