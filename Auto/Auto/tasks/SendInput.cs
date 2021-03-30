using System.Linq;
using Auto.helpers;
using System.Threading;
using System.Windows.Forms;
using static Auto.Constants;

namespace Auto.tasks
{
    public static class SendInput
    {
        public static void Send(bool del, string input = null)
        {
            if (del)
            {
                KeyboardHelper.ClickKey((ushort)Keys.ShiftKey, WM_KEYDOWN);
                KeyboardHelper.ClickKey((ushort)Keys.ControlKey, WM_KEYDOWN);
                KeyboardHelper.ClickKey((ushort)Keys.Left, null);
                KeyboardHelper.ClickKey((ushort)Keys.ControlKey, WM_KEYUP);
                KeyboardHelper.ClickKey((ushort)Keys.ShiftKey, WM_KEYUP);
            }
            SendKeys(input);
        }

        private static void SendKeys(string input)
        {
            for (var i = 0; i < input.Length; i++)
            {
                string str;
                switch (input[i])
                {
                    case '{':
                        str = GetKey(input, i + 1);
                        KeyboardHelper.SendChar(str);
                        i += str.Length + 1;
                        break;
                    case '[':
                        // KeyDown: [1:key], KeyUp: [0:key]
                        str = GetKey(input, i+3);
                        if (input[i + 2].Equals('!'))
                            Thread.Sleep(int.Parse(str));
                        else
                            KeyboardHelper.SendChar(str, short.Parse(input[i + 1].ToString()) == 1 ? WM_KEYDOWN : WM_KEYUP);
                        i += str.Length + 3;
                        break;
                    default:
                        KeyboardHelper.SendChar(input[i].ToString());
                        break;
                }
            }
        }

        private static string GetKey(string input, int startPos) =>
            string.Concat(input[startPos..].TakeWhile(c => !(c.Equals(']') || c.Equals('}'))));
    }
}
