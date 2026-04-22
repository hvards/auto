using System.CommandLine;

using CliCommand = System.CommandLine.Command;

namespace Auto.Cli;

internal static class CliCommandExtensions
{
	internal static CliCommand AddOption<T>(
		this CliCommand command,
		string name,
		string description,
		out Option<T> option,
		bool required = false,
		bool argumentRequired = true,
		T? defaultValue = default)
	{
		option = new Option<T>(name)
		{
			Description = description,
			Required = required,
			AllowMultipleArgumentsPerToken = typeof(T).IsArray
		};
		if (!EqualityComparer<T>.Default.Equals(defaultValue, default))
			option.DefaultValueFactory = _ => defaultValue!;
		if (!argumentRequired)
			option.Arity = ArgumentArity.ZeroOrMore;
		command.Options.Add(option);
		return command;
	}

	internal static CliCommand AddArgument<T>(this CliCommand command, string name, string description,
		out Argument<T> argument)
	{
		argument = new Argument<T>(name) { Description = description };
		command.Arguments.Add(argument);
		return command;
	}

	internal static void SetActionWithErrorHandling(this CliCommand command, Action<ParseResult> action)
	{
		command.SetAction(pr =>
		{
			try
			{
				action(pr);
				return 0;
			}
			catch (ArgumentException e)
			{
				Console.Error.WriteLine(e.Message);
				return 1;
			}
		});
	}
}
