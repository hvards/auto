namespace Auto.Commands;

public class Plugin
{
	public Func<object[], object> Action { get; init; }
	public bool StaThreadRequired { get; init; }
	public Type[] ArgumentTypes { get; init; }
}