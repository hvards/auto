using System.Runtime.InteropServices;

namespace Auto;

public class Constants
{
    public const int WH_KEYBOARD_LL = 13;
    public const int WH_MOUSE_LL = 14;
    public static readonly IntPtr WM_KEYDOWN = (IntPtr)0x0100;
    public static readonly IntPtr WM_KEYUP = (IntPtr)0x0101;
    public static readonly IntPtr WM_SYSKEYDOWN = (IntPtr)0x0104;
    public static readonly IntPtr WM_SYSKEYUP = (IntPtr)0x0105;
    public static readonly IntPtr KEY_DOWN_UP = (IntPtr)0x0000;
    public static readonly IntPtr WM_LBUTTONDOWN = (IntPtr)0x0201;
    public static readonly uint MOUSEEVENTF_LEFTDOWN = 0x0002;
    public static readonly uint MOUSEEVENTF_LEFTUP = 0x0004;
    public static readonly int IGNORE_INPUT = 0xffffff;

    [StructLayout(LayoutKind.Sequential)]
    public struct KeyboardInput
    {
        public ushort wVk;
        public ushort wScan;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MouseInput
    {
        public int dx;
        public int dy;
        public uint mouseData;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct HardwareInput
    {
        public uint uMsg;
        public ushort wParamL;
        public ushort wParamH;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct InputUnion
    {
        [FieldOffset(0)] public MouseInput mi;
        [FieldOffset(0)] public KeyboardInput ki;
        [FieldOffset(0)] public HardwareInput hi;
    }

    public struct Input
    {
        public int type;
        public InputUnion u;
    }

    public enum InputType
    {
        Mouse = 0,
        Keyboard = 1,
        Hardware = 2
    }

    [Flags]
    public enum KeyEventF
    {
        KeyDown = 0x0000,
        ExtendedKey = 0x0001,
        KeyUp = 0x0002,
        Unicode = 0x0004,
        ScanCode = 0x0008
    }
}