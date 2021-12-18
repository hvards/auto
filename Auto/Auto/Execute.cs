using System.Collections.Concurrent;
using Auto.Handlers;
using Auto.Tasks;
using static Auto.Constants;

namespace Auto;

public static class Execute
{
    private static readonly Thread ExecuteThread = new(ProcessCommands);
    private static readonly BlockingCollection<Command> MessageQueue = new();
    public static bool Executing { get; set; }
    public static void Start() => ExecuteThread.Start();

    public static IntPtr QueueCommand(Command s)
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
            {
                KeyboardHandler.ClickKey(k, WM_KEYUP);
            }
            ExecuteCommand(s);
            Executing = false;
        }
    }

    private static void ExecuteCommand(Command command)
    {
        var args = TransformArguments(command.args);
        try
        {
            switch (command.Keyword)
            {
                case "SendKeys":
                    SendInput.Send(command.Macro?.Any() ?? false, args[0]);
                    break;
                case "TestCustomer":
                    SendInput.Send(command.Macro?.Any() ?? false, TestCustomer.GetTestCustomer(args[0], args[1]));
                    break;
                case "StartProgram":
                    StartProgram.Start(args[0], args.Length > 1 ? args[1] : null);
                    break;
                case "PowerShell":
                    StartProgram.ExecutePowerShellScript(args);
                    break;
                case "DeleteClipboard":
                    ClipboardHandler.DeleteClipboard();
                    break;
                case "SqlQuery":
                    NotepadHandler.OpenWithText(SqlHandler.RunSqlQuery(args[0], args[1], args[2], args[3]));
                    break;
                case "Fast":
                    break;
            }
        }
        catch (Exception e)
        {
            Log.Error($"Error executing command: {command.Keyword},{command.Macro},{command.KeyCombo}:\n{e}");
        }
    }

    private static string[] TransformArguments(IReadOnlyList<string> args)
    {
        var a = new string[args.Count];
        for (var i = 0; i < a.Length; i++)
        {
            a[i] = args[i];
            if (a[i].Contains("{:highlighted"))
                a[i] = a[i].Replace("{:highlighted}", ClipboardHandler.GetClipboardText(true));
            if (a[i].Contains("{:clipboard}"))
                a[i] = a[i].Replace("{:clipboard}", ClipboardHandler.GetClipboardText());
        }

        return a;
    }
}