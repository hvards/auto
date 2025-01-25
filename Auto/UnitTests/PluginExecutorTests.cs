using Auto.Command;
using Auto.Interfaces;
using Auto.tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace UnitTests;

[TestFixture]
public class PluginExecutorTests
{
	private Mock<IPluginLoader> _pluginLoaderMock;
	private PluginExecutor _subject;

	[SetUp]
	public void SetUp()
	{
		_pluginLoaderMock = new Mock<IPluginLoader>();
		_subject = new PluginExecutor(_pluginLoaderMock.Object, new NullLogger<PluginExecutor>());
	}

	[Test]
	public void ExecutePlugin_PluginNotFound_ReturnsNull()
	{
		_pluginLoaderMock.Setup(pl => pl.CreateCommands()).Returns(new Dictionary<string, Plugin>());
		_subject = new PluginExecutor(_pluginLoaderMock.Object, new NullLogger<PluginExecutor>());

		var result = _subject.ExecutePlugin("nonexistent", new List<object>());

		Assert.That(result, Is.Null);
	}

	[Test]
	public void ExecutePlugin_PluginFound_ExecutesSuccessfully()
	{
		var plugin = new Plugin
		{
			ArgumentTypes = [],
			Action = _ => "Executed",
			StaThreadRequired = false
		};
		var plugins = new Dictionary<string, Plugin> { { "testPlugin", plugin } };
		_pluginLoaderMock.Setup(pl => pl.CreateCommands()).Returns(plugins);
		_subject = new PluginExecutor(_pluginLoaderMock.Object, new NullLogger<PluginExecutor>());

		var result = _subject.ExecutePlugin("testPlugin", []);

		Assert.That(result, Is.EqualTo("Executed"));
	}

	[Test]
	public void ExecutePlugin_PluginThrowsException_ReturnsNull()
	{
		var plugin = new Plugin
		{
			ArgumentTypes = [],
			Action = _ => throw new Exception("Error"),
			StaThreadRequired = false
		};
		var plugins = new Dictionary<string, Plugin> { { "testPlugin", plugin } };
		_pluginLoaderMock.Setup(pl => pl.CreateCommands()).Returns(plugins);
		_subject = new PluginExecutor(_pluginLoaderMock.Object, new NullLogger<PluginExecutor>());

		var result = _subject.ExecutePlugin("testPlugin", []);

		Assert.That(result, Is.Null);
	}
}