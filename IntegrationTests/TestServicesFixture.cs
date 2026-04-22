using Auto;
using Auto.Cli;
using Auto.PluginUtils;

using IntegrationTests.Stubs;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace IntegrationTests;

[SetUpFixture]
internal class TestServicesFixture
{
	public static ServiceProvider Services { get; private set; } = null!;

	[OneTimeSetUp]
	public void OneTimeSetUp()
	{
		Services = Program.CreateServiceCollection()
			.AddCliCommands()
			.Replace(ServiceDescriptor.Singleton<IPluginLoader, PluginLoaderStub>())
			.BuildServiceProvider();
	}

	[OneTimeTearDown]
	public void OneTimeTearDown()
	{
		Services.Dispose();
	}
}
