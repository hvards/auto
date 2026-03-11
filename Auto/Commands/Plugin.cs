namespace Auto.Commands;

public class Plugin
{
	public required Func<object?[], object?> Action { get; init; }
	public bool StaThreadRequired { get; init; }
	public required Type[] ArgumentTypes { get; init; }
}