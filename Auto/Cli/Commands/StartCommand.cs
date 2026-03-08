using System.CommandLine;
using System.Diagnostics;
using CliCommand = System.CommandLine.Command;

namespace Auto.Cli.Commands;

public static class StartCommand
{
	public static CliCommand Create()
	{
		var foregroundOption = new Option<bool>("--foreground") { Hidden = true };

		var command = new CliCommand("start", "Start the background service");
		command.Options.Add(foregroundOption);

		command.SetAction(ctx =>
		{
			if (ctx.GetValue(foregroundOption))
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

			return 0;
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
