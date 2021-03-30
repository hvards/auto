using System.Diagnostics;

namespace Auto.tasks
{
    internal class StartProgram
    {
        public static void Start(string program, string args)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "cmd",
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = false,
                CreateNoWindow = true,
                Arguments = $"/c start \"\" \"{program}\" \"{args}\""
            });
        }
    }
}
