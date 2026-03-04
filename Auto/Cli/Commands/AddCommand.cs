using Auto.Cli.Services;
using Auto.Models;
using Auto.PluginUtils;
using System.CommandLine;
using System.IO;
using CliCommand = System.CommandLine.Command;

namespace Auto.Cli.Commands;

public static class AddCommand
{
	public static CliCommand Create(Option<string> configDirOption)
	{
		var command = new CliCommand("add") { Description = "Create a new command" }
			.AddArgument<string>("name", "Command name", out var nameArg)
			.AddOption<string>("--file", "Target JSON file (relative to commands dir, default: default.json)",
				out var fileOption, defaultValue: "default.json")
			.AddOption<string>("--combination", "Key combination (e.g. LCtrl+LWin+LAlt+R)", out var combinationOption)
			.AddOption<string>("--sequence", "Key sequence (e.g. E,X,A,M,P,L,E)", out var sequenceOption)
			.AddOption<string>("--description", "Command description", out var descOption)
			.AddOption<bool>("--disabled", "Create as disabled", out var disabledOption)
			.AddOption<string[]>("--action", "Action (repeatable, e.g. plugin:guid, ps:script.ps1)",
				out var actionOption)
			.AddOption<string>("--plugin", "Plugin name or GUID (e.g. StartProgram, KeyboardInput)",
				out var pluginOption)
			.AddOption<string>("--powershell", "PowerShell script to run", out var psScriptOption)
			.AddOption<string[]>("--ps-arg", "PowerShell argument (script:paramName=value)", out var psArgOption)
			.AddOption<string[]>(
				"--plugin-arg", "Plugin argument (use with --plugin, or guid:value with --action)",
				out var pluginArgOption);

		command.SetAction(parseResult =>
		{
			var configDir = parseResult.GetValue(configDirOption);
			var name = parseResult.GetValue(nameArg);
			var file = parseResult.GetValue(fileOption);
			var combination = parseResult.GetValue(combinationOption);
			var sequence = parseResult.GetValue(sequenceOption);
			var desc = parseResult.GetValue(descOption);
			var disabled = parseResult.GetValue(disabledOption);
			var plugin = parseResult.GetValue(pluginOption);
			var psScript = parseResult.GetValue(psScriptOption);
			var actions = parseResult.GetValue(actionOption) ?? [];
			var psArgs = parseResult.GetValue(psArgOption) ?? [];
			var pluginArgs = parseResult.GetValue(pluginArgOption) ?? [];

			var store = new CommandStore(configDir);
			var path = store.ResolvePath(file);

			if (!KeyNameResolver.ParseTrigger(combination, sequence, out var parsedTrigger))
				return 1;

			var cmd = new CommandEntry
			{
				Id = Guid.NewGuid(),
				Name = name,
				Description = desc ?? string.Empty,
				Enabled = !disabled,
				Trigger = parsedTrigger,
				Actions = [],
				PowerShellArguments = [],
				PluginArguments = []
			};

			var actionList = new List<ArgumentToken>();
			Dictionary<string, CommandArgument[]> pluginArgGroups = [];

			if (plugin != null)
			{
				var resolvedId = PluginLoader.ResolvePlugin(plugin);
				if (resolvedId == null)
				{
					Console.Error.WriteLine($"Unknown plugin: {plugin}");
					return 1;
				}

				actionList.Add(new ArgumentToken { Type = ArgumentType.Plugin, Value = resolvedId });
				pluginArgGroups = new Dictionary<string, CommandArgument[]>
				{
					[resolvedId] = [.. pluginArgs.Select(ArgParser.ParsePluginArg)]
				};
				pluginArgs = [];
			}

			if (psScript != null)
			{
				actionList.Add(new ArgumentToken { Type = ArgumentType.PowerShell, Value = psScript });
			}

			try
			{
				foreach (var a in actions)
					actionList.Add(ArgParser.ParseAction(a));

				if (pluginArgGroups.Count == 0)
					pluginArgGroups = ArgParser.GroupArgSpecs(pluginArgs);
			}
			catch (ArgumentException ex) { Console.Error.WriteLine(ex.Message); return 1; }

			cmd.Actions = [.. actionList];
			cmd.PowerShellArguments = ArgParser.GroupArgSpecs(psArgs, powerShell: true);
			cmd.PluginArguments = pluginArgGroups;

			var duplicate = store.LoadAll().FirstOrDefault(x =>
				string.Equals(x.Command.Name, name, StringComparison.OrdinalIgnoreCase));
			if (duplicate.Command != null)
				Console.Error.WriteLine($"Warning: '{duplicate.Command.Name}' already exists (id: {duplicate.Command.Id})");

			var existing = File.Exists(path) ? CommandStore.LoadFile(path) : [];
			existing.Add(cmd);
			CommandStore.SaveFile(path, existing);

			Console.WriteLine($"Added '{name}' to {file} (id: {cmd.Id})");
			return 0;
		});

		return command;
	}
}