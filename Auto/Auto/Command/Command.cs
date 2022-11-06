using System.Text.RegularExpressions;
using Auto.Handlers;
using Auto.Tasks;

namespace Auto.Command;

public class Command
{
    public Trigger Trigger { get; set; }
    public string Action { get; set; }
    public string[] Args { get; set; }
    public Dictionary<string, List<string>> ScriptArguments { get; set; }
    public bool Enabled { get; set; }

    public List<string> ExecuteArguments() => Args.Select(arg =>
    {
        if (arg.Contains("{:highlighted}"))
            arg = arg.Replace("{:highlighted}", ClipboardHandler.GetClipboardText(true));
        if (arg.Contains("{:clipboard}"))
            arg = arg.Replace("{:clipboard}", ClipboardHandler.GetClipboardText());
        return GetPowerShellArguments(arg);
    }).ToList();

    private string GetPowerShellArguments(string argument)
    {
        var psResult = Regex.Match(argument, "{:powershell:([^}]*)}");
        if (!psResult.Success) return argument;
        ScriptArguments.TryGetValue(psResult.Groups[1].Value, out var scriptArgs);
        return argument.Replace(argument.Substring(psResult.Index, psResult.Length),
            PowerShell.Execute(psResult.Groups[1].Value, scriptArgs?.Select(GetPowerShellArguments) ?? Enumerable.Empty<string>()));
    }
}