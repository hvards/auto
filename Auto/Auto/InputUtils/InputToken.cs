namespace Auto.InputUtils;

public class InputToken
{
    public string Value { get; set; }
    public InputAction InputAction { get; set; }
}

public enum InputAction
{
    NotSet = 0,
    Down = 1,
    Up = 2,
    Sleep = 3
}