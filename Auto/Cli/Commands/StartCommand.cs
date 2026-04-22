using System.CommandLine;

using Auto.Cli.Services;

using CliCommand = System.CommandLine.Command;

namespace Auto.Cli.Commands;

internal static class StartCommand
{
	public static CliCommand Create()
	{
		var foregroundOption = new Option<bool>("--foreground") { Hidden = true };

		var command = new CliCommand("start", "Start the background service");
		command.Options.Add(foregroundOption);

		command.SetActionWithErrorHandling(pr =>
		{
			AdminCheck.WarnIfNotAdmin();

			if (pr.GetValue(foregroundOption))
			{
				BackgroundProcess.Stop();
				Program.ConfigureFileLogger();
				Program.StartService();
			}
			else
			{
				BackgroundProcess.Start();
			}
		});

		return command;
	}
}
