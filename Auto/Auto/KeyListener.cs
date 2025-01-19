using Auto.Interfaces;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Auto.tasks;
using static Auto.Constants;

namespace Auto;

public class KeyListener
{
    private readonly ICommandProvider _commandProvider;
    private readonly IExecute _execute;

    private delegate nint LowLevelKeyboardProc(int nCode, nint wParam, ref KeyboardInput lParam);

    private static LowLevelKeyboardProc _keyboardHook;
    private static nint _hookId = nint.Zero;
    private static readonly HashSet<ushort> PressedKeys = [];

    public KeyListener(ICommandProvider commandProvider, IExecute execute)
    {
        _commandProvider = commandProvider;
        _execute = execute;

        Hook();
    }

    private void Hook()
    {
        using var currentProcess = Process.GetCurrentProcess();
        using var module = currentProcess.MainModule;

        _keyboardHook = KeyboardHookCallback;
        _hookId = SetWindowsHookEx(WH_KEYBOARD_LL, _keyboardHook, GetModuleHandle(module!.ModuleName!), 0);
    }

    private nint KeyboardHookCallback(int nCode, nint wParam, ref KeyboardInput lParam)
    {
        var keyDown = wParam is WM_KEYDOWN or WM_SYSKEYDOWN;
        var keyUp = wParam is WM_KEYUP or WM_SYSKEYUP;
        var vkCode = lParam.wVk;
        if (SendInput.BlockInput && (int)lParam.dwExtraInfo != IGNORE_INPUT)
            return 1;

        if (keyUp)
            PressedKeys.Clear();

        if (SendInput.BlockInput || (int)lParam.dwExtraInfo == IGNORE_INPUT || nCode != 0 || !keyDown)
            return CallNextHookEx(_hookId, nCode, wParam, ref lParam);

        PressedKeys.Add(vkCode);

        return _commandProvider.TryGetCommand(PressedKeys, vkCode, out var command)
            ? _execute.QueueCommand(command)
            : CallNextHookEx(_hookId, nCode, wParam, ref lParam);
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

    ~KeyListener()
    {
        UnhookWindowsHookEx(_hookId);
    }
}