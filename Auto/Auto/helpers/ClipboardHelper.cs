using System;
using System.Linq;
using System.Threading;
using Windows.ApplicationModel.DataTransfer;

namespace Auto.helpers
{
    public static class ClipboardHelper
    {
        private static string clipboardText;

        public static string GetHighlightedText()
        {
            KeyboardHelper.CopyHighlightedText();
            Thread.Sleep(350);
            Thread resetClipboard = StartResetClipboardThread();
            resetClipboard.Join();
            return clipboardText;
        }

        public static Thread StartResetClipboardThread() {
            Thread resetClipboard = new Thread(() => ResetClipboard());
            resetClipboard.SetApartmentState(ApartmentState.STA);
            resetClipboard.Start();
            return resetClipboard;
        }

        public static async void ResetClipboard()
        {
            var history = await Clipboard.GetHistoryItemsAsync();

            var t = history.Items;

            clipboardText = t[0]?.Content.GetTextAsync().GetAwaiter().GetResult();

            if (clipboardText == null)
            {
                clipboardText = "empty clipboard";
                return;
            }
            
            if (t.Any())
                Clipboard.DeleteItemFromHistory(t[0]);

            if (t.Count > 1)
                Clipboard.SetHistoryItemAsContent(t[1]);
        }
    }
}
