using System.Text.RegularExpressions;

using Auto.Models;

namespace Auto.Cli.Services;

internal static partial class ArgParser
{
	[GeneratedRegex(@"%\{([^}]+)\}", RegexOptions.IgnoreCase)]
	private static partial Regex TokenPattern();

	internal static CommandArgument ParsePluginArgument(string arg)
	{
		return new CommandArgument { Tokens = [.. Tokenize(arg)] };
	}

	private static ArgumentToken[] Tokenize(string raw)
	{
		var tokens = new List<ArgumentToken>();
		var lastEnd = 0;

		foreach (Match match in TokenPattern().Matches(raw))
		{
			if (match.Index > lastEnd)
				tokens.Add(new ArgumentToken { Type = ArgumentType.Text, Value = raw[lastEnd..match.Index] });

			tokens.Add(new ArgumentToken { Type = ArgumentType.Variable, Value = match.Groups[1].Value });
			lastEnd = match.Index + match.Length;
		}

		if (lastEnd < raw.Length)
			tokens.Add(new ArgumentToken { Type = ArgumentType.Text, Value = raw[lastEnd..] });

		return [.. tokens];
	}
}