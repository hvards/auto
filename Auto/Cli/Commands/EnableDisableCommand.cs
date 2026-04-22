using Auto.Cli.Services;

using CliCommand = System.CommandLine.Command;

namespace Auto.Cli.Commands;

internal class EnableDisableCommand(ICommandStoreFactory storeFactory, bool enable) : ICliCommand
{
	private record EnableDisableInput(string NameOrId, bool Enable);

	public CliCommand Build()
	{
		var verb = enable ? "enable" : "disable";
		var command = new CliCommand(verb) { Description = $"{(enable ? "Enable" : "Disable")} a command" }
			.AddArgument<string>("name-or-id", "Command name or ID", out var nameArg);

		command.SetActionWithErrorHandling(pr =>
			Execute(storeFactory.Create(pr), new EnableDisableInput(pr.GetValue(nameArg) ?? string.Empty, enable))
		);

		return command;
	}

	private static void Execute(CommandStore store, EnableDisableInput input)
	{
		store.Update(input.NameOrId, target => target.Enabled = input.Enable);
		Console.WriteLine($"{(input.Enable ? "Enabled" : "Disabled")} '{input.NameOrId}'");
	}
}
