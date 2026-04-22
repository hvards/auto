namespace Auto.Native;

internal static class Constants
{
	public const int WH_KEYBOARD_LL = 13;
	public const int WH_MOUSE_LL = 14;
	public const nint WM_KEYDOWN = 0x0100;
	public const nint WM_KEYUP = 0x0101;
	public const nint WM_SYSKEYDOWN = 0x0104;
	public const nint WM_SYSKEYUP = 0x0105;
	public const nint KEY_DOWN_UP = 0x0000;
	public const nint WM_LBUTTONDOWN = 0x0201;
	public const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
	public const uint MOUSEEVENTF_LEFTUP = 0x0004;
	public const int IGNORE_INPUT = 0xffffff;
}
