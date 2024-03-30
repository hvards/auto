using System.Collections.Concurrent;
using Auto.Handlers;
using Auto.tasks;

namespace Auto;

public static class Execute
{
	private static readonly Thread ExecuteThread = new(ProcessCommands);
	private static readonly BlockingCollection<Command.Command> MessageQueue = [];
	public static void Start() => ExecuteThread.Start();

	public static nint QueueCommand(Command.Command s)
	{
		MessageQueue.Add(s);
		return 1;
	}

	private static void ProcessCommands()
	{
		Plugin.Initialize();
		PowerShell.Initialize();
		while (true)
		{
			try
			{
				var command = MessageQueue.Take();
				for (var i = 0; command.Trigger.MacroTriggered && i < command.Trigger.Sequence.Length - 1; i++)
					KeyboardHandler.ClickKey((ushort)Keys.Back, null);

				var clipboard = command.ClipboardTextRequired
					? ClipboardHandler.GetClipboardText()
					: string.Empty;
				var highlighted = command.HighlightedTextRequired
					? ClipboardHandler.GetClipboardText(true)
					: string.Empty;

				Task.Run(() => command.ExecuteArguments(clipboard, highlighted));
			}
			catch (Exception e)
			{
				Log.Error($"Error executing command: {e}");
			}
			finally
			{
				// To avoid keypresses from trigger to interfere with execution
				Thread.Sleep(500);
			}
		}
	}
}