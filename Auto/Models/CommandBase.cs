namespace Auto.Models;

public class CommandBase
{
	public Trigger Trigger { get; set; } = new();
	public ArgumentToken[] Actions { get; set; } = [];
	public Dictionary<string, CommandArgument[]> PowerShellArguments { get; set; } = [];
	public Dictionary<string, CommandArgument[]> PluginArguments { get; set; } = [];
	public bool Enabled { get; set; }
}