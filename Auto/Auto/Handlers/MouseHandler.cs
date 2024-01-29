using System.Runtime.InteropServices;
using static Auto.Constants;

namespace Auto.Handlers;

public static class MouseHandler
{
    public static void LeftClick()
    {
        var input = new [] {GetMouseInput(true), GetMouseInput(false)};
        _ = SendInput((uint) input.Length, input, Marshal.SizeOf(typeof(Input)));
    }

    private static Input GetMouseInput(bool down) => new()
    {
        type = (int)InputType.Mouse,
        u = new InputUnion
        {
            mi = new MouseInput
            {
                dwFlags = down ? MOUSEEVENTF_LEFTDOWN : MOUSEEVENTF_LEFTUP
            }
        }
    };

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint SendInput(uint nInputs, Input[] pInputs, int cbSize);
}