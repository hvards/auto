using System.Text.RegularExpressions;

namespace Auto.InputUtils;

internal static partial class InputExtensions
{
	public static IEnumerable<InputToken> GetTokens(this string input)
	{
		foreach (Match match in TokenRegex().Matches(input))
		{
			if (match.Groups["brace"].Success)
			{
				yield return new InputToken
				{
					Value = match.Groups["key"].Value,
					InputAction = InputAction.NotSet
				};
			}
			else if (match.Groups["bracket"].Success)
			{
				var actionChar = match.Groups["action"].Value;
				var action = actionChar switch
				{
					"!" => InputAction.Sleep,
					"1" => InputAction.Down,
					_ => InputAction.Up
				};
				yield return new InputToken
				{
					Value = match.Groups["key"].Value,
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

	[GeneratedRegex(@"(?<brace>\{(?<key>[^}]*)\})|(?<bracket>\[(?<action>[01!]):(?<key>[^\]]*)\])|(?<char>.)",
		RegexOptions.Compiled)]
	private static partial Regex TokenRegex();
}