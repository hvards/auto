namespace Auto;

public class Command
{
    public HashSet<ushort> KeyCombo { get; set; }
    public string Keyword { get; set; }
    public ushort[] Macro { get; set; }
    public string[] args { get; set; }
    private ushort _macroPosition;

    public bool TestMacro(int key)
    {
        if (_macroPosition >= Macro.Length || key != Macro[_macroPosition++])
        {
            _macroPosition = 0;
            return false;
        }

        if (_macroPosition == Macro.Length)
            _macroPosition = 0;
        return _macroPosition == 0;
    }
}