using Auto.Models;
using Auto.PluginUtils;

namespace Auto.Cli.Services;

internal class PluginHelper
{
	internal static Dictionary<string, CommandArgument[]> GroupPluginArgs(string[] args)
	{
		if (args.Length == 0) return [];

		var pluginId = PluginLoader.ResolvePlugin(args[0]);

		var values = args[1..].Select(ArgParser.ParsePluginArg).ToArray();
		return new Dictionary<string, CommandArgument[]> { [pluginId] = values };
	}

	internal static Dictionary<string, CommandArgument[]> GroupPsArgs(string[] psArgs)
	{
		if (psArgs.Length == 0) return [];

		var name = psArgs[0];
		var arguments = psArgs[1..];

		var values = new CommandArgument[arguments.Length / 2];
		for (var i = 0; i < arguments.Length; i += 2)
		{
			var arg = ArgParser.ParsePluginArg(arguments[i + 1]);
			arg.ParameterName = arguments[i];
			values[i / 2] = arg;
		}
		return new Dictionary<string, CommandArgument[]> { [name] = values };
	}
}
