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
            for (int i = 0; i < input.Length; i++)
            {
                if (input[i].Equals('{')) // variable name inside {}
                {
                    int startPos = i + 1;
                    while (!input[i].Equals('}'))
                        i++;
                    var str = input[startPos..i];
                    KeyboardHelper.SendChar(str);
                }
                else if (input[i].Equals('['))
                {
                    // KeyDown: [1:key], KeyUp: [0:key]
                    int startPos = i + 1;
                    while (!input[i].Equals(']'))
                        i++;
                    var str = input[(startPos+2)..i];
                    if (input[startPos + 1].Equals('!'))
                        Thread.Sleep(int.Parse(str));
                    else
                        KeyboardHelper.SendChar(str, short.Parse(input[startPos].ToString()) == 1 ? WM_KEYDOWN : WM_KEYUP);
                }
                else 
                    KeyboardHelper.SendChar(input[i].ToString());
            }
        }
    }
}
