using Auto.Native.Models;

namespace Auto.Native;

public interface INativeMethods
{
	KeyScanResult KeyScan(char ch);
	void SendKeyboardInput(KeyboardInput[] keyboardInputs);
	bool IsKeyPressed(int vk);
	nint GetCurrentProcessHandle();
	nint SetKeyboardHook(NativeMethods.LowLevelKeyboardProc lpfn, nint handle);
	nint CallNextHook(nint hookId, int nCode, nint wParam, KeyboardInput lParam);
}