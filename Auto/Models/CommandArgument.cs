using System.Text.Json.Serialization;

namespace Auto.Models;

public class CommandArgument
{
	public string? ParameterName { get; set; }
	public required ArgumentToken[] Tokens { get; set; }
	[JsonIgnore] public bool HighlightedTextRequired => Tokens.Any(x => x.Type == ArgumentType.Highlighted);
	[JsonIgnore] public bool ClipboardTextRequired => Tokens.Any(x => x.Type == ArgumentType.Clipboard);
}