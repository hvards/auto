using System.Text.RegularExpressions;
using Auto.Models;

namespace Auto.Cli.Services;

internal static partial class ArgParser
{
	[GeneratedRegex(@"%\{(clipboard|highlighted|(?:plugin|powershell|ps):([^}]+))\}", RegexOptions.IgnoreCase)]
	private static partial Regex TokenPattern();
	internal static CommandArgument ParsePluginArg(string arg)
	{
		return new CommandArgument { Tokens = [.. Tokenize(arg)] };
	}

	internal static CommandArgument ParsePowerShellArg(string arg)
	{
		if (!arg.StartsWith('%'))
		{
			var eqIdx = arg.IndexOf('=');
			if (eqIdx > 0)
				return new CommandArgument
				{
					ParameterName = arg[..eqIdx],
					Tokens = [.. Tokenize(arg[(eqIdx + 1)..])]
				};
		}
		return ParsePluginArg(arg);
	}

	private static ArgumentToken[] Tokenize(string raw)
	{
		var tokens = new List<ArgumentToken>();
		var lastEnd = 0;

		foreach (Match match in TokenPattern().Matches(raw))
		{
			if (match.Index > lastEnd)
				tokens.Add(new ArgumentToken { Type = ArgumentType.Text, Value = raw[lastEnd..match.Index] });

			tokens.Add(MatchToToken(match));
			lastEnd = match.Index + match.Length;
		}

		if (lastEnd < raw.Length)
			tokens.Add(new ArgumentToken { Type = ArgumentType.Text, Value = raw[lastEnd..] });

		return tokens.Count > 0 ? [.. tokens] : [new ArgumentToken { Type = ArgumentType.Text, Value = raw }];
	}

	private static ArgumentToken MatchToToken(Match match)
	{
		if (!match.Groups[2].Success)
			return new ArgumentToken
			{
				Type = match.Groups[1].ValueSpan.Equals("clipboard", StringComparison.OrdinalIgnoreCase)
					? ArgumentType.Clipboard
					: ArgumentType.Highlighted
			};

		var colonIdx = match.Groups[1].ValueSpan.IndexOf(':');
		return new ArgumentToken
		{
			Type = match.Groups[1].ValueSpan[..colonIdx].Equals("plugin", StringComparison.OrdinalIgnoreCase)
				? ArgumentType.Plugin
				: ArgumentType.PowerShell,
			Value = match.Groups[2].Value
		};
	}

	internal static ArgumentToken ParseValue(string raw)
	{
		if (raw.StartsWith('%'))
		{
			var lower = raw.ToLowerInvariant();
			if (lower == "%clipboard")
				return new ArgumentToken { Type = ArgumentType.Clipboard };
			if (lower == "%highlighted")
				return new ArgumentToken { Type = ArgumentType.Highlighted };

			var colonIdx = raw.IndexOf(':', 1);
			if (colonIdx >= 0)
			{
				var prefix = raw[1..colonIdx].ToLowerInvariant();
				var value = raw[(colonIdx + 1)..];
				var type = prefix switch
				{
					"plugin" => (ArgumentType?)ArgumentType.Plugin,
					"ps" or "powershell" => ArgumentType.PowerShell,
					_ => null
				};
				if (type != null)
				{
					if (string.IsNullOrEmpty(value))
						throw new ArgumentException($"Missing value after %{prefix}:");
					return new ArgumentToken { Type = type.Value, Value = value };
				}
			}
		}
		return new ArgumentToken { Type = ArgumentType.Text, Value = raw };
	}

	internal static (string Key, CommandArgument Arg) ParseArgSpec(string spec, bool powerShell = false)
	{
		var colonIdx = spec.IndexOf(':');
		if (colonIdx < 0)
			throw new ArgumentException($"Invalid argument format (missing ':'): {spec}");

		var arg = powerShell
			? ParsePowerShellArg(spec[(colonIdx + 1)..])
			: ParsePluginArg(spec[(colonIdx + 1)..]);
		return (spec[..colonIdx], arg);
	}

	internal static Dictionary<string, CommandArgument[]> GroupArgSpecs(string[] args, bool powerShell = false)
		=> args.Select(s => ParseArgSpec(s, powerShell))
			.GroupBy(x => x.Key, x => x.Arg)
			.ToDictionary(g => g.Key, g => g.ToArray());

	/// <summary>
	/// Parses an --action value, accepting both %type:value and type:value formats.
	/// </summary>
	internal static ArgumentToken ParseAction(string raw)
	{
		if (raw.StartsWith('%'))
			return ParseValue(raw);

		var colonIdx = raw.IndexOf(':');
		var prefix = colonIdx >= 0 ? raw[..colonIdx].ToLowerInvariant() : raw.ToLowerInvariant();

		// Valueless types — support bare keyword and keyword: forms
		if (prefix is "clipboard" or "highlighted")
			return ParseValue($"%{prefix}");

		if (colonIdx < 0)
			throw new ArgumentException($"Invalid action format: {raw}");

		return ParseValue($"%{raw}");
	}
}