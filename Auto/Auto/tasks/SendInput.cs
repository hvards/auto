using Auto.Handlers;
using Auto.InputUtils;
using static Auto.Constants;

namespace Auto.tasks;


public static class SendInput
{
    public static void Mouse(string input)
    {
        foreach (var _ in input.GetTokens())
        {
            MouseHandler.LeftClick();
        }
    }

    public static void Keyboard(string input)
    {        
        foreach (var token in input.GetTokens())
        {
            switch (token.InputAction)
            {
                case InputAction.NotSet:
                    KeyboardHandler.SendChar(token.Value);
                    break;
                case InputAction.Down:
                    KeyboardHandler.SendChar(token.Value, WM_KEYDOWN);
                    break;
                case InputAction.Up:
                    KeyboardHandler.SendChar(token.Value, WM_KEYUP);
                    break;
                case InputAction.Sleep:
                    Thread.Sleep(int.Parse(token.Value));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}