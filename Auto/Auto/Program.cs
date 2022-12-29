using System.Diagnostics;
using System.Runtime.InteropServices;
using Auto.Command;
using Auto.Handlers;
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
    private static List<Command.Command> _commands;
    private static List<Command.Command> _remappedKeys;
    private static readonly HashSet<ushort> PressedKeys = new();

    private static void Main()
    {
        var commandFolders =
            (Environment.GetEnvironmentVariable("Auto", EnvironmentVariableTarget.Machine) ??
             throw new Exception("Missing auto folders")).Split(";")
            .Where(Directory.Exists).ToList();
        _commands = GetCommands.Execute(commandFolders).Where(x => x.Enabled).ToList();
        _remappedKeys = GetCommands.GetRemappedKeys(commandFolders).ToList();
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

        var remappedKey = _remappedKeys.FirstOrDefault(x => x.Trigger.Combination.First() == vkCode);
        if (remappedKey != null)
            return RemapKey(remappedKey, keyUp);

        if (Execute.Executing || (int)lParam.dwExtraInfo == IGNORE_INPUT || nCode != 0 || !keyDown)
            return CallNextHookEx(_hookId, nCode, wParam, ref lParam);

        PressedKeys.Add(vkCode);

        var command = _commands.FirstOrDefault(x => x.Trigger.Check(PressedKeys, vkCode));
        return command == null ? CallNextHookEx(_hookId, nCode, wParam, ref lParam) : Execute.QueueCommand(command);
    }

    private static IntPtr RemapKey(Command.Command command, bool keyUp)
    {
        foreach (var commandArg in command.Arguments.SelectMany(x => x.Tokens).Select(x => x.Value))
        {
            var vkCode = ushort.Parse(commandArg);
            KeyboardHandler.ClickKey(vkCode, keyUp ? WM_KEYUP : WM_KEYDOWN);
        }
        
        return (IntPtr)1;
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