using System.CommandLine;
using System.IO;

namespace Auto.Cli.Services;

internal class CommandStoreFactory : ICommandStoreFactory
{
	public Option<string> ConfigDirOption { get; } = new("--config-dir")
	{
		DefaultValueFactory = _ => Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config", "auto"),
		Description = "Configuration directory path",
		Recursive = true,
		Hidden = true
	};

	public CommandStore Create(ParseResult pr)
		=> new(pr.GetValue(ConfigDirOption) ?? string.Empty);
}
