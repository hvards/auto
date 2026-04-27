using Auto.Commands;
using Auto.Native;

using static Auto.Native.Constants;

namespace Auto;

internal class KeyListener
{
	private readonly ICommandProvider _commandProvider;
	private readonly IExecute _execute;
	private readonly INativeMethods _nativeMethods;

	private nint _hookId = nint.Zero;
	private readonly HashSet<ushort> _pressedKeys = [];
	private readonly HashSet<ushort> _suppressedRepeats = [];

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
		var vkCode = lParam.wVk;

		if ((int)lParam.dwExtraInfo == IGNORE_INPUT || nCode != 0)
			return _nativeMethods.CallNextHook(_hookId, nCode, wParam, lParam);

		if (!keyDown)
		{
			_pressedKeys.Remove(vkCode);
			_suppressedRepeats.Remove(vkCode);
			return _nativeMethods.CallNextHook(_hookId, nCode, wParam, lParam);
		}

		// Check _pressedKeys key state in case of missing key up events
		_pressedKeys.RemoveWhere(k => k != vkCode && !_nativeMethods.IsKeyPressed(k));
		_suppressedRepeats.IntersectWith(_pressedKeys);

		if (_suppressedRepeats.Contains(vkCode))
			return 1;

		_pressedKeys.Add(vkCode);

		if (!_commandProvider.TryGetCommand(_pressedKeys, vkCode, out var command))
			return _nativeMethods.CallNextHook(_hookId, nCode, wParam, lParam);

		_suppressedRepeats.UnionWith(_pressedKeys);
		return _execute.QueueCommand(command!);
	}
}
