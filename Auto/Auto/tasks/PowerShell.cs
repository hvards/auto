using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;
using Microsoft.PowerShell;

namespace Auto.Tasks
{
    public class PowerShell
    {
		private static RunspacePool _runspacePool;
		private static InitialSessionState iss = InitialSessionState.CreateDefault();

        public static void Initialize()
        {
            iss.ExecutionPolicy = ExecutionPolicy.Unrestricted;
			_runspacePool = RunspaceFactory.CreateRunspacePool();
			_runspacePool.Open();
        }

        public static string Execute(string file, IList<string> parameters)
        {
            var powerShell = System.Management.Automation.PowerShell.Create(iss);
            powerShell.AddCommand(Path.Combine(Application.StartupPath, file));

            if (parameters != null)
            {
	            foreach (var parameter in parameters)
	            {
		            var multi = parameter.StartsWith("#Multi:");
		            if (multi)
		            {
			            foreach (var param in parameter[7..].Split("\n"))
			            {
				            powerShell.AddParameter(null, param);
			            }
                    }
		            else
		            {
			            powerShell.AddParameter(null, parameter);
		            }
	            }
            }
            var result= powerShell.Invoke();
            return ResultToString(result);
        }

        private static string ResultToString(IEnumerable<PSObject> result)
        {
            var sb = new StringBuilder();
            foreach (var psObject in result.Where(x => x != null))
                sb.AppendLine(psObject.ToString());
            return sb.ToString().Trim();
        }
    }
}
