using System;
using System.Linq;
using System.Threading;
using Windows.ApplicationModel.DataTransfer;

namespace Auto.helpers
{
    public static class ClipboardHelper
    {
        private static string _clipboardText;
        private const int CopyDelay = 75;

        public static string GetClipboardText(bool copyHighlightedText = false)
        {   
            if (copyHighlightedText)
                KeyboardHelper.CopyHighlightedText();

            var retrieveClipboard = new Thread(RetrieveClipboardText);
            StartStaThread(retrieveClipboard);

            if (copyHighlightedText)
                DeleteClipboard();

            retrieveClipboard.Join();
            return _clipboardText?.Trim();
        }

        public static void DeleteClipboard() =>
            StartStaThread(new Thread(ResetClipboard));

        public static void RetrieveClipboardText()
        {
            Thread.Sleep(CopyDelay);
            _clipboardText = System.Windows.Forms.Clipboard.GetText();
        }

        private static void StartStaThread(Thread clipboardThread)
        {
            clipboardThread.SetApartmentState(ApartmentState.STA);
            clipboardThread.Start();
        }

        public static async void ResetClipboard()
        {
            Thread.Sleep(500 + CopyDelay);

            var history = await Clipboard.GetHistoryItemsAsync();
            var t = history.Items;

            if (t.Any())
                Clipboard.DeleteItemFromHistory(t[0]);

            if (t.Count > 1)
                Clipboard.SetHistoryItemAsContent(t[1]);
        }
    }
}