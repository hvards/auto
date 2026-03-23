using Auto.Models;

namespace Auto.Cli.Services;

internal static class ActionValidator
{
	private static readonly HashSet<string> PredefinedVariables = new(["Clipboard", "Highlighted"]);

	internal static Dictionary<CommandAction, int> ComputeOrder(CommandAction[] actions)
	{
		var duplicates = actions
			.Where(a => a.Variable != null)
			.GroupBy(a => a.Variable)
			.Where(g => g.Count() > 1)
			.Select(g => g.Key)
			.ToList();

		if (duplicates.Count > 0)
			throw new ArgumentException($"Duplicate variable names: {string.Join(", ", duplicates)}");

		var availableVariables = new HashSet<string>(PredefinedVariables);
		var remainingActions = new List<CommandAction>(actions);
		var result = new Dictionary<CommandAction, int>();
		var order = 0;

		while (remainingActions.Count > 0)
		{
			var ready = remainingActions
				.Where(a => GetReferencedVariables(a).All(availableVariables.Contains))
				.ToList();

			if (ready.Count == 0)
			{
				var missing = remainingActions
					.SelectMany(GetReferencedVariables)
					.First(v => !availableVariables.Contains(v));
				throw new ArgumentException(
					$"Variable '{missing}' is not available. " +
					$"It must be produced by another action, " +
					$"or be a predefined variable (Clipboard, Highlighted).");
			}

			foreach (var a in ready)
			{
				result[a] = order;
				if (a.Variable != null)
					availableVariables.Add(a.Variable);
			}

			remainingActions.RemoveAll(ready.Contains);
			order++;
		}

		return result;
	}

	private static IEnumerable<string> GetReferencedVariables(CommandAction action) =>
		action.Arguments
			.SelectMany(a => a.Tokens)
			.Where(t => t.Type == ArgumentType.Variable && !PredefinedVariables.Contains(t.Value))
			.Select(t => t.Value);
}