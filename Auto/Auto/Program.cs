using System.Diagnostics;
using System.Runtime.InteropServices;
using Auto.Command;
using Auto.Handlers;
using static Auto.Constants;

namespace Auto;

public class Program
{
    private delegate nint LowLevelKeyboardProc(int nCode, nint wParam, ref KeyboardInput lParam);
    private static LowLevelKeyboardProc _keyboardHook;
    private static nint _hookId = nint.Zero;

    private static List<Command.Command> _commands;
    private static List<Command.Command> _remappedKeys;
    private static HashSet<ushort> _blockedKeys;

    private static readonly HashSet<ushort> PressedKeys = new();
    private static HashSet<ushort> _activeRemapModifier;

    private static void Main()
    {
	    var commandFolders =
		    (Environment.GetEnvironmentVariable("Auto", EnvironmentVariableTarget.Machine) ??
		     throw new Exception("Missing auto folders")).Split(";")
		    .Where(Directory.Exists).ToList();
	        InitializeCommandLists(commandFolders);

	    Execute.Start();
        Hook();
        Application.Run();
        UnhookWindowsHookEx(_hookId);
    }

    private static void InitializeCommandLists(IEnumerable<string> folders)
    {
	    var commands = GetCommands.Execute(folders);
	    _commands = GetCommands.GetActions(commands).ToList();
	    _remappedKeys = GetCommands.GetRemappedKeys(commands).ToList();
	    _blockedKeys = new HashSet<ushort>(GetCommands.GetBlockedKeys(commands).Select(x => x.Trigger.Combination.First()));
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

        var remappedKey = TestRemapKey(vkCode);
        if (remappedKey != null)
            return RemapKey(remappedKey, keyUp);

        if (Execute.Executing || (int)lParam.dwExtraInfo == IGNORE_INPUT || nCode != 0 || !keyDown)
            return CallNextHookEx(_hookId, nCode, wParam, ref lParam);

        PressedKeys.Add(vkCode);

        var command = _commands.FirstOrDefault(x => x.Trigger.Check(PressedKeys, vkCode));
        return command == null
	        ? _blockedKeys.Contains(vkCode) ? 1 : CallNextHookEx(_hookId, nCode, wParam, ref lParam)
	        : Execute.QueueCommand(command);
    }

    private static Command.Command TestRemapKey(ushort vkCode)
    {
        var keys = new HashSet<ushort>(_activeRemapModifier ?? PressedKeys) { vkCode };
        var remappedKey = _remappedKeys.FirstOrDefault(x =>
	        x.Trigger.Combination.SetEquals(keys) ||
	        x.Trigger.Combination.Count == 1 && x.Trigger.Combination.First() == vkCode);
        if (remappedKey == null) return null;
        if (_activeRemapModifier == null && remappedKey.Trigger.Combination.Count > 1)
        {
            _activeRemapModifier = new HashSet<ushort>(remappedKey.Trigger.Combination);
            _activeRemapModifier.Remove(vkCode);
            KeyboardHandler.ReleaseKeys(remappedKey.Trigger.Combination);
        }
        return remappedKey;
    }

    private static nint RemapKey(Command.Command command, bool keyUp)
    {
        foreach (var commandArg in command.Arguments.SelectMany(x => x.Tokens).Select(x => x.Value))
        {
            var vkCode = ushort.Parse(commandArg);
            KeyboardHandler.ClickKey(vkCode, keyUp ? WM_KEYUP : WM_KEYDOWN);
        }
        
        return 1;
    }

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern nint SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, nint hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(nint hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern nint CallNextHookEx(nint hhk, int nCode, nint wParam, ref KeyboardInput lParam);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern nint CallNextHookEx(nint hhk, int nCode, nint wParam, ref MouseInput lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern nint GetModuleHandle(string lpModuleName);
}
