namespace Auto.Models;

public class CommandBase
{
	public Trigger Trigger { get; set; } = new();
	public CommandAction[] Actions { get; set; } = [];
	public bool Enabled { get; set; }
}
