using Clipboard = Windows.ApplicationModel.DataTransfer.Clipboard;

namespace Auto.Handlers;
public static class ClipboardHandler
{
    private const int COPY_DELAY = 75;

    public static string GetClipboardText(bool copyHighlightedText = false)
    {
        if (copyHighlightedText)
            KeyboardHandler.CopyHighlightedText();

        var clipboardText = StaHandler.Execute(RetrieveClipboardText);

        if (copyHighlightedText)
            DeleteClipboard();

        return clipboardText?.Trim();
    }

    private static void DeleteClipboard() => StaHandler.Execute(ResetClipboard);

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
