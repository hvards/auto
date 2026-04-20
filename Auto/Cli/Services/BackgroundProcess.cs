using System.Diagnostics;

namespace Auto.Cli.Services;

internal static class BackgroundProcess
{
	public static bool Stop()
	{
		var stopped = false;
		foreach (var process in Process.GetProcessesByName("Auto"))
		{
			if (process.Id == Environment.ProcessId)
			{
				process.Dispose();
				continue;
			}
			try { process.Kill(); stopped = true; } catch { }
			process.Dispose();
		}
		return stopped;
	}

	public static void Start()
	{
		Process.Start(new ProcessStartInfo
		{
			FileName = Environment.ProcessPath,
			Arguments = "start --foreground",
			UseShellExecute = false,
			CreateNoWindow = true,
		});
	}
}