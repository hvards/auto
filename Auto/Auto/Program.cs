using System.Diagnostics;
using System.Runtime.InteropServices;
using Auto.Command;
using static Auto.Constants;

namespace Auto;

public static class Program
{
    private delegate nint LowLevelKeyboardProc(int nCode, nint wParam, ref KeyboardInput lParam);
    private static LowLevelKeyboardProc _keyboardHook;
    private static nint _hookId = nint.Zero;
    private static List<Command.Command> _commands;
    private static readonly HashSet<ushort> PressedKeys = [];
    private static HashSet<ushort> _activeRemapModifier;

    private static void Main()
    {
	    var commandFolders =
		    (Environment.GetEnvironmentVariable("Auto", EnvironmentVariableTarget.Machine) ??
		     throw new Exception("Missing auto folders")).Split(";")
		    .Where(Directory.Exists).ToList();
	    _commands = GetCommands.Execute(commandFolders);

	    Execute.Start();
        Hook();
        Application.Run();
        UnhookWindowsHookEx(_hookId);
    }

    private static void Hook()
    {
        using var currentProcess = Process.GetCurrentProcess();
        using var module = currentProcess.MainModule;

        _keyboardHook = KeyboardHookCallback;
        _hookId = SetWindowsHookEx(WH_KEYBOARD_LL, _keyboardHook, GetModuleHandle(module!.ModuleName!), 0);
    }

    private static nint KeyboardHookCallback(int nCode, nint wParam, ref KeyboardInput lParam)
    {
        var keyDown = wParam == WM_KEYDOWN || wParam == WM_SYSKEYDOWN;
        var keyUp = wParam == WM_KEYUP || wParam == WM_SYSKEYUP;
        var vkCode = lParam.wVk;
        if (Execute.Executing && (int) lParam.dwExtraInfo != IGNORE_INPUT)
            return 1;

        if (keyUp)
        {
            PressedKeys.Clear();
            if (lParam.dwExtraInfo != IGNORE_INPUT && _activeRemapModifier != null && _activeRemapModifier.Contains(vkCode))
                _activeRemapModifier = null;
        }

        if (Execute.Executing || (int)lParam.dwExtraInfo == IGNORE_INPUT || nCode != 0 || !keyDown)
            return CallNextHookEx(_hookId, nCode, wParam, ref lParam);

        PressedKeys.Add(vkCode);

        var command = _commands.FirstOrDefault(x => x.Trigger.Check(PressedKeys, vkCode));
        return command == null ? CallNextHookEx(_hookId, nCode, wParam, ref lParam) : Execute.QueueCommand(command);
    }

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern nint SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, nint hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(nint hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern nint CallNextHookEx(nint hhk, int nCode, nint wParam, ref KeyboardInput lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern nint GetModuleHandle(string lpModuleName);
}
