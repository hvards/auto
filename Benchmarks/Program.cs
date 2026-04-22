using BenchmarkDotNet.Running;

namespace Benchmarks;

public static class Program
{
	public static void Main()
	{
		BenchmarkSwitcher.FromAssembly(typeof(CommandBenchmarks).Assembly).Run();
	}
}
