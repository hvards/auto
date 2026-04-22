using Auto.Cli.Commands;
using Auto.Cli.Services;

using Microsoft.Extensions.DependencyInjection;

namespace Auto.Cli;

internal static class CliServiceCollectionExtensions
{
	public static IServiceCollection AddCliCommands(this IServiceCollection services)
	{
		services.AddSingleton<ICommandStoreFactory, CommandStoreFactory>();

		services.AddSingleton<ICliCommand, ListCommand>();
		services.AddSingleton<ICliCommand, GetCommand>();
		services.AddSingleton<ICliCommand, AddCommand>();
		services.AddSingleton<ICliCommand, EditCommand>();
		services.AddSingleton<ICliCommand>(sp => new EnableDisableCommand(sp.GetRequiredService<ICommandStoreFactory>(), enable: true));
		services.AddSingleton<ICliCommand>(sp => new EnableDisableCommand(sp.GetRequiredService<ICommandStoreFactory>(), enable: false));
		services.AddSingleton<ICliCommand, ActionCommand>();
		services.AddSingleton<ICliCommand, DeleteCommand>();
		services.AddSingleton<ICliCommand, ExecuteCommand>();
		services.AddSingleton<ICliCommand, ListPluginsCommand>();
		services.AddSingleton<ICliCommand, ListKeysCommand>();
		services.AddSingleton<ICliCommand, RecordInputCommand>();
		services.AddSingleton<ICliCommand, StartCommand>();
		services.AddSingleton<ICliCommand, StopCommand>();

		services.AddSingleton<ActionAddCommand>();
		services.AddSingleton<ActionEditCommand>();
		services.AddSingleton<ActionDeleteCommand>();

		return services;
	}
}
