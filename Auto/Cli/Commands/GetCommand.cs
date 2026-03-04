using Auto.Cli.Serialization;
using Auto.Cli.Services;
using Auto.Models;
using Auto.PluginUtils;
using System.CommandLine;
using CliCommand = System.CommandLine.Command;

namespace Auto.Cli.Commands;

public static class GetCommand
{
	public static CliCommand Create(Option<string> configDirOption)
	{
		var command = new CliCommand("get") { Description = "Show command details" }
			.AddArgument<string>("name-or-id", "Command name or ID", out var nameArg)
			.AddOption<bool>("--json", "Output as JSON", out var jsonOption);

		command.SetAction(parseResult =>
		{
			var configDir = parseResult.GetValue(configDirOption);
			var nameOrId = parseResult.GetValue(nameArg);
			var json = parseResult.GetValue(jsonOption);

			var store = new CommandStore(configDir);
			if (!store.FindCommand(nameOrId, out var found))
				return 1;
			var (file, cmd) = found;

			if (json)
			{
				PrintJsonCommand(cmd);
			}
			else
			{
				PrintTableCommand(cmd, store.GetRelativePath(file));
			}

			return 0;
		});

		return command;
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
		if (cmd.Trigger.Combination is { Count: > 0 })
			Console.WriteLine($"Trigger:     {KeyNameResolver.FormatCombination(cmd.Trigger.Combination)}");
		if (cmd.Trigger.Sequence is { Length: > 0 })
			Console.WriteLine($"Sequence:    {KeyNameResolver.FormatSequence(cmd.Trigger.Sequence)}");
		if (cmd.Actions.Length > 0)
		{
			Console.WriteLine("Actions:");
			foreach (var action in cmd.Actions)
			{
				if (action.Type == ArgumentType.Plugin)
				{
					var pluginName = PluginLoader.GetPluginName(action.Value);
					Console.WriteLine(pluginName != null
						? $"  Plugin: {pluginName} ({action.Value})"
						: $"  Plugin: {action.Value}");
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