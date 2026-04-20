using System.CommandLine;

using Auto.Cli.Services;

using CliCommand = System.CommandLine.Command;

namespace Auto.Cli.Commands;

internal static class StopCommand
{
	public static CliCommand Create()
	{
		var command = new CliCommand("stop", "Stop the background service");

		command.SetActionWithErrorHandling(_ =>
		{
			var stopped = BackgroundProcess.Stop();
			Console.WriteLine(stopped ? "Stopped background process" : "No running background process");
		});

		return command;
	}
}