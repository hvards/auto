using Auto.Native;
using static Auto.Native.Constants;

namespace Auto.Handlers;

public interface IKeyboardHandler
{
    void ReleaseAllKeys();
    void SendChar(string ch, nint? action = null);
    void ClickKey(ushort vk, nint? action);
    void CopyHighlightedText();
}

public class KeyboardHandler(INativeMethods nativeMethods) : IKeyboardHandler
{
    public void ReleaseAllKeys()
    {
        foreach (int key in Enum.GetValues<Keys>())
        {
	        if (nativeMethods.IsKeyPressed(key))
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

	    var keyScan = nativeMethods.KeyScan(ch[0]);
	    switch (keyScan.Modifier)
	    {
		    case 6:
			    SendWithAltGr(keyScan.VirtualKey);
			    break;
		    case 2:
			    SendWithLCtrl(keyScan.VirtualKey);
			    break;
		    case 1:
			    SendWithLShift(keyScan.VirtualKey);
			    break;
		    default:
			    ClickKey(keyScan.VirtualKey, action);
			    break;
	    }
    }

    public void ClickKey(ushort vk, nint? action) =>
	    nativeMethods.SendKeyboardInput(GetKeyboardInputArr(vk, action: action));

    public void CopyHighlightedText() => SendWithLCtrl(0x43);

    private void SendWithAltGr(ushort vk) => nativeMethods.SendKeyboardInput([
	    GetKeyboardInput(162, true), GetKeyboardInput(165, true), GetKeyboardInput(vk, true),
	    GetKeyboardInput(vk, false), GetKeyboardInput(165, false), GetKeyboardInput(162, false)
    ]);

    private void SendWithLShift(ushort vk) => nativeMethods.SendKeyboardInput(GetKeyboardInputArr(vk, 16));

    private void SendWithLCtrl(ushort vk) => nativeMethods.SendKeyboardInput(GetKeyboardInputArr(vk, 0xA2));

    private static KeyboardInput[] GetKeyboardInputArr(ushort vk, ushort modifier = 0, nint? action = null) =>
	    action == null
		    ? modifier == 0
			    ? [GetKeyboardInput(vk, true), GetKeyboardInput(vk, false)]
			    :
			    [
				    GetKeyboardInput(modifier, true), GetKeyboardInput(vk, true),
				    GetKeyboardInput(vk, false), GetKeyboardInput(modifier, false)
			    ]
		    : [GetKeyboardInput(vk, (int)action == (int)WM_KEYDOWN)];

    private static KeyboardInput GetKeyboardInput(ushort vk, bool down) => new()
    {
	    wVk = vk,
	    dwFlags = (ushort)(down ? KeyEventF.KeyDown : KeyEventF.KeyUp),
	    dwExtraInfo = IGNORE_INPUT
    };
}
