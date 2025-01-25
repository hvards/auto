using System.Diagnostics;

namespace Auto.tasks;

public static class StartProgram
{
    public static void Start(string program, string args, bool hidden = false)
    {
        var psi = GetCmdProcessStartInfo();
        psi.Arguments = $"/c start {(hidden ? "/b " : "")}\"\" \"{program}\" \"{args}\"";
        Process.Start(psi);
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