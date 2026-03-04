using Auto.Cli.Commands;
using Auto.Commands;
using Auto.Handlers;
using Auto.Native;
using Auto.PluginUtils;
using Auto.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.CommandLine;
using System.IO;

namespace Auto;

public static class Program
{
	private static async Task<int> Main(string[] args)
	{
		if (args.Length > 0)
			return await RunCli(args);

		StartService();
		return 0;
	}

	private static async Task<int> RunCli(string[] args)
		=> await BuildCli().Parse(args).InvokeAsync();

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

		var rootCommand = new RootCommand("Auto");
		rootCommand.Options.Add(configDirOption);

		rootCommand.Subcommands.Add(ListCommand.Create(configDirOption));
		rootCommand.Subcommands.Add(GetCommand.Create(configDirOption));
		rootCommand.Subcommands.Add(AddCommand.Create(configDirOption));
		rootCommand.Subcommands.Add(EditCommand.Create(configDirOption));
		rootCommand.Subcommands.Add(DeleteCommand.Create(configDirOption));
		rootCommand.Subcommands.Add(EnableDisableCommand.CreateEnable(configDirOption));
		rootCommand.Subcommands.Add(EnableDisableCommand.CreateDisable(configDirOption));
		rootCommand.Subcommands.Add(ListPluginsCommand.Create());
		rootCommand.Subcommands.Add(ListKeysCommand.Create());

		return rootCommand;
	}

	private static void StartService()
	{
		var serviceCollection = new ServiceCollection();
		var configuration = new ConfigurationBuilder()
			.SetBasePath(AppContext.BaseDirectory)
			.AddJsonFile("appsettings.json")
			.Build();
		Log.Logger = new LoggerConfiguration()
			.ReadFrom.Configuration(configuration)
			.CreateLogger();

		ConfigureServices(serviceCollection);

		var serviceProvider = serviceCollection.BuildServiceProvider();
		_ = serviceProvider.GetRequiredService<KeyListener>();
		Application.Run();
	}

	private static void ConfigureServices(IServiceCollection services)
	{
		services.AddLogging(config => { config.AddSerilog(); });

		services.AddSingleton<KeyListener>();
		services.AddSingleton<ICommandProvider, CommandProvider>();
		services.AddSingleton<IExecute, Execute>();
		services.AddSingleton<IClipboardHandler, ClipboardHandler>();
		services.AddSingleton<ISendInput, SendInput>();
		services.AddSingleton<IKeyboardHandler, KeyboardHandler>();
		services.AddSingleton<IPluginLoader, PluginLoader>();
		services.AddSingleton<IPluginExecutor, PluginExecutor>();
		services.AddSingleton<Commands.ICommandExecutor, CommandExecutor>();
		services.AddSingleton<IPowerShell, PowerShell>();
		services.AddSingleton<INativeMethods, NativeMethods>();
	}
}