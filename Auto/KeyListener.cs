using Auto.Commands;
using Auto.Native;
using Auto.Tasks;

using static Auto.Native.Constants;

namespace Auto;

internal class KeyListener
{
	private readonly ICommandProvider _commandProvider;
	private readonly IExecute _execute;
	private readonly INativeMethods _nativeMethods;

	private static nint _hookId = nint.Zero;
	private static readonly HashSet<ushort> PressedKeys = [];

	public KeyListener(ICommandProvider commandProvider, IExecute execute, INativeMethods nativeMethods)
	{
		_commandProvider = commandProvider;
		_execute = execute;
		_nativeMethods = nativeMethods;

		Hook();
	}

	private void Hook()
	{
		var handle = _nativeMethods.GetCurrentProcessHandle();
		_hookId = _nativeMethods.SetKeyboardHook(KeyboardHookCallback, handle);
	}

	private nint KeyboardHookCallback(int nCode, nint wParam, ref KeyboardInput lParam)
	{
		var keyDown = wParam is WM_KEYDOWN or WM_SYSKEYDOWN;
		var keyUp = wParam is WM_KEYUP or WM_SYSKEYUP;
		var vkCode = lParam.wVk;
		if (SendInput.BlockInput && (int)lParam.dwExtraInfo != IGNORE_INPUT)
			return 1;

		if (keyUp)
			PressedKeys.Clear();

		if (SendInput.BlockInput || (int)lParam.dwExtraInfo == IGNORE_INPUT || nCode != 0 || !keyDown)
			return _nativeMethods.CallNextHook(_hookId, nCode, wParam, lParam);

		PressedKeys.Add(vkCode);

		return _commandProvider.TryGetCommand(PressedKeys, vkCode, out var command)
			? _execute.QueueCommand(command!)
			: _nativeMethods.CallNextHook(_hookId, nCode, wParam, lParam);
	}
}