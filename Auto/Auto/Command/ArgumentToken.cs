namespace Auto.Command;

public class ArgumentToken
{
    public ArgumentType Type { get; set; }
    public string Value { get; set; }
}

public enum ArgumentType
{
    NotSet,
    Text,
    PowerShell,
    Clipboard,
    Highlighted,
    Plugin
}