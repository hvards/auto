using Auto.Commands;
using Auto.Models;
using Auto.PluginUtils;
using Auto.Tasks;
using Moq;

namespace UnitTests.Commands;

[TestFixture]
public class CommandExecutorTests
{
	private Mock<IPluginExecutor> _pluginExecutorMock;
	private Mock<IPowerShell> _powerShellMock;
	private CommandExecutor _subject;

	[SetUp]
	public void SetUp()
	{
		_pluginExecutorMock = new Mock<IPluginExecutor>();
		_powerShellMock = new Mock<IPowerShell>();
		_subject = new CommandExecutor(_pluginExecutorMock.Object, _powerShellMock.Object);
	}

	[Test]
	public void ExecuteCommand_ShouldReturnClipboardText_WhenArgumentTypeIsClipboard()
	{
		var command = new Command
		{
			Actions = [new ArgumentToken { Type = ArgumentType.Clipboard }]
		};
		var result = _subject.ExecuteCommand(command, clipboard: "clipboard text");

		Assert.That(result, Has.Count.EqualTo(1));
		Assert.That(result[0], Is.EqualTo("clipboard text"));
	}

	[Test]
	public void ExecuteCommand_ShouldReturnHighlightedText_WhenArgumentTypeIsHighlighted()
	{
		var command = new Command
		{
			Actions = [new ArgumentToken { Type = ArgumentType.Highlighted }]
		};
		var result = _subject.ExecuteCommand(command, highlighted: "highlighted text");

		Assert.That(result, Has.Count.EqualTo(1));
		Assert.That(result[0], Is.EqualTo("highlighted text"));
	}

	[Test]
	public void ExecuteCommand_ShouldExecutePowerShell_WhenArgumentTypeIsPowerShell()
	{
		var command = new Command
		{
			Actions = [new ArgumentToken { Type = ArgumentType.PowerShell, Value = "script" }],
			PowerShellArguments = []
		};
		_powerShellMock.Setup(x => x.Execute("script", It.IsAny<List<(string, string)>>()))
			.Returns("powershell result");

		var result = _subject.ExecuteCommand(command);

		Assert.That(result, Has.Count.EqualTo(1));
		Assert.That(result[0], Is.EqualTo("powershell result"));
	}

	[Test]
	public void ExecuteCommand_ShouldExecutePlugin_WhenArgumentTypeIsPlugin()
	{
		var command = new Command
		{
			Actions = [new ArgumentToken { Type = ArgumentType.Plugin, Value = "plugin" }],
			PluginArguments = new Dictionary<string, CommandArgument[]>
			{
				{"plugin", []}
			}
		};
		_pluginExecutorMock.Setup(x => x.ExecutePlugin("plugin", It.IsAny<IEnumerable<object>>()))
			.Returns("plugin result");

		var result = _subject.ExecuteCommand(command);

		Assert.That(result, Has.Count.EqualTo(1));
		Assert.That(result[0], Is.EqualTo("plugin result"));
	}

	[Test]
	public void ExecuteCommand_ShouldReturnText_WhenArgumentTypeIsText()
	{
		var command = new Command
		{
			Actions = [new ArgumentToken { Type = ArgumentType.Text, Value = "text" }]
		};

		var result = _subject.ExecuteCommand(command);

		Assert.That(result, Has.Count.EqualTo(1));
		Assert.That(result[0], Is.EqualTo("text"));
	}
}