using Auto.Cli.Services;
using Auto.Models;
using Auto.PluginUtils;
using System.CommandLine;
using System.IO;

using CliCommand = System.CommandLine.Command;

namespace Auto.Cli.Commands;

public static class AddCommand
{
	private record AddInput(
		string ConfigDir,
		string Name,
		string File,
		string? Description,
		bool Disabled,
		string[] Combination,
		string[] Sequence,
		string[] Plugins,
		string[] PowerShellScripts,
		string[] PluginArguments,
		string[] PowerShellArguments
	);

	public static CliCommand Create(Option<string> configDirOption)
	{
		var command = new CliCommand("add") { Description = "Create a new command" }
			.AddArgument<string>("name", "Command name", out var nameArg)
			.AddOption("--file", "Target JSON file (relative to commands dir, default: default.json)",
				out var fileOption, defaultValue: "default.json")
			.AddOption<string[]>("--combination", "Key combination (e.g. LCtrl LWin LAlt R)", out var combinationOption)
			.AddOption<string[]>("--sequence", "Key sequence (e.g. E X A M P L E)", out var sequenceOption)
			.AddOption<string>("--description", "Command description", out var descOption)
			.AddOption<bool>("--disabled", "Create as disabled", out var disabledOption)
			.AddOption<string[]>("--plugin", "Plugin name or GUID", out var pluginOption)
			.AddOption<string[]>("--powershell", "PowerShell script to run", out var psScriptOption)
			.AddOption<string[]>("--arg", "Plugin argument (--arg <plugin> <values...>)",
				out var argOption)
			.AddOption<string[]>("--ps-arg", "PowerShell argument (--ps-arg <script> <param value>...)",
				out var psArgOption);

		command.SetActionWithErrorHandling(pr => Execute(
			new AddInput(
				pr.GetValue(configDirOption) ?? string.Empty,
				pr.GetValue(nameArg) ?? string.Empty,
				pr.GetValue(fileOption) ?? string.Empty,
				pr.GetValue(descOption),
				pr.GetValue(disabledOption),
				pr.GetValue(combinationOption) ?? [],
				pr.GetValue(sequenceOption) ?? [],
				pr.GetValue(pluginOption) ?? [],
				pr.GetValue(psScriptOption) ?? [],
				pr.GetValue(argOption) ?? [],
				pr.GetValue(psArgOption) ?? []
			)
		));

		return command;
	}

	private static void Execute(AddInput input)
	{
		var store = new CommandStore(input.ConfigDir);
		var path = store.ResolvePath(input.File);

		var trigger = KeyNameResolver.ParseTrigger(input.Combination, input.Sequence);

		var cmd = new CommandEntry
		{
			Id = Guid.NewGuid(),
			Name = input.Name,
			Description = input.Description ?? string.Empty,
			Enabled = !input.Disabled,
			Trigger = trigger,
			Actions = [],
			PowerShellArguments = [],
			PluginArguments = []
		};

		var actionList = new List<ArgumentToken>();

		foreach (var plugin in input.Plugins)
		{
			var id = PluginLoader.ResolvePlugin(plugin);
			actionList.Add(new ArgumentToken { Type = ArgumentType.Plugin, Value = id });
		}
		foreach (var ps in input.PowerShellScripts)
		{
			actionList.Add(new ArgumentToken { Type = ArgumentType.PowerShell, Value = ps });
		}
		cmd.Actions = [.. actionList];

		cmd.PluginArguments = PluginHelper.GroupPluginArgs(input.PluginArguments);
		cmd.PowerShellArguments = PluginHelper.GroupPsArgs(input.PowerShellArguments);

		var duplicate = store.LoadAll().FirstOrDefault(x =>
			string.Equals(x.Command.Name, input.Name, StringComparison.OrdinalIgnoreCase));
		if (duplicate.Command != null)
			Console.Error.WriteLine($"Warning: '{duplicate.Command.Name}' already exists (id: {duplicate.Command.Id})");

		var existing = File.Exists(path) ? CommandStore.LoadFile(path) : [];
		existing.Add(cmd);
		CommandStore.SaveFile(path, existing);

		Console.WriteLine($"Added '{input.Name}' to {input.File} (id: {cmd.Id})");
	}
}
