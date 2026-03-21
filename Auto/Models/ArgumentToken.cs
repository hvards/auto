namespace Auto.Models;

public class ArgumentToken
{
	public ArgumentType Type { get; set; }
	public string Value { get; set; } = string.Empty;
}

public enum ArgumentType
{
	Text,
	Variable
}
