using Auto.tasks;

namespace Auto.Command;

public class Command
{
	public Trigger Trigger { get; set; }
	public string Action { get; set; }
	public CommandArgument[] Arguments { get; set; }
	public Dictionary<string, CommandArgument[]> PowerShellArguments { get; set; }
	public Dictionary<string, CommandArgument[]> PluginArguments { get; set; }
	public bool Enabled { get; set; }
	public bool HighlightedTextRequired { get; set; }
	public bool ConcurrentExecution { get; set; }
	public bool ClipboardTextRequired { get; set; }
	private readonly Dictionary<string, string> _powerShellExecutionResult = [];

	public List<string> ExecuteArguments(string clipboard = null, string highlighted = null)
	{
		var res = Arguments.Select(arg => ExecuteArgument(arg, clipboard, highlighted)).ToList();
		_powerShellExecutionResult.Clear();
		return res;
	}

	private string ExecuteArgument(CommandArgument argument, string clipboard, string highlighted)
	{
		return argument.Tokens.Aggregate(string.Empty,
			(current, next) =>
			{
				current += ExecuteArgumentToken(next, clipboard, highlighted);
				return current;
			});
	}

	private string ExecuteArgumentToken(ArgumentToken token, string clipboard, string highlighted)
	{
		switch (token.Type)
		{
			case ArgumentType.Clipboard:
				return clipboard;
			case ArgumentType.Highlighted:
				return highlighted;
			case ArgumentType.PowerShell:
				if (_powerShellExecutionResult.TryGetValue(token.Value, out var result))
					return result;
				PowerShellArguments.TryGetValue(token.Value, out var scriptArgs);
				result = PowerShell.Execute(token.Value,
					scriptArgs?.Select(x => (x.ParameterName, ExecuteArgument(x, clipboard, highlighted))).ToList());
				_powerShellExecutionResult.Add(token.Value, result);
				return result;
			case ArgumentType.Plugin:
				var pluginArguments = GetPluginArgumentValues(PluginArguments[token.Value], clipboard, highlighted);
				return tasks.Plugin.ExecutePlugin(token.Value, pluginArguments)?.ToString() ?? string.Empty;
			case ArgumentType.Text:
				return token.Value;
			case ArgumentType.NotSet:
			default:
				return null;
		}
	}

	private IEnumerable<object> GetPluginArgumentValues(CommandArgument[] arguments, string clipboard,
		string highlighted)
	{
		foreach (var argument in arguments)
		{
			if (argument.Tokens.Length == 1 && argument.Tokens[0].Type == ArgumentType.Plugin)
			{
				var args = GetPluginArgumentValues(PluginArguments[argument.Tokens[0].Value], clipboard,
					highlighted);
				yield return tasks.Plugin.ExecutePlugin(argument.Tokens[0].Value, args);
			}
			else
			{
				yield return ExecuteArgument(argument, clipboard, highlighted);
			}
		}
	}
}