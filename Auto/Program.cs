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

namespace Auto;

internal static class Program
{
	private static async Task<int> Main(string[] args)
	{
		Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
		Application.EnableVisualStyles();
		Application.SetCompatibleTextRenderingDefault(false);
		return await BuildCli().Parse(args).InvokeAsync();
	}

	internal static RootCommand BuildCli()
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

		var serviceProvider = InitializeCliServiceProvider();
		var triggerCreator = serviceProvider.GetRequiredService<ITriggerCreator>();
		var keyRecorder = serviceProvider.GetRequiredService<IKeyRecorder>();

		var rootCommand = new RootCommand("Auto");
		rootCommand.Options.Add(configDirOption);

		rootCommand.Subcommands.Add(ListCommand.Create(ResolveStore));
		rootCommand.Subcommands.Add(GetCommand.Create(ResolveStore));
		rootCommand.Subcommands.Add(AddCommand.Create(ResolveStore, triggerCreator));
		rootCommand.Subcommands.Add(EditCommand.Create(ResolveStore, triggerCreator));
		rootCommand.Subcommands.Add(ActionCommand.Create(ResolveStore));
		rootCommand.Subcommands.Add(DeleteCommand.Create(ResolveStore));
		rootCommand.Subcommands.Add(EnableDisableCommand.CreateEnable(ResolveStore));
		rootCommand.Subcommands.Add(EnableDisableCommand.CreateDisable(ResolveStore));
		rootCommand.Subcommands.Add(ExecuteCommand.Create(ResolveStore));
		rootCommand.Subcommands.Add(ListPluginsCommand.Create());
		rootCommand.Subcommands.Add(ListKeysCommand.Create());
		rootCommand.Subcommands.Add(RecordInputCommand.Create(keyRecorder));
		rootCommand.Subcommands.Add(StartCommand.Create());

		return rootCommand;
	}

	internal static void StartService()
	{
		var serviceProvider = InitializeServiceProvider();
		_ = serviceProvider.GetRequiredService<KeyListener>();
		Application.Run();
	}

	internal static IServiceProvider InitializeCliServiceProvider()
	{
		var services = CreateBaseServiceCollection();
		services.AddSingleton<IKeyRecorder, KeyRecorder>();
		services.AddSingleton<ITriggerCreator, TriggerCreator>();
		return services.BuildServiceProvider();
	}

	internal static IServiceProvider InitializeServiceProvider()
	{
		var services = CreateBaseServiceCollection();
		services.AddLogging(config => config.AddSerilog());
		services.AddSingleton<KeyListener>();
		services.AddSingleton<ICommandProvider, CommandProvider>();
		services.AddSingleton<IExecute, Execute>();
		services.AddSingleton<IClipboardHandler, ClipboardHandler>();
		services.AddSingleton<ISendInput, SendInput>();
		services.AddSingleton<IKeyboardHandler, KeyboardHandler>();
		services.AddSingleton<IPluginLoader, PluginLoader>();
		services.AddSingleton<IPluginExecutor, PluginExecutor>();
		services.AddSingleton<Commands.ICommandExecutor, CommandExecutor>();
		return services.BuildServiceProvider();
	}

	private static ServiceCollection CreateBaseServiceCollection()
	{
		var serviceCollection = new ServiceCollection();
		var configuration = new ConfigurationBuilder()
			.SetBasePath(AppContext.BaseDirectory)
			.AddJsonFile("appsettings.json")
			.Build();
		Log.Logger = new LoggerConfiguration()
			.ReadFrom.Configuration(configuration)
			.CreateLogger();
		serviceCollection.AddSingleton<INativeMethods, NativeMethods>();
		return serviceCollection;
	}
}