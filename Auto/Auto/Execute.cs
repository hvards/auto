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
        return (IntPtr) 1;
    }

    private static void ProcessCommands()
    {
        PowerShell.Initialize();
        while (true)
        {
            var command = MessageQueue.Take();
            var arguments = command.ExecuteArguments();

            Executing = true;
            foreach (var k in ModifierKeys)
                KeyboardHandler.ClickKey(k, WM_KEYUP);
            foreach (var k in command.KeyCombo)
                KeyboardHandler.ClickKey(k, WM_KEYUP);

            ExecuteCommand(command, arguments);
            Executing = false;
        }
    }

    private static void ExecuteCommand(Command command, IReadOnlyList<string> args)
    {
        try
        {
            switch (command.Keyword)
            {
                case "MouseInput":
                    SendInput.Mouse(args[0]);
                    break;
                case "KeyboardInput":
                    SendInput.Keyboard(command.Macro?.Any() ?? false, args[0]);
                    break;
                case "StartProgram":
                    StartProgram.Start(args[0], args.Count > 1 ? args[1] : null);
                    break;
                case "DeleteClipboard":
                    ClipboardHandler.DeleteClipboard();
                    break;
            }
        }
        catch (Exception e)
        {
            Log.Error($"Error executing command: {command.Keyword},{command.Macro},{command.KeyCombo}:\n{e}");
        }
    }
}