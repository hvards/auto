using Auto.Models;

namespace Auto.Cli.Services;

internal interface ITriggerCreator
{
	Trigger CreateTrigger(string[]? combination, string[]? sequence);
	HashSet<ushort> GetCombination(string[]? input);
	ushort[] GetSequence(string[]? input);
}

internal class TriggerCreator(IKeyRecorder keyRecorder) : ITriggerCreator
{
	public Trigger CreateTrigger(string[]? combination, string[]? sequence)
	{
		return new Trigger
		{
			Combination = GetCombination(combination),
			Sequence = GetSequence(sequence)
		};
	}

	public HashSet<ushort> GetCombination(string[]? input)
	{
		return [.. GetTrigger(input, keyRecorder.RecordCombination)];
	}

	public ushort[] GetSequence(string[]? input)
	{
		return [.. GetTrigger(input, keyRecorder.RecordSequence)];
	}

	private static IEnumerable<ushort> GetTrigger(string[]? input, Func<IEnumerable<ushort>> recordInput)
	{
		if (input == null)
		{
			return [];
		}
		if (input.Length == 0)
		{
			return recordInput();
		}
		return KeyNameResolver.ParseInput(input);
	}
}
