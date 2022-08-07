using System.Diagnostics;
using System.Runtime.InteropServices;
using Auto.CommandJson;
using static Auto.Constants;

namespace Auto;

public class Program
{
    private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, ref KeyboardInput lParam);
    private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, ref MouseInput lParam);
    private static LowLevelKeyboardProc _keyboardHook;
    private static LowLevelMouseProc _mouseHook;
    private static IntPtr _hookId = IntPtr.Zero;
    private static IntPtr _mouseHookId = IntPtr.Zero;
    private static List<Command> _commands;
    private static readonly HashSet<ushort> PressedKeys = new();

    private static void Main(string[] args)
    {
        _commands = GetCommands.Execute(args).Where(x => x.Enabled).ToList();
        Execute.Start();
        Hook();
        Application.Run();
        UnhookWindowsHookEx(_hookId);
        UnhookWindowsHookEx(_mouseHookId);
    }

    private static void Hook()
    {
        using var currentProcess = Process.GetCurrentProcess();
        using var module = currentProcess.MainModule;

        _keyboardHook = KeyboardHookCallback;
        _mouseHook = MouseHookCallback;
        
        _hookId = SetWindowsHookEx(WH_KEYBOARD_LL, _keyboardHook, GetModuleHandle(module!.ModuleName!), 0);
        _mouseHookId = SetWindowsHookEx(WH_MOUSE_LL, _mouseHook, GetModuleHandle(module.ModuleName!), 0);
    }

    private static IntPtr KeyboardHookCallback(int nCode, IntPtr wParam, ref KeyboardInput lParam)
    {
        var keyDown = wParam == WM_KEYDOWN || wParam == WM_SYSKEYDOWN;
        var keyUp = wParam == WM_KEYUP || wParam == WM_SYSKEYUP;
        var vkCode = lParam.wVk;

        if (Execute.Executing && (int) lParam.dwExtraInfo != IGNORE_INPUT)
            return (IntPtr) 1;

        if (keyUp)
            PressedKeys.Clear();

        if (Execute.Executing || (int)lParam.dwExtraInfo == IGNORE_INPUT || nCode != 0 || !keyDown)
            return CallNextHookEx(_hookId, nCode, wParam, ref lParam);

        PressedKeys.Add(vkCode);

        var result = _commands.FirstOrDefault(command => command.KeyCombo.SetEquals(PressedKeys) || command.TestMacro(vkCode));
        return result == null ? CallNextHookEx(_hookId, nCode, wParam, ref lParam) : Execute.QueueCommand(result);
    }

    private static IntPtr MouseHookCallback(int nCode, IntPtr wParam, ref MouseInput lParam)
    {
        if (wParam == WM_LBUTTONDOWN)
            PressedKeys.Clear();

        return CallNextHookEx(_mouseHookId, nCode, wParam, ref lParam);
    }

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, ref KeyboardInput lParam);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, ref MouseInput lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);
}