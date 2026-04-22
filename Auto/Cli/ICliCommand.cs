using CliCommand = System.CommandLine.Command;

namespace Auto.Cli;

internal interface ICliCommand
{
	CliCommand Build();
}
