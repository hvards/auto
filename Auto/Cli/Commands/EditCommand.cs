using Auto.Cli.Services;
using Auto.Models;
using Auto.PluginUtils;
using System.CommandLine;

using CliCommand = System.CommandLine.Command;

namespace Auto.Cli.Commands;

public static class EditCommand
{
	private record EditInput(
		string ConfigDir,
		string NameOrId,
		string[] Combination,
		string[] Sequence,
		string Description,
		string NewName,
		string[] Plugins,
		string[] PowerShellScripts,
		string[] PluginArguments,
		string[] PowerShellArguments
	);

	public static CliCommand Create(Option<string> configDirOption)
	{
		var command = new CliCommand("edit") { Description = "Modify an existing command" }
			.AddArgument<string>("name-or-id", "Command name or ID", out var nameArg)
			.AddOption<string[]>("--combination", "New key combination", out var combinationOption)
			.AddOption<string[]>("--sequence", "New key sequence", out var sequenceOption)
			.AddOption<string>("--description", "New description", out var descOption)
			.AddOption<string>("--name", "New name", out var renameOption)
			.AddOption<string[]>("--plugin", "Set plugin(s) to execute (name or GUID)", out var pluginOption)
			.AddOption<string[]>("--powershell", "Set PowerShell script(s) to execute", out var psScriptOption)
			.AddOption<string[]>("--arg", "Plugin argument (--arg <plugin> <values...>)", out var argOption)
			.AddOption<string[]>("--ps-arg",
				"PowerShell argument (--ps-arg <script> <param value>...)",
				out var psArgOption);

		command.SetActionWithErrorHandling(pr => Execute(
			new EditInput(
				pr.GetValue(configDirOption),
				pr.GetValue(nameArg),
				pr.GetValue(combinationOption),
				pr.GetValue(sequenceOption),
				pr.GetValue(descOption),
				pr.GetValue(renameOption),
				pr.GetValue(pluginOption) ?? [],
				pr.GetValue(psScriptOption) ?? [],
				pr.GetValue(argOption) ?? [],
				pr.GetValue(psArgOption) ?? []
			)
		));

		return command;
	}

	private static void Execute(EditInput input)
	{
		var store = new CommandStore(input.ConfigDir);
		var (file, cmd) = store.GetCommand(input.NameOrId);
		var commands = CommandStore.LoadFile(file);
		var target = commands.First(c => c.Id == cmd.Id);

		if (input.Combination is { Length: > 0 })
			target.Trigger.Combination = KeyNameResolver.ParseCombination(input.Combination);
		if (input.Sequence is { Length: > 0 })
			target.Trigger.Sequence = KeyNameResolver.ParseSequence(input.Sequence);

		target.Description = input.Description ?? target.Description;
		target.Name = input.NewName ?? target.Name;

		if (input.Plugins.Length > 0 || input.PowerShellScripts.Length > 0)
		{
			var actionList = new List<ArgumentToken>();

			foreach (var plugin in input.Plugins)
			{
				var id = PluginLoader.ResolvePlugin(plugin);
				actionList.Add(new ArgumentToken { Type = ArgumentType.Plugin, Value = id });
			}

			foreach (var ps in input.PowerShellScripts)
				actionList.Add(new ArgumentToken { Type = ArgumentType.PowerShell, Value = ps });

			target.Actions = [.. actionList];

			target.PluginArguments = PluginHelper.GroupPluginArgs(input.PluginArguments);
			target.PowerShellArguments = PluginHelper.GroupPsArgs(input.PowerShellArguments);
		}
		else if (input.PluginArguments.Length > 0 || input.PowerShellArguments.Length > 0)
		{
			foreach (var (key, value) in PluginHelper.GroupPluginArgs(input.PluginArguments))
				target.PluginArguments[key] = value;

			var psActionSet = target.Actions
				.Where(a => a.Type == ArgumentType.PowerShell)
				.Select(a => a.Value).ToHashSet(StringComparer.OrdinalIgnoreCase);
			foreach (var (key, value) in PluginHelper.GroupPsArgs(input.PowerShellArguments))
				target.PowerShellArguments[key] = value;
		}

		CommandStore.SaveFile(file, commands);
		Console.WriteLine($"Updated '{target.Name}'");
	}
}
