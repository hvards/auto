using System.Runtime.InteropServices;
using Auto.Interfaces;
using static Auto.Constants;

namespace Auto.Handlers;
public class KeyboardHandler : IKeyboardHandler
{
    public void ReleaseAllKeys()
    {
        foreach (int key in Enum.GetValues(typeof(Keys)))
        {
            var state = GetAsyncKeyState(key);
            if ((state & 0x8000) != 0)
                ClickKey((ushort)key, WM_KEYUP);
        }
    }

    public void SendChar(string ch, nint? action = null)
    {
	    if (ch.Length > 1 && Enum.TryParse(typeof(Keys), ch, out var value))
	    {
		    var key = (Keys)value;
		    ClickKey((ushort)key, action);
			return;
	    }

	    var scanResult = VkKeyScan(ch[0]);
	    var vk = (ushort)(scanResult & 0xff);
	    var modifier = scanResult >> 8;

	    switch (modifier)
	    {
		    case 6:
			    SendWithAltGr(vk);
			    break;
		    case 2:
			    SendWithLCtrl(vk);
			    break;
		    case 1:
			    SendWithLShift(vk);
			    break;
		    default:
			    ClickKey(vk, action);
			    break;
	    }
    }

    public void ClickKey(ushort vk, nint? action) => SendKeyboardInput(GetKeyboardInputArr(vk, action: action));

    public void CopyHighlightedText() => SendWithLCtrl(0x43);

    private static void SendWithAltGr(ushort vk) => SendKeyboardInput([
	    GetKeyboardInput(162, true), GetKeyboardInput(165, true), GetKeyboardInput(vk, true),
		GetKeyboardInput(vk, false), GetKeyboardInput(165, false), GetKeyboardInput(162, false)
    ]);

    private static void SendWithLShift(ushort vk) => SendKeyboardInput(GetKeyboardInputArr(vk, 16));

    private static void SendWithLCtrl(ushort vk) => SendKeyboardInput(GetKeyboardInputArr(vk, 0xA2));

    private static void SendKeyboardInput(Input[] kbInputs) => _ = SendInput((uint)kbInputs.Length, kbInputs, Marshal.SizeOf(typeof(Input)));
    private static Input[] GetKeyboardInputArr(ushort vk, ushort modifier = 0, nint? action = null) => action == null ? modifier == 0
            ? [GetKeyboardInput(vk, true), GetKeyboardInput(vk, false)]
            : [GetKeyboardInput(modifier, true), GetKeyboardInput(vk, true), GetKeyboardInput(vk, false), GetKeyboardInput(modifier, false)]
			: [GetKeyboardInput(vk, (int)action == (int)WM_KEYDOWN)];

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
    private static extern short GetAsyncKeyState(int lpKeyState);
}
