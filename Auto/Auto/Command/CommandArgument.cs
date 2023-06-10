namespace Auto.Command;

public class CommandArgument
{
    public string ParameterName { get; set; }
    public ArgumentToken[] Tokens { get; set; }
    public bool HighlightedTextRequired => Tokens.Any(x => x.Type == ArgumentType.Highlighted);
    public bool ClipboardTextRequired => Tokens.Any(x => x.Type == ArgumentType.Clipboard);
}