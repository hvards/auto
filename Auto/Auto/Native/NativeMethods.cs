using System.Diagnostics;
using System.Runtime.InteropServices;
using Auto.Native.Models;
using Microsoft.Extensions.Logging;

namespace Auto.Native;

public class NativeMethods : INativeMethods
{
	private LowLevelKeyboardProc _keyboardHook;

	public KeyScanResult KeyScan(char ch)
	{
		var scanResult = VkKeyScan(ch);

	    var vk = (ushort)(scanResult & 0xff);
	    var modifier = scanResult >> 8;

		return new KeyScanResult
		{
			Modifier = modifier,
			VirtualKey = vk
		};
	}

	public nint GetCurrentProcessHandle()
	{
        using var currentProcess = Process.GetCurrentProcess();
        using var module = currentProcess.MainModule;
        return GetModuleHandle(module!.ModuleName);
	}

	public void SendKeyboardInput(KeyboardInput[] keyboardInputs)
	{
		var input = keyboardInputs.Select(x => new Input
		{
			type = (int)InputType.Keyboard,
			u = new InputUnion
			{
				ki = x
			}
		}).ToArray();

		 _ = SendInput((uint)input.Length, input, Marshal.SizeOf(typeof(Input)));
	}

	public bool IsKeyPressed(int vk)
	{
		var state = GetAsyncKeyState(vk);
		return (state & 0x8000) != 0;
	}

	public nint SetKeyboardHook(LowLevelKeyboardProc lpfn, nint handle)
	{
		if (_keyboardHook != null)
			throw new Exception("Multiple keyboard hooks not supported");

		_keyboardHook = lpfn; // Assign private field to avoid garbage collection
		return SetWindowsHookEx(Constants.WH_KEYBOARD_LL, lpfn, handle, 0);
	}

	public nint CallNextHook(nint hookId, int nCode, nint wParam, KeyboardInput lParam)
	{
		return CallNextHookEx(hookId, nCode, wParam, ref lParam);
	}

	public void RemoveKeyboardHook(nint hookId)
	{
		_keyboardHook = null;
		UnhookWindowsHookEx(hookId);
	}

    public delegate nint LowLevelKeyboardProc(int nCode, nint wParam, ref KeyboardInput lParam);

    [DllImport("user32.dll")]
    private static extern short VkKeyScan(char ch);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint SendInput(uint nInputs, Input[] pInputs, int cbSize);

    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern nint SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, nint hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(nint hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern nint CallNextHookEx(nint hhk, int nCode, nint wParam, ref KeyboardInput lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern nint GetModuleHandle(string lpModuleName);
}