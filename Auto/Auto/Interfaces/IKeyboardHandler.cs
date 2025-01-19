namespace Auto.Interfaces;

public interface IKeyboardHandler
{
    void ReleaseAllKeys();
    void SendChar(string ch, nint? action = null);
    void ClickKey(ushort vk, nint? action);
    void CopyHighlightedText();
}