using Auto.Models;
using Auto.PluginUtils;
using Auto.Tasks;

namespace Auto.Commands;

internal interface ICommandExecutor
{
	List<string?> ExecuteCommand(Command command, string? clipboard = null, string? highlighted = null);
}

internal class CommandExecutor(IPluginExecutor pluginExecutor, IPowerShell powerShell) : ICommandExecutor
{
	public List<string?> ExecuteCommand(Command command, string? clipboard = null, string? highlighted = null)
	{
		var variables = new Dictionary<string, object?>()
		{
			["Clipboard"] = clipboard,
			["Highlighted"] = highlighted
		};
		var results = new List<string?>();

		foreach (var action in command.Actions.OrderBy(a => a.Order))
		{
			var result = action.Type switch
			{
				ActionType.Plugin => ExecutePlugin(action, variables),
				ActionType.PowerShell => ExecutePowerShell(action, variables),
				_ => null
			};

			if (action.Variable != null)
				variables[action.Variable] = result;

			results.Add(result?.ToString());
		}

		return results;
	}

	private object? ExecutePlugin(CommandAction action, Dictionary<string, object?> variables)
	{
		var args = ResolveArguments(action.Arguments, variables);
		return pluginExecutor.ExecutePlugin(action.Target, args);
	}

	private string? ExecutePowerShell(CommandAction action, Dictionary<string, object?> variables)
	{
		var parameters = action.Arguments
			.Select(a => (a.ParameterName, ResolveArgument(a, variables)?.ToString() ?? string.Empty))
			.ToList();
		return powerShell.Execute(action.Target, parameters);
	}

	private static IEnumerable<object?> ResolveArguments(CommandArgument[] arguments, Dictionary<string, object?> variables)
	{
		foreach (var arg in arguments)
		{
			if (arg.Tokens.Length == 1 && arg.Tokens[0].Type == ArgumentType.Variable)
			{
				variables.TryGetValue(arg.Tokens[0].Value, out var raw);
				yield return raw;
			}
			else
			{
				yield return ResolveArgument(arg, variables);
			}
		}
	}

	private static object? ResolveArgument(CommandArgument argument, Dictionary<string, object?> variables)
	{
		if (argument.Tokens.Length == 1 && argument.Tokens[0].Type == ArgumentType.Variable)
			return variables[argument.Tokens[0].Value];

		return string.Concat(argument.Tokens.Select(t => t.Type switch
		{
			ArgumentType.Variable => variables.TryGetValue(t.Value, out var v) ? v?.ToString() ?? "" : "",
			ArgumentType.Text => t.Value,
			_ => ""
		}));
	}
}