using Auto.Command;

using BenchmarkDotNet.Attributes;

namespace Benchmarks;

[MemoryDiagnoser]
public class CommandBenchmarks
{
	private Trigger[] _sequenceTriggers = [];
	private Trigger[] _combinationTriggers = [];
	private ushort[] _letters = [];
	private HashSet<ushort>[] _combinations = [];

	[IterationSetup]
	public void IterationSetup()
	{
		_letters = Enumerable.Range('a', 'i').Select(x => (ushort)x).ToArray();
		_sequenceTriggers =
			(from letter in _letters
			 from letter2 in _letters
			 select new Trigger
			 {
				 Sequence = Enumerable.Range(letter, letter2).Select(x => (ushort)x).ToArray(),
				 Combination = []
			 }
			).ToArray();

		_combinations = GetPowerSet(Enumerable.Range(1, 12).Select(x => (ushort)x).ToList())
			.Select(x => new HashSet<ushort>(x)).ToArray();
		_combinationTriggers = _combinations.Select(x => new Trigger
		{
			Combination = x,
			Sequence = null
		}).ToArray();
	}

	[Benchmark]
	public void Benchmark_Sequence_Check()
	{
		foreach (var letter in _letters)
		{
			foreach (var trigger in _sequenceTriggers)
			{
				trigger.Check(null, letter);
			}
		}
	}


	[Benchmark]
	public void Benchmark_Combination_Check()
	{
		foreach (var combination in _combinations)
		{
			foreach (var trigger in _combinationTriggers)
			{
				trigger.Check(combination, 'a');
			}
		}
	}

	// source: https://stackoverflow.com/a/3319689
	private static IEnumerable<IEnumerable<T>> GetPowerSet<T>(List<T> list)
	{
		return from m in Enumerable.Range(0, 1 << list.Count)
			   select
				   from i in Enumerable.Range(0, list.Count)
				   where (m & (1 << i)) != 0
				   select list[i];
	}
}