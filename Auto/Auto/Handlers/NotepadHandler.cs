using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Auto.Handlers;

public class NotepadHandler
{
    public static void OpenWithText(string text)
    {
        var notepad = Process.Start(new ProcessStartInfo("notepad.exe"));
        if (notepad == null)
            return;

        notepad.WaitForInputIdle();

        var child = FindWindowEx(notepad.MainWindowHandle, new IntPtr(0), "Edit", null);
        SendMessage(child, 0x000C, 0, text);
    }

    [DllImport("user32.dll", EntryPoint = "FindWindowEx", CharSet = CharSet.Unicode)]
    private static extern IntPtr FindWindowEx(IntPtr hWndParent, IntPtr hWndChildAfter, string lpszClass, string lpszWindow);

    [DllImport("User32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Unicode)]
    private static extern int SendMessage(IntPtr hWnd, int uMsg, int wParam, string lParam);
}