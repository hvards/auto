using System.Security.Principal;

namespace Auto.Cli;

internal static class AdminCheck
{
	internal static void WarnIfNotAdmin()
	{
		using var identity = WindowsIdentity.GetCurrent();
		var principal = new WindowsPrincipal(identity);
		if (!principal.IsInRole(WindowsBuiltInRole.Administrator))
			Console.WriteLine("Warning: Not running as administrator.");
	}
}