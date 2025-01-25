using Auto.tasks;

namespace Auto.Command;

public interface ICommandExecutor
{
	List<string> ExecuteCommand(Command command, string clipboard = null, string highlighted = null);
}

public class CommandExecutor(IPluginExecutor pluginExecutor, IPowerShell powerShell) : ICommandExecutor
{
	public List<string> ExecuteCommand(Command command, string clipboard = null, string highlighted = null)
	{
		return ExecuteCommand(command.Actions, command.PowerShellArguments, command.PluginArguments,
			clipboard, highlighted);
	}

	private List<string> ExecuteCommand(
		ArgumentToken[] tokens,
		Dictionary<string, CommandArgument[]> powershellArguments,
		Dictionary<string, CommandArgument[]> pluginArguments,
		string clipboard = null, string highlighted = null)
	{
		var powerShellExecutionResult = new Dictionary<string, string>();
		var res = tokens.Select(ExecuteArgumentToken).ToList();

		return res;

		string ExecuteArgument(CommandArgument argument)
		{
			return argument.Tokens.Aggregate(string.Empty,
				(current, next) =>
				{
					current += ExecuteArgumentToken(next);
					return current;
				});
		}

		string ExecuteArgumentToken(ArgumentToken token)
		{
			switch (token.Type)
			{
				case ArgumentType.Clipboard:
					return clipboard;
				case ArgumentType.Highlighted:
					return highlighted;
				case ArgumentType.PowerShell:
					if (powerShellExecutionResult.TryGetValue(token.Value, out var result))
						return result;
					powershellArguments.TryGetValue(token.Value, out var scriptArgs);
					result = powerShell.Execute(token.Value,
						scriptArgs?.Select(x => (x.ParameterName, ExecuteArgument(x))).ToList());
					powerShellExecutionResult.Add(token.Value, result);
					return result;
				case ArgumentType.Plugin:
					var pluginArgs = GetPluginArgumentValues(pluginArguments[token.Value]);
					return pluginExecutor.ExecutePlugin(token.Value, pluginArgs)?.ToString() ?? string.Empty;
				case ArgumentType.Text:
					return token.Value;
				case ArgumentType.NotSet:
				default:
					return null;
			}
		}

		IEnumerable<object> GetPluginArgumentValues(CommandArgument[] arguments)
		{
			foreach (var argument in arguments)
			{
				if (argument.Tokens.Length == 1 && argument.Tokens[0].Type == ArgumentType.Plugin)
				{
					var args = GetPluginArgumentValues(pluginArguments[argument.Tokens[0].Value]);
					yield return pluginExecutor.ExecutePlugin(argument.Tokens[0].Value, args);
				}
				else
				{
					yield return ExecuteArgument(argument);
				}
			}
		}
	}
}