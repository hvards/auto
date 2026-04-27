using System.Collections.Concurrent;

using Auto.Handlers;

using Microsoft.Extensions.Logging;

namespace Auto;

internal interface IExecute
{
	nint QueueCommand(Models.Command s);
}

internal partial class Execute : IExecute
{
	private readonly IClipboardHandler _clipboardHandler;
	private readonly IKeyboardHandler _keyboardHandler;
	private readonly Commands.ICommandExecutor _commandExecutor;
	private readonly ILogger<Execute> _logger;

	private readonly Thread _executeThread;
	private readonly BlockingCollection<Models.Command> _messageQueue = [];

	public Execute(
		IClipboardHandler clipboardHandler,
		IKeyboardHandler keyboardHandler,
		Commands.ICommandExecutor commandExecutor,
		ILogger<Execute> logger)
	{
		_clipboardHandler = clipboardHandler;
		_keyboardHandler = keyboardHandler;
		_commandExecutor = commandExecutor;
		_logger = logger;

		_executeThread = new Thread(ProcessCommands);
		_executeThread.Start();
	}

	public nint QueueCommand(Models.Command s)
	{
		_messageQueue.Add(s);
		return 1;
	}

	private void ProcessCommands()
	{
		while (true)
		{
			try
			{
				var command = _messageQueue.Take();
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
			catch (Exception ex)
			{
				LogErrorExecutingCommand(ex);
			}
		}
	}

	[LoggerMessage(LogLevel.Error, Message = "Error executing command")]
	public partial void LogErrorExecutingCommand(Exception ex);
}
