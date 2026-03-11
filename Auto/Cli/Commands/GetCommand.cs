using Auto.Cli.Serialization;
using Auto.Cli.Services;
using Auto.Models;
using Auto.PluginUtils;
using System.CommandLine;
using CliCommand = System.CommandLine.Command;

namespace Auto.Cli.Commands;

public static class GetCommand
{
	private record GetInput(string ConfigDir, string NameOrId, bool Json);

	public static CliCommand Create(Option<string> configDirOption)
	{
		var command = new CliCommand("get") { Description = "Show command details" }
			.AddArgument<string>("name-or-id", "Command name or ID", out var nameArg)
			.AddOption<bool>("--json", "Output as JSON", out var jsonOption);

		command.SetActionWithErrorHandling(pr => Execute(
			new GetInput(pr.GetValue(configDirOption), pr.GetValue(nameArg), pr.GetValue(jsonOption)
		)));

		return command;
	}

	private static void Execute(GetInput input)
	{
		var store = new CommandStore(input.ConfigDir);
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
			foreach (var action in cmd.Actions)
			{
				if (action.Type == ArgumentType.Plugin)
				{
					var pluginName = PluginLoader.GetPluginName(action.Value);
					Console.WriteLine($"  Plugin: {pluginName} ({action.Value})");
				}
				else
				{
					Console.WriteLine($"  {action.Type}: {action.Value}");
				}
			}
		}
		if (cmd.PowerShellArguments.Count > 0)
		{
			Console.WriteLine("PowerShell Arguments:");
			foreach (var (key, args) in cmd.PowerShellArguments)
			{
				Console.WriteLine($"  {key}:");
				foreach (var arg in args)
					Console.WriteLine($"    {arg.ParameterName ?? "(unnamed)"} = [{FormatTokens(arg.Tokens)}]");
			}
		}
		if (cmd.PluginArguments.Count > 0)
		{
			Console.WriteLine("Plugin Arguments:");
			foreach (var (key, args) in cmd.PluginArguments)
			{
				Console.WriteLine($"  {key}:");
				foreach (var arg in args)
					Console.WriteLine($"    [{FormatTokens(arg.Tokens)}]");
			}
		}
	}

	private static string FormatTokens(ArgumentToken[] tokens) =>
		string.Join(" + ", tokens.Select(t =>
			t.Type is ArgumentType.Clipboard or ArgumentType.Highlighted ? $"{t.Type}" : $"{t.Type}:{t.Value}"));
}