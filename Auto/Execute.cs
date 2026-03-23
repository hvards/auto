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
	private readonly Auto.Commands.ICommandExecutor _commandExecutor;
	private readonly ILogger<Execute> _logger;

	private readonly Thread _executeThread;
	private static readonly BlockingCollection<Models.Command> MessageQueue = [];

	public Execute(IClipboardHandler clipboardHandler, IKeyboardHandler keyboardHandler,
		Auto.Commands.ICommandExecutor commandExecutor, ILogger<Execute> logger)
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
			catch (Exception ex)
			{
				LogErrorExecutingCommand(ex);
			}
			finally
			{
				// To avoid keypresses from trigger to interfere with execution
				Thread.Sleep(500);
			}
		}
	}

	[LoggerMessage(LogLevel.Error, Message = "Error executing command")]
	public partial void LogErrorExecutingCommand(Exception ex);
}