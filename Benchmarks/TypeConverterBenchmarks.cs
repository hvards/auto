using Auto.Handlers;

using BenchmarkDotNet.Attributes;

namespace Benchmarks;

[MemoryDiagnoser]
public class TypeConverterBenchmarks
{
	private string inputString = string.Empty;

	[IterationSetup]
	public void IterationSetup()
	{
		inputString = string.Join(';', Enumerable.Range(1, 100));
	}

	[Benchmark]
	public void Benchmark_List_TypeConverter()
	{
		for (var i = 0; i < 1_000; i++)
		{
			TypeConverter.Convert(inputString, typeof(List<int>));
		}
	}

	[Benchmark]
	public void Benchmark_Array_TypeConverter()
	{
		for (var i = 0; i < 1_000; i++)
		{
			TypeConverter.Convert(inputString, typeof(int[]));
		}
	}
}