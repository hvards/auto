namespace Auto.Interfaces;

public interface ICommandExecutor
{
	List<string> ExecuteCommand(Command.Command command, string clipboard = null, string highlighted = null);
}