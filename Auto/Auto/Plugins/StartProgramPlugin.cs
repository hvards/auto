using System.Diagnostics;
using AutoContracts;

namespace Auto.Plugins;

public class StartProgramPlugin : ICommand
{
	public string Name => "Start program";
	public string Description => "Start program";
	public Guid Id { get; } = Guid.Parse("21092f13-5366-4cba-90df-66bd123e66a5");
	public Type ReturnType { get; } = typeof(bool);

	public List<PluginArgument> ExpectedArguments { get; } =
	[
		new()
		{
			Name = "Program",
			Type = typeof(string)
		},
		new()
		{
			Name = "Arguments",
			Type = typeof(string)
		}
	];

	public object Execute(object[] args)
	{
		var program = (string)args[0];
		var arguments = args.Length > 1 ? (string)args[1] : string.Empty;
		const bool hidden = false;

        var psi = GetCmdProcessStartInfo();
        psi.Arguments = $"/c start {(hidden ? "/b " : "")}\"\" \"{program}\" \"{arguments}\"";
        Process.Start(psi);

        return true;
	}

    private static ProcessStartInfo GetCmdProcessStartInfo() =>
        new()
        {
            FileName = "cmd",
            WindowStyle = ProcessWindowStyle.Hidden,
            UseShellExecute = false,
            CreateNoWindow = true
        };
}