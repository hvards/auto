using System.CommandLine;

namespace Auto.Cli.Services;

internal interface ICommandStoreFactory
{
	Option<string> ConfigDirOption { get; }
	CommandStore Create(ParseResult pr);
}
