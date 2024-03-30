using Auto.Handlers;
using Auto.tasks;
using AutoContracts;

namespace Auto.Plugins;

public class KeyboardInputPlugin : ICommand
{
	public string Name => "Keyboard input";
	public string Description => "Send keyboard input.";
	public Guid Id { get; } = Guid.Parse("902a5fec-8684-4b83-a959-453d81de5479");
	public Type ReturnType { get; } = typeof(bool);

	public List<PluginArgument> ExpectedArguments { get; } =
	[
		new PluginArgument
		{
			Name = "Input",
			Type = typeof(string)
		}
	];

	public object Execute(object[] args)
	{
		var input = (string)args[0];
		KeyboardHandler.ReleaseAllKeys();
		SendInput.Keyboard(input);
		return false;
	}
}