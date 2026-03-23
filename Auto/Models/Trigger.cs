namespace Auto.Models;

internal class Trigger
{
	public HashSet<ushort> Combination { get; set; } = [];
	public ushort[] Sequence
	{
		get => _sequence;
		set
		{
			_sequence = value;
			_sequenceEnabled = (value?.Length ?? 0) > 1;
			if (_sequenceEnabled)
				_sequencePosition = new bool[value!.Length - 1];
		}
	}

	private ushort[] _sequence = [];
	private bool[] _sequencePosition = [];
	private bool _sequenceEnabled;
	public bool MacroTriggered { get; private set; }

	public bool Check(HashSet<ushort> pressedKeys, ushort lastKey)
	{
		MacroTriggered = TestMacro(lastKey);
		return MacroTriggered || (pressedKeys.Count != 0 && pressedKeys.SetEquals(Combination));
	}

	private bool TestMacro(ushort key)
	{
		if (!_sequenceEnabled)
			return false;

		if (_sequencePosition[Sequence.Length - 2] && Sequence[^1] == key)
		{
			Array.Clear(_sequencePosition, 0, _sequencePosition.Length);
			return true;
		}

		_sequencePosition[Sequence.Length - 2] = false;
		for (var i = Sequence.Length - 2; i > 0; i--)
		{
			if (!_sequencePosition[i - 1]) continue;
			_sequencePosition[i] = Sequence[i] == key;
			_sequencePosition[i - 1] = false;
		}

		_sequencePosition[0] = Sequence[0] == key;
		return false;
	}
}