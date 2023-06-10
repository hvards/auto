using Auto.Tasks;

namespace Auto.Command;

public class Command
{
    public Trigger Trigger { get; set; }
    public string Action { get; set; }
    public CommandArgument[] Arguments { get; set; }
    public Dictionary<string, CommandArgument[]> PowerShellArguments { get; set; }
    public bool Enabled { get; set; }
    public bool HighlightedTextRequired { get; set; }
    public bool ConcurrentExecution { get; set; }
    public bool ClipboardTextRequired { get; set; }
    private readonly Dictionary<string, string> _powerShellExecutionResult = new();

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
                PowerShellArguments.TryGetValue(token.Value, out var scriptArgs);
                if (_powerShellExecutionResult.TryGetValue(token.Value, out var result))
                    return result;
                result = PowerShell.Execute(token.Value,
                    scriptArgs?.Select(x => (x.ParameterName, ExecuteArgument(x, clipboard, highlighted))).ToList());
                _powerShellExecutionResult.Add(token.Value, result);
                return result;
            case ArgumentType.Text:
                return token.Value;
            case ArgumentType.NotSet:
            default:
                return null;
        }
    }
}