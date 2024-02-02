using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;
using Microsoft.PowerShell;

namespace Auto.Tasks;

public static class PowerShell
{
	private static RunspacePool _runspacePool;
	private static readonly InitialSessionState Iss = InitialSessionState.CreateDefault();

	public static void Initialize()
	{
		Iss.ExecutionPolicy = ExecutionPolicy.Unrestricted;
		_runspacePool = RunspaceFactory.CreateRunspacePool();
		_runspacePool.Open();
	}

	public static string Execute(string file, IList<(string name, string value)> parameters)
	{
		var powerShell = System.Management.Automation.PowerShell.Create(Iss);
		powerShell.AddCommand(Path.Combine(Application.StartupPath, file));

		if (parameters != null)
		{
			foreach (var parameter in parameters)
			{
				var multi = parameter.value.StartsWith("#Multi:");
				if (multi)
				{
					foreach (var param in parameter.value[7..].Split("\n"))
					{
						powerShell.AddParameter(parameter.name, param);
					}
				}
				else
				{
					powerShell.AddParameter(parameter.name, parameter.value);
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