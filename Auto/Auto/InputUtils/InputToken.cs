namespace Auto.InputUtils;

public class InputToken
{
    public string Value { get; init; }
    public InputAction InputAction { get; init; }
}

public enum InputAction
{
    NotSet = 0,
    Down = 1,
    Up = 2,
    Sleep = 3
}