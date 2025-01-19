using System.Collections.Concurrent;
using Auto.Interfaces;

namespace Auto;

public class Execute : IExecute
{
	private readonly IClipboardHandler _clipboardHandler;
	private readonly IKeyboardHandler _keyboardHandler;
	private readonly Interfaces.ICommandExecutor _commandExecutor;

	private readonly Thread _executeThread;
	private static readonly BlockingCollection<Command.Command> MessageQueue = [];

	public Execute(IClipboardHandler clipboardHandler, IKeyboardHandler keyboardHandler,
		Interfaces.ICommandExecutor commandExecutor)
	{
		_clipboardHandler = clipboardHandler;
		_keyboardHandler = keyboardHandler;
		_commandExecutor = commandExecutor;

		_executeThread = new Thread(ProcessCommands);
		_executeThread.Start();
	}

	public nint QueueCommand(Command.Command s)
	{
		MessageQueue.Add(s);
		return 1;
	}

	private void ProcessCommands()
	{
		while (true)
		{
			try
			{
				var command = MessageQueue.Take();
				for (var i = 0; command.Trigger.MacroTriggered && i < command.Trigger.Sequence.Length - 1; i++)
					_keyboardHandler.ClickKey((ushort)Keys.Back, null);

				var clipboard = command.ClipboardTextRequired
					? _clipboardHandler.GetClipboardText()
					: string.Empty;
				var highlighted = command.HighlightedTextRequired
					? _clipboardHandler.GetClipboardText(true)
					: string.Empty;

				Task.Run(() => _commandExecutor.ExecuteCommand(command, clipboard, highlighted));
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