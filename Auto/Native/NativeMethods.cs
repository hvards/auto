using System.Diagnostics;
using System.Runtime.InteropServices;

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

public partial class NativeMethods : INativeMethods
{
	private LowLevelKeyboardProc _keyboardHook;

	public KeyScanResult KeyScan(char ch)
	{
		var scanResult = VkKeyScanW(ch);

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
		return GetModuleHandleW(module!.ModuleName);
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
		return SetWindowsHookExW(Constants.WH_KEYBOARD_LL, lpfn, handle, 0);
	}

	public nint CallNextHook(nint hookId, int nCode, nint wParam, KeyboardInput lParam)
	{
		return CallNextHookEx(hookId, nCode, wParam, ref lParam);
	}

	public delegate nint LowLevelKeyboardProc(int nCode, nint wParam, ref KeyboardInput lParam);

	[LibraryImport("user32.dll")]
	private static partial short VkKeyScanW(ushort ch);

	[LibraryImport("user32.dll", SetLastError = true)]
	private static partial uint SendInput(uint nInputs, Input[] pInputs, int cbSize);

	[LibraryImport("user32.dll")]
	private static partial short GetAsyncKeyState(int vKey);

	[LibraryImport("user32.dll", SetLastError = true)]
	private static partial nint SetWindowsHookExW(int idHook, LowLevelKeyboardProc lpfn, nint hMod, uint dwThreadId);

	[LibraryImport("user32.dll", SetLastError = true)]
	private static partial nint CallNextHookEx(nint hhk, int nCode, nint wParam, ref KeyboardInput lParam);

	[LibraryImport("kernel32.dll", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
	private static partial nint GetModuleHandleW(string lpModuleName);
}