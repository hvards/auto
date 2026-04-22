using System.CommandLine;
using System.IO;

using Auto.Cli.Commands;
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
		return await BuildCli(InitializeServiceProvider()).Parse(args).InvokeAsync();
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
		var configDirOption = new Option<string>("--config-dir")
		{
			DefaultValueFactory = _ => Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config", "auto"),
			Description = "Configuration directory path",
			Recursive = true,
			Hidden = true
		};

		CommandStore ResolveStore(ParseResult pr) =>
			new(pr.GetValue(configDirOption) ?? string.Empty);

		var triggerCreator = serviceProvider.GetRequiredService<ITriggerCreator>();
		var keyRecorder = serviceProvider.GetRequiredService<IKeyRecorder>();
		var pluginLoader = serviceProvider.GetRequiredService<IPluginLoader>();
		var commandExecutor = serviceProvider.GetRequiredService<Commands.ICommandExecutor>();

		var rootCommand = new RootCommand("Auto");
		rootCommand.Options.Add(configDirOption);

		rootCommand.Subcommands.Add(ListCommand.Create(ResolveStore));
		rootCommand.Subcommands.Add(GetCommand.Create(ResolveStore, pluginLoader));
		rootCommand.Subcommands.Add(AddCommand.Create(ResolveStore, triggerCreator));
		rootCommand.Subcommands.Add(EditCommand.Create(ResolveStore, triggerCreator));
		rootCommand.Subcommands.Add(ActionCommand.Create(ResolveStore, pluginLoader));
		rootCommand.Subcommands.Add(DeleteCommand.Create(ResolveStore));
		rootCommand.Subcommands.Add(EnableDisableCommand.CreateEnable(ResolveStore));
		rootCommand.Subcommands.Add(EnableDisableCommand.CreateDisable(ResolveStore));
		rootCommand.Subcommands.Add(ExecuteCommand.Create(ResolveStore, commandExecutor));
		rootCommand.Subcommands.Add(ListPluginsCommand.Create(pluginLoader));
		rootCommand.Subcommands.Add(ListKeysCommand.Create());
		rootCommand.Subcommands.Add(RecordInputCommand.Create(keyRecorder));
		rootCommand.Subcommands.Add(StartCommand.Create());
		rootCommand.Subcommands.Add(StopCommand.Create());

		return rootCommand;
	}

	internal static void StartService()
	{
		var serviceProvider = InitializeServiceProvider();
		_ = serviceProvider.GetRequiredService<KeyListener>();
		Application.Run();
	}

	internal static IServiceProvider InitializeServiceProvider()
		=> CreateServiceCollection().BuildServiceProvider();

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