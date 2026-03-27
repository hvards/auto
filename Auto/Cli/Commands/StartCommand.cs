using System.CommandLine;
using System.Diagnostics;

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
				KillExistingInstances();
				Program.StartService();
			}
			else
			{
				Process.Start(new ProcessStartInfo
				{
					FileName = Environment.ProcessPath,
					Arguments = "start --foreground",
					UseShellExecute = false,
					CreateNoWindow = true,
				});
			}
		});

		return command;
	}

	private static void KillExistingInstances()
	{
		foreach (var process in Process.GetProcessesByName("Auto"))
		{
			if (process.Id == Environment.ProcessId) continue;
			try { process.Kill(); } catch { }
			process.Dispose();
		}
	}
}