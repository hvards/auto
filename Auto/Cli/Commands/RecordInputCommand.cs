using Auto.Cli.Services;

using CliCommand = System.CommandLine.Command;

namespace Auto.Cli.Commands;

internal class RecordInputCommand(IKeyRecorder keyRecorder) : ICliCommand
{
	public CliCommand Build()
	{
		var command = new CliCommand("record-input") { Description = "Record keyboard input as plugin syntax" }
			.AddOption<bool>("--delay", "Record delays between keystrokes", out var delayOption);

		command.SetActionWithErrorHandling(pr =>
		{
			var result = keyRecorder.RecordInput(pr.GetValue(delayOption));
			Console.WriteLine(result);
		});

		return command;
	}
}
