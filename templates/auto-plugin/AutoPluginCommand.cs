using AutoContracts;

namespace AutoPlugin;

public class AutoPluginCommand : ICommand
{
    public string Name => "AutoPlugin";
    public string Description => "PLUGIN-DESCRIPTION";
    public Guid Id { get; } = Guid.Parse("PLUGIN-GUID");
    public Type ReturnType { get; } = typeof(string);
    public bool RequiresSta => false;

    public List<PluginArgument> ExpectedArguments { get; } =
    [
        new()
        {
            Name = "Input",
            Type = typeof(string)
        }
    ];

    public void Init() { }

    public object? Execute(object?[] args)
    {
        var input = (string)(args[0] ?? string.Empty);
        return input;
    }
}