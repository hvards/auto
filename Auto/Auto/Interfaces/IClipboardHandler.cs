namespace Auto.Interfaces;

public interface IClipboardHandler
{
	string GetClipboardText(bool copyHighlighted = false);
}