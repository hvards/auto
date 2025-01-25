namespace Auto.Command;

public class Command
{
	public Trigger Trigger { get; init; }
	public ArgumentToken[] Actions { get; init; }
	public Dictionary<string, CommandArgument[]> PowerShellArguments { get; init; }
	public Dictionary<string, CommandArgument[]> PluginArguments { get; init; }
	public bool Enabled { get; init; }
	public bool HighlightedTextRequired { get; set; }
	public bool ConcurrentExecution { get; set; }
	public bool ClipboardTextRequired { get; set; }
}