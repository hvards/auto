using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using Windows.UI.Composition;
using Microsoft.VisualBasic;

namespace Auto.tasks
{
    internal class StartProgram
    {
        public static void Start(string program, string args, bool hidden = false)
        {
            var psi = GetCmdProcessStartInfo();
            psi.Arguments = $"/c start {(hidden ? "/b " : "")}\"\" {program} \"{args}\"";
            Process.Start(psi);
        }

        private static ProcessStartInfo GetCmdProcessStartInfo() =>
            new ProcessStartInfo
            {
                FileName = "cmd",
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = false,
                CreateNoWindow = true
            };

        public static void ExecutePowerShellScript(string[] args)
        {
            var psi = GetCmdProcessStartInfo();
            foreach (var arg in new [] {"/c", "start", "", "/b", "powershell.exe", Application.StartupPath + args[0]})
                psi.ArgumentList.Add(arg);

            for (var i = 1; i < args.Length; i++)
                psi.ArgumentList.Add(args[i]);

            Process.Start(psi);
        }
    }
}