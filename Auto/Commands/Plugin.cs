namespace Auto.Commands;

internal class Plugin
{
	public required Guid Id { get; init; }
	public required Func<object?[], object?> Action { get; init; }
	public bool StaThreadRequired { get; init; }
	public required Type[] ArgumentTypes { get; init; }
}
