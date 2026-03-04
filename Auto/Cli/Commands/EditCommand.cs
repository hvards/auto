using Auto.Cli.Services;
using Auto.Models;
using Auto.PluginUtils;
using System.CommandLine;
using CliCommand = System.CommandLine.Command;

namespace Auto.Cli.Commands;

public static class EditCommand
{
	public static CliCommand Create(Option<string> configDirOption)
	{
		var command = new CliCommand("edit") { Description = "Modify an existing command" }
			.AddArgument<string>("name-or-id", "Command name or ID", out var nameArg)
			.AddOption<string>("--combination", "New key combination", out var combinationOption)
			.AddOption<string>("--sequence", "New key sequence", out var sequenceOption)
			.AddOption<string>("--description", "New description", out var descOption)
			.AddOption<string>("--name", "New name", out var renameOption)
			.AddOption<string>("--plugin", "Replace actions with plugin (name or GUID)", out var pluginOption)
			.AddOption<string[]>("--plugin-arg",
				"Plugin argument (use with --plugin, or guid:value with --action)",
				out var pluginArgOption)
			.AddOption<string>("--powershell", "Replace actions with PowerShell script", out var psScriptOption)
			.AddOption<string[]>(
				"--action", "Replace actions (repeatable, e.g. plugin:guid, ps:script.ps1, %clipboard)",
				out var actionOption)
			.AddOption<string[]>("--ps-arg", "PowerShell argument (script:paramName=value)", out var psArgOption);

		command.SetAction(parseResult =>
		{
			var configDir = parseResult.GetValue(configDirOption);
			var nameOrId = parseResult.GetValue(nameArg);

			var store = new CommandStore(configDir);
			if (!store.FindCommand(nameOrId, out var found))
				return 1;
			var (file, cmd) = found;
			var commands = CommandStore.LoadFile(file);
			var target = commands.First(c => c.Id == cmd.Id);

			var combination = parseResult.GetValue(combinationOption);
			var sequence = parseResult.GetValue(sequenceOption);
			var desc = parseResult.GetValue(descOption);
			var rename = parseResult.GetValue(renameOption);
			var plugin = parseResult.GetValue(pluginOption);
			var pluginArgs = parseResult.GetValue(pluginArgOption) ?? [];
			var psScript = parseResult.GetValue(psScriptOption);
			var actions = parseResult.GetValue(actionOption) ?? [];
			var psArgs = parseResult.GetValue(psArgOption) ?? [];

			try
			{
				if (combination != null)
					target.Trigger.Combination = KeyNameResolver.ParseCombination(combination);
				if (sequence != null)
					target.Trigger.Sequence = KeyNameResolver.ParseSequence(sequence);
			}
			catch (ArgumentException ex) { Console.Error.WriteLine(ex.Message); return 1; }

			if (desc != null)
				target.Description = desc;
			if (rename != null)
				target.Name = rename;

			try
			{
				if (plugin != null)
				{
					var resolvedId = PluginLoader.ResolvePlugin(plugin);
					if (resolvedId == null)
					{
						Console.Error.WriteLine($"Unknown plugin: {plugin}");
						return 1;
					}

					target.Actions = [new ArgumentToken { Type = ArgumentType.Plugin, Value = resolvedId }];
					target.PluginArguments = new Dictionary<string, CommandArgument[]>
					{
						[resolvedId] = [.. pluginArgs.Select(ArgParser.ParsePluginArg)]
					};
					target.PowerShellArguments = [];
				}
				else if (psScript != null)
				{
					target.Actions = [new ArgumentToken { Type = ArgumentType.PowerShell, Value = psScript }];
					target.PluginArguments = [];
					target.PowerShellArguments = [];
				}
				else if (actions.Length > 0)
				{
					target.Actions = [.. actions.Select(ArgParser.ParseAction)];
					target.PluginArguments = ArgParser.GroupArgSpecs(pluginArgs);
					target.PowerShellArguments = ArgParser.GroupArgSpecs(psArgs, powerShell: true);
				}
				else if (pluginArgs.Length > 0 || psArgs.Length > 0)
				{
					Console.Error.WriteLine("--plugin-arg and --ps-arg require --plugin, --powershell, or --action");
					return 1;
				}
			}
			catch (ArgumentException ex) { Console.Error.WriteLine(ex.Message); return 1; }

			CommandStore.SaveFile(file, commands);
			Console.WriteLine($"Updated '{target.Name}'");
			return 0;
		});

		return command;
	}
}