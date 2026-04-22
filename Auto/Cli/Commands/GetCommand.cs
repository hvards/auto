using System.CommandLine;

using Auto.Cli.Serialization;
using Auto.Cli.Services;
using Auto.Models;
using Auto.PluginUtils;

using CliCommand = System.CommandLine.Command;

namespace Auto.Cli.Commands;

internal static class GetCommand
{
	private record GetInput(string NameOrId, bool Json);

	public static CliCommand Create(Func<ParseResult, CommandStore> resolveStore, IPluginLoader pluginLoader)
	{
		var command = new CliCommand("get") { Description = "Show command details" }
			.AddArgument<string>("name-or-id", "Command name or ID", out var nameArg)
			.AddOption<bool>("--json", "Output as JSON", out var jsonOption);

		command.SetActionWithErrorHandling(pr => Execute(
			resolveStore(pr),
			pluginLoader,
			new GetInput(
				pr.GetValue(nameArg) ?? string.Empty,
				pr.GetValue(jsonOption)
		)));

		return command;
	}

	private static void Execute(CommandStore store, IPluginLoader pluginLoader, GetInput input)
	{
		var (file, cmd) = store.GetCommand(input.NameOrId);

		if (input.Json)
		{
			PrintJsonCommand(cmd);
		}
		else
		{
			PrintTableCommand(cmd, pluginLoader, store.GetRelativePath(file));
		}
	}

	private static void PrintJsonCommand(CommandEntry cmd)
		=> Console.WriteLine(CommandSerializer.SerializeSingle(cmd));

	private static void PrintTableCommand(CommandEntry cmd, IPluginLoader pluginLoader, string filePath)
	{
		var actionTexts = cmd.Actions.Select(x => x.Target).Distinct()
			.ToDictionary(x => x, pluginLoader.GetPluginName);

		Console.WriteLine($"Name:        {cmd.Name}");
		Console.WriteLine($"Description: {cmd.Description}");
		Console.WriteLine($"Id:          {cmd.Id}");
		Console.WriteLine($"Enabled:     {cmd.Enabled}");
		Console.WriteLine($"File:        {filePath}");
		Console.WriteLine("Trigger:");
		if (cmd.Trigger.Combination is { Count: > 0 })
			Console.WriteLine($"  Combination:     {KeyNameResolver.FormatCombination(cmd.Trigger.Combination)}");
		if (cmd.Trigger.Sequence is { Length: > 0 })
			Console.WriteLine($"  Sequence:    {KeyNameResolver.FormatSequence(cmd.Trigger.Sequence)}");
		if (cmd.Actions.Length > 0)
		{
			Console.WriteLine("Actions:");
			foreach (var (action, index) in cmd.Actions.OrderBy(a => a.Order).Select((a, i) => (a, i)))
			{
				var actionText = $"{actionTexts[action.Target]} ({action.Target})";

				var varSuffix = action.Variable != null ? $" -> {action.Variable}" : "";
				Console.WriteLine($"  [{index}] {actionText}{varSuffix}");

				if (action.Arguments.Length > 0)
				{
					Console.WriteLine($"    Args:");
					foreach (var arg in action.Arguments)
					{
						Console.WriteLine($"      {FormatTokens(arg.Tokens)}");
					}
				}
			}
		}
	}

	private static string FormatTokens(ArgumentToken[] tokens) =>
		string.Join(" + ", tokens.Select(t =>
			t.Type == ArgumentType.Variable ? $"%{{{t.Value}}}" : $"{t.Value}"));
}
