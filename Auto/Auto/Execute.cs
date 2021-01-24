using Auto.helpers;
using Auto.scripts;
using Auto.tasks;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using static Auto.Constants;

namespace Auto
{
    public static class Execute
    {
        private static readonly Thread ExecuteThread = new Thread(ProcessCommands);
        private static readonly BlockingCollection<Script> MessageQueue = new BlockingCollection<Script>();
        public static bool Executing { get; set; }

        public static void Start() => ExecuteThread.Start();

        public static IntPtr QueueCommand(Script s)
        {
            MessageQueue.Add(s);
            return (IntPtr)1;
        }

        private static void ProcessCommands()
        {
            while (true)
            {
                var s = MessageQueue.Take();
                Executing = true;
                foreach (var k in ModifierKeys)
                    KeyboardHelper.ClickKey(k, WM_KEYUP);
                ExecuteScript(s);
                Executing = false;
            }
        }

        private static void ExecuteScript(Script script)
        {
            var args = TransformArguments(script.CommandArgs);
            try
            {
                switch (script.Command)
                {
                    case "SendKeys":
                        SendInput.Send(script.Macro?.Any() ?? false, args[0]);
                        break;
                    case "TestCustomer":
                        SendInput.Send(script.Macro?.Any() ?? false, TestCustomer.GetTestCustomer(args[0], args[1]));
                        break;
                    case "StartProgram":
                        StartProgram.Start(args[0], args.Length > 1 ? args[1] : null);
                        break;
                    case "DeleteClipboard":
                        ClipboardHelper.StartResetClipboardThread();
                        break;
                    case "Fast":
                        break;
                }
            }
            catch (Exception e)
            {
                Log.Error($"Error ececuting command: {script.Command},{script.Macro},{script.KeyCombo}:\n{e}");
            }
        }

        private static string[] TransformArguments(string[] args)
        {
            // Do not update args
            string[] a = new string[args.Length];
            for (var i = 0; i < a.Length; i++)
            {
                a[i] = args[i];
                if (a[i].Contains("{:highlighted}"))
                    a[i] = a[i].Replace("{:highlighted}", ClipboardHelper.GetHighlightedText());
            }
            return a;
        }
    }
}
