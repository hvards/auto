using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;
using Microsoft.PowerShell;

namespace Auto.Tasks
{
    public class PowerShell
    {
        private static System.Management.Automation.PowerShell _powerShell;

        public static void Initialize()
        {
            var iss = InitialSessionState.CreateDefault();
            iss.ExecutionPolicy = ExecutionPolicy.Unrestricted;
            _powerShell = System.Management.Automation.PowerShell.Create(iss);
        }

        public static string Execute(string file, IList<string> parameters)
        {
            _powerShell.AddCommand(Path.Combine(Application.StartupPath, file));

            if (parameters != null)
            {
	            foreach (var parameter in parameters)
	            {
		            var multi = parameter.StartsWith("#Multi:");
		            if (multi)
		            {
			            foreach (var param in parameter[7..].Split("\n"))
			            {
				            _powerShell.AddParameter(null, param);
			            }
                    }
		            else
		            {
			            _powerShell.AddParameter(null, parameter);
		            }
	            }
            }

            var result = _powerShell.Invoke();
            _powerShell.Commands.Clear();
            return ResultToString(result);
        }

        private static string ResultToString(IEnumerable<PSObject> result)
        {
            var sb = new StringBuilder();
            foreach (var psObject in result)
                sb.AppendLine(psObject.ToString());
            return sb.ToString().Trim();
        }
    }
}
