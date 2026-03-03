namespace Auto.Command;

public class ArgumentToken
{
	public ArgumentType Type { get; init; }
	public string Value { get; init; }
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