using Auto.Native;

using static Auto.Native.Constants;

namespace Auto.Handlers;

internal interface IKeyboardHandler
{
	void ReleaseAllKeys();
	void SendChar(string ch, nint? action = null);
	void ClickKey(ushort vk, nint? action);
	void CopyHighlightedText();
}

internal class KeyboardHandler(INativeMethods nativeMethods) : IKeyboardHandler
{
	private const ushort VkLControl = (ushort)Keys.LControlKey;
	private const ushort VkRMenu = (ushort)Keys.RMenu;
	private const ushort VkShift = (ushort)Keys.ShiftKey;

	private const int ModifierShift = 1;
	private const int ModifierCtrl = 2;
	private const int ModifierAltGr = 6;

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
			case ModifierAltGr:
				SendWithAltGr(keyScan.VirtualKey);
				break;
			case ModifierCtrl:
				SendWithLCtrl(keyScan.VirtualKey);
				break;
			case ModifierShift:
				SendWithLShift(keyScan.VirtualKey);
				break;
			default:
				ClickKey(keyScan.VirtualKey, action);
				break;
		}
	}

	public void ClickKey(ushort vk, nint? action) =>
		nativeMethods.SendKeyboardInput(GetKeyboardInputArr(vk, action: action));

	public void CopyHighlightedText() => SendWithLCtrl((ushort)Keys.C);

	private void SendWithAltGr(ushort vk) => nativeMethods.SendKeyboardInput([
		GetKeyboardInput(VkLControl, true), GetKeyboardInput(VkRMenu, true), GetKeyboardInput(vk, true),
		GetKeyboardInput(vk, false), GetKeyboardInput(VkRMenu, false), GetKeyboardInput(VkLControl, false)
	]);

	private void SendWithLShift(ushort vk) => nativeMethods.SendKeyboardInput(GetKeyboardInputArr(vk, VkShift));

	private void SendWithLCtrl(ushort vk) => nativeMethods.SendKeyboardInput(GetKeyboardInputArr(vk, VkLControl));

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
