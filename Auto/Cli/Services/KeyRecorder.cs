using Auto.Native;

using static Auto.Native.Constants;

namespace Auto.Cli.Services;

internal interface IKeyRecorder
{
	ushort[] Record(bool isSequence);
}

internal class KeyRecorder(INativeMethods nativeMethods) : IKeyRecorder
{
	private nint _hookId = nint.Zero;
	private readonly List<ushort> _pressedKeys = [];

	private bool _isSequence;

	public ushort[] Record(bool isSequence)
	{
		_isSequence = isSequence;
		_pressedKeys.Clear();
		if (_isSequence)
		{
			Console.WriteLine("Type the desired key sequence. Press Enter to confirm.");
		}
		else
		{
			Console.WriteLine("Press the desired key combination. Release to confirm.");
		}

		var handle = nativeMethods.GetCurrentProcessHandle();
		_hookId = nativeMethods.SetKeyboardHook(KeyboardHookCallback, handle);

		Application.Run();

		Console.WriteLine();

		if (isSequence) _pressedKeys.RemoveAt(_pressedKeys.Count - 1);
		return [.. _pressedKeys];
	}

	private nint KeyboardHookCallback(int nCode, nint wParam, ref KeyboardInput lParam)
	{
		var keyUp = wParam is WM_KEYUP or WM_SYSKEYUP;
		var vkCode = lParam.wVk;

		if (IsRecordFinished(vkCode, keyUp))
		{
			nativeMethods.RemoveKeyboardHook(_hookId);
			Application.ExitThread();
		}

		if (!keyUp)
		{
			if (_isSequence || !_pressedKeys.Contains(vkCode))
			{
				Console.Write(Enum.GetName(typeof(Keys), vkCode) + " ");
			}

			_pressedKeys.Add(vkCode);
		}

		return 1;
	}

	private bool IsRecordFinished(uint keyCode, bool keyUp)
	{
		return !_isSequence ? keyUp : keyCode == (uint)Keys.Enter;
	}
}