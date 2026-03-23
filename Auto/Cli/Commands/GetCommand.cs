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

	public static CliCommand Create(Func<ParseResult, CommandStore> resolveStore)
	{
		var command = new CliCommand("get") { Description = "Show command details" }
			.AddArgument<string>("name-or-id", "Command name or ID", out var nameArg)
			.AddOption<bool>("--json", "Output as JSON", out var jsonOption);

		command.SetActionWithErrorHandling(pr => Execute(
			resolveStore(pr),
			new GetInput(
				pr.GetValue(nameArg) ?? string.Empty,
				pr.GetValue(jsonOption)
		)));

		return command;
	}

	private static void Execute(CommandStore store, GetInput input)
	{
		var (file, cmd) = store.GetCommand(input.NameOrId);

		if (input.Json)
		{
			PrintJsonCommand(cmd);
		}
		else
		{
			PrintTableCommand(cmd, store.GetRelativePath(file));
		}
	}

	private static void PrintJsonCommand(CommandEntry cmd)
		=> Console.WriteLine(CommandSerializer.SerializeSingle(cmd));

	private static void PrintTableCommand(CommandEntry cmd, string filePath)
	{
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
				var actionText = action.Type == ActionType.Plugin
					? $"Plugin: {PluginLoader.GetPluginName(action.Target)} ({action.Target})"
					: $"PowerShell: {action.Target}";

				var varSuffix = action.Variable != null ? $" -> {action.Variable}" : "";
				Console.WriteLine($"  [{index}] {actionText}{varSuffix}");

				if (action.Arguments.Length > 0)
				{
					Console.WriteLine($"    Args:");
					foreach (var arg in action.Arguments)
					{
						var parameterNamePrefix = arg.ParameterName != null ? arg.ParameterName + " = " : string.Empty;
						Console.WriteLine($"      {parameterNamePrefix}{FormatTokens(arg.Tokens)}");
					}
				}
			}
		}
	}

	private static string FormatTokens(ArgumentToken[] tokens) =>
		string.Join(" + ", tokens.Select(t =>
			t.Type == ArgumentType.Variable ? $"%{{{t.Value}}}" : $"{t.Value}"));
}