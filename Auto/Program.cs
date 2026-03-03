using Auto.Command;
using Auto.Handlers;
using Auto.Native;
using Auto.PluginUtils;
using Auto.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Serilog;

namespace Auto;

public static class Program
{
	private static void Main()
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
		services.AddSingleton<Command.ICommandExecutor, CommandExecutor>();
		services.AddSingleton<IPowerShell, PowerShell>();
		services.AddSingleton<INativeMethods, NativeMethods>();
	}
}