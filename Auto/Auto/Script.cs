using System.Collections.Generic;

namespace Auto
{
    public class Script
    {
        public HashSet<ushort> KeyCombo { get; set; }
        public string Command { get; set; }
        public ushort[] Macro { get; set; }
        public string[] CommandArgs { get; set; }
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
}
