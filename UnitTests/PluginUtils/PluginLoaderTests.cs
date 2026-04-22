using Auto.Handlers;
using Auto.PluginUtils;

using AutoContracts;

using Microsoft.Extensions.Logging.Abstractions;

namespace UnitTests.PluginUtils;

[TestFixture]
internal class PluginLoaderTests
{
	private PluginLoader _loader = null!;

	[SetUp]
	public void SetUp() => _loader = new PluginLoader(null!, NullLogger<PluginLoader>.Instance);

	[Test]
	public void ToPlugin_NonSta_ActionWaitsForInit()
	{
		// Arrange
		var command = new FakeCommand { RequiresSta = false, InitHook = () => Thread.Sleep(100) };

		// Act
		var plugin = _loader.ToPlugin(command);
		plugin.Action([]);
		plugin.Action([]);

		// Assert
		Assert.That(command.InitCallCount, Is.EqualTo(1));
	}

	[Test]
	public void ToPlugin_Sta_InitRunsOnSharedStaThread()
	{
		// Arrange
		var command = new FakeCommand { RequiresSta = true };

		// Act
		var plugin = _loader.ToPlugin(command);
		var executeThreadId = StaHandler.Execute(command.Id, () => Environment.CurrentManagedThreadId);

		// Assert
		Assert.That(plugin.StaThreadRequired, Is.True);
		Assert.That(command.InitCallCount, Is.EqualTo(1));
		Assert.That(command.InitApartment, Is.EqualTo(ApartmentState.STA));
		Assert.That(command.InitThreadId, Is.EqualTo(executeThreadId));
	}

	[Test]
	public void ToPlugin_InitThrows_ThreadAlive()
	{
		// Arrange
		var command = new FakeCommand
		{
			RequiresSta = true,
			InitHook = () => throw new InvalidOperationException("init exception")
		};

		// Act
		Assert.DoesNotThrow(() => _loader.ToPlugin(command));
		var result = StaHandler.Execute(command.Id, () => 42);

		// Assert
		Assert.That(result, Is.EqualTo(42));
	}

	private class FakeCommand : ICommand
	{
		public string Name => "Fake";
		public string Description => "Fake";
		public Guid Id { get; } = Guid.NewGuid();
		public Type ReturnType => typeof(object);
		public List<PluginArgument> ExpectedArguments { get; } = [];
		public bool RequiresSta { get; init; }
		public Action? InitHook { get; init; }

		public int InitCallCount { get; private set; }
		public int? InitThreadId { get; private set; }
		public ApartmentState? InitApartment { get; private set; }

		public void Init()
		{
			InitThreadId = Environment.CurrentManagedThreadId;
			InitApartment = Thread.CurrentThread.GetApartmentState();
			InitHook?.Invoke();
			InitCallCount++;
		}

		public object? Execute(object?[] args) => null;
	}
}
