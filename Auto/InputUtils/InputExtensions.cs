using System.Text.RegularExpressions;

namespace Auto.InputUtils;

internal static partial class InputExtensions
{
	public static IEnumerable<InputToken> GetTokens(this string input)
	{
		foreach (Match match in TokenRegex().Matches(input))
		{
			if (match.Groups["escaped"].Success)
			{
				yield return new InputToken
				{
					Value = match.Groups["escaped"].Value[0].ToString(),
					InputAction = InputAction.NotSet
				};
			}
			else if (match.Groups["brace"].Success)
			{
				var prefix = match.Groups["prefix"].Value;
				var key = match.Groups["key"].Value;
				var action = prefix switch
				{
					"+" => InputAction.Down,
					"-" => InputAction.Up,
					"!" => InputAction.Sleep,
					_ => InputAction.NotSet
				};
				yield return new InputToken
				{
					Value = key,
					InputAction = action
				};
			}
			else
			{
				yield return new InputToken
				{
					Value = match.Groups["char"].Value,
					InputAction = InputAction.NotSet
				};
			}
		}
	}

	[GeneratedRegex(@"(?<escaped>\{\{|\}\})|(?<brace>\{(?<prefix>[+\-!])?(?<key>[^}]*)\})|(?<char>.)",
		RegexOptions.Compiled)]
	private static partial Regex TokenRegex();
}
