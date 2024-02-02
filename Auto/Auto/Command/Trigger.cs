namespace Auto.Command;

public class Trigger
{
    public HashSet<ushort> Combination { get; init; } = [];
    public ushort[] Sequence
    {
        get => _sequence;
        init
        {
            _sequence = value;
            _sequencePosition = new bool[value?.Length ?? 0];
        }
    }
    private readonly ushort[] _sequence;
    private bool[] _sequencePosition;
    public bool MacroTriggered { get; private set; }
    
    public bool Check(HashSet<ushort> pressedKeys, ushort lastKey)
    {
        MacroTriggered = TestMacro(lastKey);
        return MacroTriggered || (pressedKeys != null && pressedKeys.Count != 0 && pressedKeys.SetEquals(Combination));
    }

    private bool TestMacro(ushort key)
    {
        if (_sequencePosition is not { Length: > 1 })
            return false;

        if (_sequencePosition[Sequence.Length - 2] && Sequence[^1] == key)
        {
            _sequencePosition = new bool[Sequence.Length];
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