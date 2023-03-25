using System.Runtime.InteropServices;
using static Auto.Constants;

namespace Auto.Handlers;
public static class KeyboardHandler
{
    private static readonly HashSet<string> LShiftKeys = new() { "§", "!", "\"", "#", "¤", "%", "&", "/", "(", ")", "=", "?", "`", "^", "*", "_", ":", ";" };
    private static readonly HashSet<string> AltGrKeys = new() { "@", "£", "$", "€", "{", "[", "]", "}", "´", "~", "€" };

    private static readonly Dictionary<string, ushort> KeyMap = new()
    {
        { "enter", (ushort)Keys.Enter },
        { "left", (ushort)Keys.Left },
        { "up", (ushort)Keys.Up },
        { "right", (ushort)Keys.Right },
        { "down", (ushort)Keys.Down },
        { "LCtrl", (ushort)Keys.LControlKey },
        { "tab", (ushort)Keys.Tab },
        { "Esc", (ushort)Keys.Escape },
        { "Menu", (ushort)Keys.Apps}
    };

    public static void ReleaseAllKeys()
    {
        foreach (int key in Enum.GetValues(typeof(Keys)))
        {
            var state = GetAsyncKeyState(key);
            if ((state & 0x8000) != 0)
                ClickKey((ushort)key, WM_KEYUP);
        }
    }

    public static void ReleaseKeys(IEnumerable<ushort> keys)
    {
        foreach (var key in keys)
        {
            ClickKey(key, WM_KEYUP);
        }
    }

    public static void SendChar(string ch, nint? action = null)
    {
        if (LShiftKeys.Contains(ch))
            SendWithLShift((ushort)VkKeyScan(ch[0]));
        else if (AltGrKeys.Contains(ch))
            SendWithAltGr((ushort)VkKeyScan(ch[0]));
        else if (KeyMap.TryGetValue(ch, out var val))
            ClickKey(val, action);
        else if (char.IsUpper(ch[0]))
            SendWithLShift((ushort)VkKeyScan(ch[0]));
        else
            ClickKey((ushort)VkKeyScan(ch[0]), action);
    }

    public static void ClickKey(ushort vk, nint? action) => SendKeyboardInput(GetKeyboardInputArr(vk, action: action));

    public static void CopyHighlightedText() => SendWithLCtrl(0x43);

    private static void SendWithAltGr(ushort vk) => SendKeyboardInput(new[]{
            GetKeyboardInput(162, true), GetKeyboardInput(165, true), GetKeyboardInput(vk, true),
            GetKeyboardInput(vk, false), GetKeyboardInput(165, false), GetKeyboardInput(162, false)
        });

    private static void SendWithLShift(ushort vk) => SendKeyboardInput(GetKeyboardInputArr(vk, 16));

    private static void SendWithLCtrl(ushort vk) => SendKeyboardInput(GetKeyboardInputArr(vk, 0xA2));

    private static void SendKeyboardInput(Input[] kbInputs) => SendInput((uint)kbInputs.Length, kbInputs, Marshal.SizeOf(typeof(Input)));
    private static Input[] GetKeyboardInputArr(ushort vk, ushort modifier = 0, nint? action = null) => action == null ? modifier == 0
            ? new[] { GetKeyboardInput(vk, true), GetKeyboardInput(vk, false) }
            : new[] { GetKeyboardInput(modifier, true), GetKeyboardInput(vk, true), GetKeyboardInput(vk, false), GetKeyboardInput(modifier, false) }
            : new[] { GetKeyboardInput(vk, (int)action == (int)WM_KEYDOWN) };

    private static Input GetKeyboardInput(ushort vk, bool down) => new()
    {
        type = (int)InputType.Keyboard,
        u = new InputUnion
        {
            ki = new KeyboardInput
            {
                wVk = vk,
                dwFlags = (ushort)(down ? KeyEventF.KeyDown : KeyEventF.KeyUp),
                dwExtraInfo = IGNORE_INPUT
            }
        }
    };

    [DllImport("user32.dll")]
    private static extern short VkKeyScan(char ch);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint SendInput(uint nInputs, Input[] pInputs, int cbSize);

    [DllImport("user32.dll")]
    static extern short GetAsyncKeyState(int lpKeyState);
}
