using Auto.Handlers;
using Auto.Tasks;
using AutoContracts;
using Microsoft.Extensions.DependencyInjection;

namespace Auto.Plugins;

public class KeyboardInputPlugin(IServiceProvider? serviceProvider) : ICommand
{
	public string Name => "KeyboardInput";
	public string Description => "Send keyboard input.";
	public Guid Id { get; } = Guid.Parse("902a5fec-8684-4b83-a959-453d81de5479");
	public Type ReturnType { get; } = typeof(bool);

	public List<PluginArgument> ExpectedArguments { get; } =
	[
		new()
		{
			Name = "Input",
			Type = typeof(string)
		}
	];

	public object? Execute(object?[] args)
	{
		var input = (string)(args[0] ?? string.Empty);

		serviceProvider?.GetService<IKeyboardHandler>()?.ReleaseAllKeys();
		serviceProvider?.GetService<ISendInput>()?.Keyboard(input);
		return false;
	}
}