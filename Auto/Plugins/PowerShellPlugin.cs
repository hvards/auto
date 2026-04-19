using System.IO;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;

using AutoContracts;

using Microsoft.PowerShell;

namespace Auto.Plugins;

internal class PowerShellPlugin : ICommand
{
	private static readonly InitialSessionState SessionState = InitialSessionState.CreateDefault();

	public string Name => "PowerShell";
	public string Description => "Execute script from ~/.config/auto/powershell/ (additional args are optional).";
	public Guid Id { get; } = Guid.Parse("a4c9b4d8-9136-4b5b-b656-f1a3af4f6f24");
	public Type ReturnType { get; } = typeof(string);
	public bool RequiresSta => false;

	public List<PluginArgument> ExpectedArguments { get; } =
	[
		new()
		{
			Name = "Script",
			Type = typeof(string)
		},
		new()
		{
			Name = "Argument",
			Type = typeof(string[])
		}
	];

	public void Init()
	{
		SessionState.ExecutionPolicy = ExecutionPolicy.Unrestricted;
	}

	public object? Execute(object?[] args)
	{
		var script = args.FirstOrDefault()?.ToString();
		if (string.IsNullOrWhiteSpace(script))
			throw new ArgumentException("PowerShell requires script name as first argument");

		var scriptPath = Path.Combine(GetPowerShellFolder(), script);
		if (!File.Exists(scriptPath))
			throw new FileNotFoundException($"PowerShell script not found: {script}", scriptPath);

		using var powerShell = PowerShell.Create(SessionState);
		powerShell.AddCommand(scriptPath);

		foreach (var value in GetScriptArguments(args))
		{
			var eq = value.IndexOf('=');
			if (eq > 0)
				powerShell.AddParameter(value[..eq], value[(eq + 1)..]);
			else
				powerShell.AddArgument(value);
		}

		var result = powerShell.Invoke();
		return ResultToString(result);
	}

	private static string GetPowerShellFolder() => Path.Combine(
		Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config", "auto", "powershell"
	);

	private static IEnumerable<string> GetScriptArguments(object?[] args)
	{
		if (args.Length <= 1)
			return [];

		if (args[1] is IEnumerable<string> values)
			return values;

		if (args[1] is IEnumerable<object?> objectValues)
			return objectValues.Select(x => x?.ToString() ?? string.Empty);

		return args.Skip(1).Select(x => x?.ToString() ?? string.Empty);
	}

	private static string ResultToString(IEnumerable<PSObject> result)
	{
		var sb = new StringBuilder();
		foreach (var psObject in result.Where(x => x != null))
			sb.AppendLine(psObject.ToString());
		return sb.ToString().Trim();
	}
}