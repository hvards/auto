using Auto.Cli.Services;

using CliCommand = System.CommandLine.Command;

namespace Auto.Cli.Commands;

internal static class ListKeysCommand
{
	public static CliCommand Create()
	{
		var command = new CliCommand("list-keys") { Description = "List valid key names for triggers" };

		command.SetActionWithErrorHandling(_ =>
		{
			PrintAliases();
			Console.WriteLine();
			PrintEnumKeys();
		});

		return command;
	}

	private static void PrintAliases()
	{
		Console.WriteLine("Aliases:");
		foreach (var (name, code) in KeyNameResolver.GetAliases())
			Console.WriteLine($"  {name,-12} -> {KeyNameResolver.FormatKey(code)} ({code})");
	}

	private static void PrintEnumKeys()
	{
		Console.WriteLine("Keys:");
		var keys = Enum.GetValues<Keys>()
			.Where(k => (int)k >= 0 && (int)k <= 254)
			.Distinct()
			.OrderBy(k => (int)k);

		foreach (var key in keys)
			Console.WriteLine($"  {key,-20} ({(int)key})");
	}
}