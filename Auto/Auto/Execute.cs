using System.Collections.Concurrent;
using Auto.Handlers;
using Auto.Tasks;

namespace Auto;

public static class Execute
{
    private static readonly Thread ExecuteThread = new(ProcessCommands);
    private static readonly BlockingCollection<Command.Command> MessageQueue = new();
    public static bool Executing { get; set; }
    public static void Start() => ExecuteThread.Start();

    public static nint QueueCommand(Command.Command s)
    {
        MessageQueue.Add(s);
        return 1;
    }

    private static void ProcessCommands()
    {
        PowerShell.Initialize();
        while (true)
        {
            try
            {
                var command = MessageQueue.Take();

                var clipboard = command.ClipboardTextRequired ? ClipboardHandler.GetClipboardText() : string.Empty;
                var highlighted = command.HighlightedTextRequired ? ClipboardHandler.GetClipboardText(true) : string.Empty;

                var arguments = command.ExecuteArguments(clipboard, highlighted);

                Executing = true;

                ClearKeyboardInput(command);
                ExecuteCommand(command, arguments);
            }
            catch(Exception e)
            {
                Log.Error($"Error executing command: {e}");
            }
            finally
            {
                Executing = false;
            }
        }
    }

    private static void ClearKeyboardInput(Command.Command command)
    {
        KeyboardHandler.ReleaseAllKeys();
        for (var i = 0; command.Trigger.MacroTriggered && i < command.Trigger.Sequence.Length - 1; i++)
            KeyboardHandler.ClickKey((ushort)Keys.Back, null);
    }

    private static void ExecuteCommand(Command.Command command, IReadOnlyList<string> args)
    {
            switch (command.Action)
            {
                case "MouseInput":
                    SendInput.Mouse(args[0]);
                    break;
                case "KeyboardInput":
                    SendInput.Keyboard(args[0]);
                    break;
                case "StartProgram":
                    StartProgram.Start(args[0], args.Count > 1 ? args[1] : null);
                    break;
            }
    }
}