using Auto.Models;

namespace Auto.Cli.Services;

public static class KeyNameResolver
{
	private static readonly Dictionary<string, ushort> Aliases = new(StringComparer.OrdinalIgnoreCase)
	{
		["LCtrl"] = 162,
		["RCtrl"] = 163,
		["Ctrl"] = 162,
		["LAlt"] = 164,
		["RAlt"] = 165,
		["Alt"] = 164,
		["LShift"] = 160,
		["RShift"] = 161,
		["Shift"] = 160,
		["Win"] = 91,
	};

	internal static IReadOnlyDictionary<string, ushort> GetAliases()
		=> Aliases;

	public static ushort ParseKey(string name)
	{
		if (Aliases.TryGetValue(name, out var code))
			return code;
		if (Enum.TryParse<Keys>(name, ignoreCase: true, out var key) && (int)key <= 254)
			return (ushort)key;
		if (ushort.TryParse(name, out var numeric))
			return numeric;
		throw new ArgumentException($"Unknown key name: '{name}'");
	}

	public static string FormatKey(ushort code)
	{
		return Enum.GetName((Keys)code) ?? code.ToString();
	}

	public static HashSet<ushort> ParseCombination(string[] keys)
	{
		return [.. keys.Select(k => ParseKey(k.Trim()))];
	}

	public static ushort[] ParseSequence(string[] keys)
	{
		return [.. keys.Select(k => ParseKey(k.Trim()))];
	}

	public static string FormatCombination(HashSet<ushort> combination)
	{
		if (combination == null || combination.Count == 0) return "";
		return string.Join("+", combination.OrderBy(x => x).Select(FormatKey));
	}

	public static string FormatSequence(ushort[] sequence)
	{
		if (sequence == null || sequence.Length == 0) return "";
		return string.Join(",", sequence.Select(FormatKey));
	}

	public static Trigger ParseTrigger(string[] combination, string[] sequence)
	{
		return new Trigger
		{
			Combination = combination.Length > 0 ? ParseCombination(combination) : [],
			Sequence = sequence.Length > 0 ? ParseSequence(sequence) : []
		};
	}
}