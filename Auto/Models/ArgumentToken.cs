namespace Auto.Models;

internal class ArgumentToken
{
	public ArgumentType Type { get; set; }
	public string Value { get; set; } = string.Empty;
}

internal enum ArgumentType
{
	Text,
	Variable
}