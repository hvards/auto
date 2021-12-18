using System.Text.RegularExpressions;
using Auto.Handlers;
using Auto.Tasks;

namespace Auto;

public class Command
{
    public HashSet<ushort> KeyCombo { get; set; }
    public string Keyword { get; set; }
    public ushort[] Macro { get; set; }
    public string[] Args { get; set; }
    public Dictionary<string, List<string>> ScriptArguments { get; set; }
    private ushort _macroPosition;

    public bool TestMacro(int key)
    {
        if (_macroPosition >= Macro?.Length || key != Macro?[_macroPosition++])
        {
            _macroPosition = 0;
            return false;
        }
        
        if (_macroPosition == Macro.Length)
            _macroPosition = 0;
        return _macroPosition == 0;
    }

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
        var psResult = Regex.Match(argument, "{:powerShell:([^}]*)}");
        if (!psResult.Success) return argument;
        ScriptArguments.TryGetValue(psResult.Groups[1].Value, out var scriptArgs);
        return argument.Replace(argument.Substring(psResult.Index, psResult.Length),
            PowerShell.Execute(psResult.Groups[1].Value, scriptArgs?.Select(GetPowerShellArguments) ?? Enumerable.Empty<string>()));
    }
}