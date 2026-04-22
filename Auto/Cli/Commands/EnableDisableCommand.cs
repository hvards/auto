using System.CommandLine;

using Auto.Cli.Services;

using CliCommand = System.CommandLine.Command;

namespace Auto.Cli.Commands;

internal static class EnableDisableCommand
{
	private record EnableDisableInput(string NameOrId, bool Enable);

	public static CliCommand CreateEnable(Func<ParseResult, CommandStore> resolveStore) => Create(resolveStore, true);
	public static CliCommand CreateDisable(Func<ParseResult, CommandStore> resolveStore) => Create(resolveStore, false);

	private static CliCommand Create(Func<ParseResult, CommandStore> resolveStore, bool enable)
	{
		var verb = enable ? "enable" : "disable";
		var command = new CliCommand(verb) { Description = $"{(enable ? "Enable" : "Disable")} a command" }
			.AddArgument<string>("name-or-id", "Command name or ID", out var nameArg);

		command.SetActionWithErrorHandling(pr =>
			Execute(resolveStore(pr), new EnableDisableInput(pr.GetValue(nameArg) ?? string.Empty, enable))
		);

		return command;
	}

	private static void Execute(CommandStore store, EnableDisableInput input)
	{
		store.Update(input.NameOrId, target => target.Enabled = input.Enable);
		Console.WriteLine($"{(input.Enable ? "Enabled" : "Disabled")} '{input.NameOrId}'");
	}
}
