namespace Auto.Models;

internal class CommandAction
{
	public ActionType Type { get; set; }
	public string Target { get; set; } = string.Empty;
	public string? Variable { get; set; }
	public int Order { get; set; }
	public CommandArgument[] Arguments { get; set; } = [];
}

internal enum ActionType { Plugin, PowerShell }