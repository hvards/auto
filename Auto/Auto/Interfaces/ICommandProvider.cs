namespace Auto.Interfaces;

public interface ICommandProvider
{
	public bool TryGetCommand(HashSet<ushort> pressedKeys, ushort vkCode, out Command.Command command);
}