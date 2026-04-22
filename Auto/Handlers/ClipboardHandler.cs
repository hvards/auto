using Clipboard = Windows.ApplicationModel.DataTransfer.Clipboard;

namespace Auto.Handlers;

internal interface IClipboardHandler
{
	string GetClipboardText(bool copyHighlighted = false);
}

internal class ClipboardHandler(IKeyboardHandler keyboardHandler) : IClipboardHandler
{
	private const int COPY_DELAY = 75;
	private static readonly Guid StaKey = new("7c1e9a2d-4b6f-4a38-9d4a-1f3b5c7e9a02");

	public string GetClipboardText(bool copyHighlightedText = false)
	{
		if (copyHighlightedText)
		{
			keyboardHandler.ReleaseAllKeys();
			keyboardHandler.CopyHighlightedText();
		}

		var clipboardText = StaHandler.Execute(StaKey, RetrieveClipboardText);

		if (copyHighlightedText)
			DeleteClipboard();

		return clipboardText?.Trim() ?? string.Empty;
	}

	private static void DeleteClipboard() => StaHandler.Execute(StaKey, ResetClipboard);

	private static string RetrieveClipboardText()
	{
		Thread.Sleep(COPY_DELAY);
		return System.Windows.Forms.Clipboard.GetText();
	}

	private static async Task<bool> ResetClipboard()
	{
		Thread.Sleep(500 + COPY_DELAY);

		var history = await Clipboard.GetHistoryItemsAsync();
		var t = history.Items;

		if (t.Count > 0)
			Clipboard.DeleteItemFromHistory(t[0]);
		if (t.Count > 1)
			Clipboard.SetHistoryItemAsContent(t[1]);

		return true;
	}
}
