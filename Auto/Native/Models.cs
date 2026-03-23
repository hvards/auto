using System.Runtime.InteropServices;

namespace Auto.Native;

[StructLayout(LayoutKind.Sequential)]
internal struct KeyboardInput
{
	public ushort wVk;
	public ushort wScan;
	public uint dwFlags;
	public uint time;
	public nint dwExtraInfo;
}

[StructLayout(LayoutKind.Sequential)]
internal struct MouseInput
{
	public int dx;
	public int dy;
	public uint mouseData;
	public uint dwFlags;
	public uint time;
	public nint dwExtraInfo;
}

[StructLayout(LayoutKind.Sequential)]
internal struct HardwareInput
{
	public uint uMsg;
	public ushort wParamL;
	public ushort wParamH;
}

[StructLayout(LayoutKind.Explicit)]
internal struct InputUnion
{
	[FieldOffset(0)] public MouseInput mi;
	[FieldOffset(0)] public KeyboardInput ki;
	[FieldOffset(0)] public HardwareInput hi;
}

internal struct Input
{
	public int type;
	public InputUnion u;
}

internal enum InputType
{
	Mouse = 0,
	Keyboard = 1,
	Hardware = 2
}

[Flags]
internal enum KeyEventF
{
	KeyDown = 0x0000,
	ExtendedKey = 0x0001,
	KeyUp = 0x0002,
	Unicode = 0x0004,
	ScanCode = 0x0008
}