using Auto.Handlers;
using Auto.InputUtils;
using static Auto.Constants;

namespace Auto.Tasks;


public static class SendInput
{
    public static void Mouse(string input)
    {
        foreach (var token in input.GetTokens())
        {
            MouseHandler.LeftClick();
        }
    }

    public static void Keyboard(bool del, string input)
    {
        if (del)
        {
            KeyboardHandler.ClickKey((ushort)Keys.ShiftKey, WM_KEYDOWN);
            KeyboardHandler.ClickKey((ushort)Keys.ControlKey, WM_KEYDOWN);
            KeyboardHandler.ClickKey((ushort)Keys.Left, null);
            KeyboardHandler.ClickKey((ushort)Keys.ControlKey, WM_KEYUP);
            KeyboardHandler.ClickKey((ushort)Keys.ShiftKey, WM_KEYUP);
        }
        
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