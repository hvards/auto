using System.Diagnostics;
using System.Text;

using Auto.Native;

using static Auto.Native.Constants;

namespace Auto.Cli.Services;

internal interface IKeyRecorder
{
	HashSet<ushort> RecordCombination();
	ushort[] RecordSequence();
	string RecordInput(bool recordDelay = false);
}

internal class KeyRecorder(INativeMethods nativeMethods) : IKeyRecorder
{
	private nint _hookId = nint.Zero;

	private enum RecordMode { Combination, Sequence, Input }
	private enum EventType { Down, Up, Delay }
	private record RecordedEvent(EventType Type, ushort VkCode, int DelayMs = 0);

	private RecordMode _mode;
	private readonly List<RecordedEvent> _events = [];
	private readonly Stopwatch _stopwatch = new();
	private long _lastEventTicks;

	public HashSet<ushort> RecordCombination()
	{
		Record(RecordMode.Combination);
		return [.. _events.Where(e => e.Type == EventType.Down).Select(e => e.VkCode)];
	}

	public ushort[] RecordSequence()
	{
		Record(RecordMode.Sequence);
		return [.. _events.Where(e => e.Type == EventType.Down).Select(e => e.VkCode)];
	}

	public string RecordInput(bool recordDelay = false)
	{
		_stopwatch.Restart();
		_lastEventTicks = 0;

		Record(RecordMode.Input);

		CollapseEvents();
		if (!recordDelay)
			_events.RemoveAll(e => e.Type == EventType.Delay);
		return Serialize();
	}

	private void Record(RecordMode mode)
	{
		_mode = mode;
		_events.Clear();

		Console.WriteLine(mode switch
		{
			RecordMode.Combination => "Press the desired key combination. Release to confirm.",
			RecordMode.Sequence => "Type the desired key sequence. Press Enter to confirm.",
			RecordMode.Input => "Recording keyboard input. Double-tap Escape to finish.",
			_ => throw new ArgumentOutOfRangeException(nameof(mode))
		});

		var handle = nativeMethods.GetCurrentProcessHandle();
		_hookId = nativeMethods.SetKeyboardHook(KeyboardHookCallback, handle);
		Application.Run();
		while (Console.KeyAvailable) Console.ReadKey(true);
		Console.WriteLine();
	}

	private nint KeyboardHookCallback(int nCode, nint wParam, ref KeyboardInput lParam)
	{
		var keyDown = wParam is WM_KEYDOWN or WM_SYSKEYDOWN;
		var vkCode = lParam.wVk;

		if (IsFinished(keyDown, vkCode))
		{
			nativeMethods.RemoveKeyboardHook(_hookId);
			Application.ExitThread();
			return nativeMethods.CallNextHook(_hookId, nCode, wParam, lParam);
		}

		if (keyDown && _mode != RecordMode.Input)
		{
			if (_mode == RecordMode.Sequence || !_events.Any(e => e.VkCode == vkCode))
				Console.Write(Enum.GetName(typeof(Keys), vkCode) + " ");
		}

		AddDelay();
		_events.Add(new RecordedEvent(keyDown ? EventType.Down : EventType.Up, vkCode));

		return nativeMethods.CallNextHook(_hookId, nCode, wParam, lParam);
	}

	private bool IsFinished(bool keyDown, ushort vkCode) => _mode switch
	{
		RecordMode.Combination => !keyDown,
		RecordMode.Sequence => vkCode == (ushort)Keys.Enter,
		RecordMode.Input => keyDown && vkCode == (ushort)Keys.Escape && IsDoubleEscape(),
		_ => false
	};

	private bool IsDoubleEscape()
	{
		var lastDown = _events.FindLastIndex(e => e.Type == EventType.Down);
		if (lastDown < 0 || _events[lastDown].VkCode != (ushort)Keys.Escape)
			return false;

		_events.RemoveRange(lastDown, _events.Count - lastDown);
		return true;
	}

	private void AddDelay()
	{
		var now = _stopwatch.ElapsedMilliseconds;
		var elapsed = (int)(now - _lastEventTicks);
		_lastEventTicks = now;
		_events.Add(new RecordedEvent(EventType.Delay, 0, elapsed));
	}

	private void CollapseEvents()
	{
		for (var i = 0; i < _events.Count; i++)
		{
			if (_events[i].Type != EventType.Down) continue;

			var j = i + 1;
			while (j < _events.Count && _events[j].Type == EventType.Delay) j++;

			if (j >= _events.Count || _events[j].Type != EventType.Up || _events[j].VkCode != _events[i].VkCode)
				continue;

			_events.RemoveAt(j);
		}
	}

	private string Serialize()
	{
		var sb = new StringBuilder();
		foreach (var e in _events)
		{
			switch (e.Type)
			{
				case EventType.Down:
					{
						var hasMatchingUp = _events.Any(x => x.Type == EventType.Up && x.VkCode == e.VkCode);
						if (hasMatchingUp)
							sb.Append($"{{+{Enum.GetName(typeof(Keys), e.VkCode)}}}");
						else
							sb.Append(VkToChar(e.VkCode)?.ToString() ?? $"{{{Enum.GetName(typeof(Keys), e.VkCode)}}}");
						break;
					}
				case EventType.Up:
					sb.Append($"{{-{Enum.GetName(typeof(Keys), e.VkCode)}}}");
					break;
				case EventType.Delay:
					sb.Append($"{{!{e.DelayMs}}}");
					break;
			}
		}
		return sb.ToString();
	}

	private static char? VkToChar(ushort vk) => vk switch
	{
		>= 0x41 and <= 0x5A => (char)(vk + 32),
		>= 0x30 and <= 0x39 => (char)vk,
		0x20 => ' ',
		_ => null
	};
}
