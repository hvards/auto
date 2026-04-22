using System.CommandLine;

using Auto.Cli;
using Auto.Cli.Services;
using Auto.Commands;
using Auto.Handlers;
using Auto.Native;
using Auto.PluginUtils;
using Auto.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Serilog;
using Serilog.Events;

namespace Auto;

internal static class Program
{
	private static async Task<int> Main(string[] args)
	{
		Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
		Application.EnableVisualStyles();
		Application.SetCompatibleTextRenderingDefault(false);
		ConfigureConsoleLogger();
		var serviceProvider = CreateServiceCollection().AddCliCommands().BuildServiceProvider();
		return await BuildCli(serviceProvider).Parse(args).InvokeAsync();
	}

	internal static void ConfigureConsoleLogger()
	{
		Log.Logger = new LoggerConfiguration()
			.MinimumLevel.Verbose()
			.WriteTo.Console(
				restrictedToMinimumLevel: LogEventLevel.Information,
				standardErrorFromLevel: LogEventLevel.Verbose,
				outputTemplate: "[{Level:u3}] {Message:lj}{NewLine}{Exception}")
			.CreateLogger();
	}

	internal static void ConfigureFileLogger()
	{
		var configuration = new ConfigurationBuilder()
			.SetBasePath(AppContext.BaseDirectory)
			.AddJsonFile("appsettings.json")
			.Build();
		Log.Logger = new LoggerConfiguration()
			.ReadFrom.Configuration(configuration)
			.CreateLogger();
	}

	internal static RootCommand BuildCli(IServiceProvider serviceProvider)
	{
		var rootCommand = new RootCommand("Auto");
		rootCommand.Options.Add(serviceProvider.GetRequiredService<ICommandStoreFactory>().ConfigDirOption);

		foreach (var cliCommand in serviceProvider.GetServices<ICliCommand>())
			rootCommand.Subcommands.Add(cliCommand.Build());

		return rootCommand;
	}

	internal static void StartService()
	{
		var serviceProvider = CreateServiceCollection().BuildServiceProvider();
		_ = serviceProvider.GetRequiredService<KeyListener>();
		Application.Run();
	}

	internal static IServiceCollection CreateServiceCollection()
	{
		var services = new ServiceCollection();
		services.AddSingleton<INativeMethods, NativeMethods>();
		services.AddLogging(config => config.AddSerilog());
		services.AddSingleton<IKeyRecorder, KeyRecorder>();
		services.AddSingleton<ITriggerCreator, TriggerCreator>();
		services.AddSingleton<IPluginLoader, PluginLoader>();
		services.AddSingleton<KeyListener>();
		services.AddSingleton<ICommandProvider, CommandProvider>();
		services.AddSingleton<IExecute, Execute>();
		services.AddSingleton<IClipboardHandler, ClipboardHandler>();
		services.AddSingleton<ISendInput, SendInput>();
		services.AddSingleton<IKeyboardHandler, KeyboardHandler>();
		services.AddSingleton<IPluginExecutor, PluginExecutor>();
		services.AddSingleton<Commands.ICommandExecutor, CommandExecutor>();
		return services;
	}
}
