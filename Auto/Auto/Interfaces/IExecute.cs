namespace Auto.Interfaces;

public interface IExecute
{
	nint QueueCommand(Command.Command s);
}