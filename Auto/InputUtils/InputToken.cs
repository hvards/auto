namespace Auto.InputUtils;

internal class InputToken
{
	public required string Value { get; init; }
	public InputAction InputAction { get; init; }
}

internal enum InputAction
{
	NotSet = 0,
	Down = 1,
	Up = 2,
	Sleep = 3
}