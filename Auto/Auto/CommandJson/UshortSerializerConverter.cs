using System.Text.Json;
using System.Text.Json.Serialization;

namespace Auto.CommandJson
{
    public class UshortSerializerConverter : JsonConverter<ushort>
    {
        private static readonly Dictionary<string, Keys> KeyMap = new()
        {
            { "Backspace", Keys.Back },
            { "Tab", Keys.Tab },
            { "Return", Keys.Return },
            { "CapsLock", Keys.CapsLock },
            { "esc", Keys.Escape },
            { "Space", Keys.Space },
            { "LeftArrow", Keys.Left },
            { "UpArrow", Keys.Up },
            { "RightArrow", Keys.Right },
            { "DownArrow", Keys.Down },
            { "PrintScreen", Keys.Snapshot },
            { "Insert", Keys.Insert },
            { "Delete", Keys.Delete },
            { "0", Keys.D0 },
            { "1", Keys.D1 },
            { "2", Keys.D2 },
            { "3", Keys.D3 },
            { "4", Keys.D4 },
            { "5", Keys.D5 },
            { "6", Keys.D6 },
            { "7", Keys.D7 },
            { "8", Keys.D8 },
            { "9", Keys.D9 },
            { "a", Keys.A },
            { "b", Keys.B },
            { "c", Keys.C },
            { "d", Keys.D },
            { "e", Keys.E },
            { "f", Keys.F },
            { "g", Keys.G },
            { "h", Keys.H },
            { "i", Keys.I },
            { "j", Keys.J },
            { "k", Keys.K },
            { "l", Keys.L },
            { "m", Keys.M },
            { "n", Keys.N },
            { "o", Keys.O },
            { "p", Keys.P },
            { "q", Keys.Q },
            { "r", Keys.R },
            { "s", Keys.S },
            { "t", Keys.T },
            { "u", Keys.U },
            { "v", Keys.V },
            { "w", Keys.W },
            { "x", Keys.X },
            { "y", Keys.Y },
            { "z", Keys.Z },
            { "Win", Keys.LWin },
            { "Numpad0", Keys.NumPad0 },
            { "Numpad1", Keys.NumPad1 },
            { "Numpad2", Keys.NumPad2 },
            { "Numpad3", Keys.NumPad3 },
            { "Numpad4", Keys.NumPad4 },
            { "Numpad5", Keys.NumPad5 },
            { "Numpad6", Keys.NumPad6 },
            { "Numpad7", Keys.NumPad7 },
            { "Numpad8", Keys.NumPad8 },
            { "Numpad9", Keys.NumPad9 },
            { "NumpadMultiply", Keys.Multiply },
            { "NumpadPlus", Keys.Add },
            { "NumpadMinus", Keys.Subtract },
            { "NumpadDot", Keys.Decimal },
            { "NumpadDivide", Keys.Divide },
            { "F1", Keys.F1 },
            { "F2", Keys.F2 },
            { "F3", Keys.F3 },
            { "F4", Keys.F4 },
            { "F5", Keys.F5 },
            { "F6", Keys.F6 },
            { "F7", Keys.F7 },
            { "F8", Keys.F8 },
            { "F9", Keys.F9 },
            { "F10", Keys.F10 },
            { "F11", Keys.F11 },
            { "F12", Keys.F12 },
            { "NumLock", Keys.NumLock },
            { "Shift", Keys.LShiftKey },
            { "RShift", Keys.RShiftKey },
            { "LCtrl", Keys.LControlKey },
            { "RCtrl", Keys.RControlKey },
            { "LAlt", Keys.LMenu },
            { "RAlt", Keys.RMenu },
            { "tilda", Keys.Oem1 },
            { "plus", Keys.Oemplus },
            { "comma", Keys.Oemcomma },
            { "minus", Keys.OemMinus },
            { "point", Keys.OemPeriod },
            { "singleQ", Keys.Oem2 },
            { "ø", Keys.Oem3 },
            { "backslash", Keys.Oem4 },
            { "pipe", Keys.OemPipe },
            { "å", Keys.Oem6 },
            { "æ", Keys.Oem7 }
        };
        public override ushort Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();
            return (ushort) KeyMap[value];
        }

        public override void Write(Utf8JsonWriter writer, ushort value, JsonSerializerOptions options)
        {
            foreach (var val in KeyMap.Where(val => (ushort) val.Value == value))
            {
                writer.WriteStringValue(val.Key);
                break;
            }
        }
    }
}
