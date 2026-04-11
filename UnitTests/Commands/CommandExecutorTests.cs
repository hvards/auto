using Auto.Commands;
using Auto.Models;
using Auto.PluginUtils;

using Moq;

namespace UnitTests.Commands;

[TestFixture]
internal class CommandExecutorTests
{
	private Mock<IPluginExecutor> _pluginExecutorMock = null!;
	private CommandExecutor _subject = null!;

	private static Command GetCommandWithActions(params CommandAction[] actions) => new()
	{
		Actions = actions
	};

	[SetUp]
	public void SetUp()
	{
		_pluginExecutorMock = new Mock<IPluginExecutor>();
		_subject = new CommandExecutor(_pluginExecutorMock.Object);
	}

	[TestCase("Clipboard")]
	[TestCase("Highlighted")]
	public void ExecuteCommand_ShouldReturnVariableText_WhenVariableIsPredefined(string variableName)
	{
		// Arrange
		var command = GetCommandWithActions(new CommandAction
		{
			Target = "plugin",
			Order = 0,
			Arguments = [new CommandArgument {
				Tokens = [new ArgumentToken { Type = ArgumentType.Variable, Value = variableName }]
			}]
		});
		_pluginExecutorMock.Setup(x => x.ExecutePlugin("plugin", It.IsAny<IEnumerable<object?>>()))
			.Returns((string _, IEnumerable<object?> args) => args.First());

		// Act
		var result = _subject.ExecuteCommand(command, clipboard: "clipboard text", highlighted: "highlighted text");

		// Assert
		Assert.That(result, Has.Count.EqualTo(1));
		Assert.That(result[0], Is.EqualTo($"{variableName.ToLowerInvariant()} text"));
	}

	[Test]
	public void ExecuteCommand_ShouldExecutePlugin()
	{
		// Arrange
		var command = GetCommandWithActions(new CommandAction
		{
			Target = "plugin",
			Order = 0,
			Arguments = []
		});
		_pluginExecutorMock.Setup(x => x.ExecutePlugin("plugin", It.IsAny<IEnumerable<object?>>()))
			.Returns("plugin result");

		// Act
		var result = _subject.ExecuteCommand(command);

		// Assert
		Assert.That(result, Has.Count.EqualTo(1));
		Assert.That(result[0], Is.EqualTo("plugin result"));
	}

	[Test]
	public void ExecuteCommand_ShouldReturnText()
	{
		// Arrange
		var command = GetCommandWithActions(new CommandAction
		{
			Target = "plugin",
			Order = 0,
			Arguments = [
				new CommandArgument { Tokens = [new ArgumentToken { Type = ArgumentType.Text, Value = "text" }] }
			]
		});
		_pluginExecutorMock.Setup(x => x.ExecutePlugin("plugin", It.IsAny<IEnumerable<object?>>()))
			.Returns((string _, IEnumerable<object?> args) => args.First());

		// Act
		var result = _subject.ExecuteCommand(command);

		// Assert
		Assert.That(result, Has.Count.EqualTo(1));
		Assert.That(result[0], Is.EqualTo("text"));
	}

	[Test]
	public void ExecuteCommand_VariablePropagation_PassesResultToNextAction()
	{
		// Arrange
		var command = GetCommandWithActions(new CommandAction
		{
			Target = "pluginA",
			Order = 0,
			Variable = "Step1",
			Arguments = [
				new CommandArgument { Tokens = [new ArgumentToken { Type = ArgumentType.Text, Value = "input" }] }
			]
		}, new CommandAction
		{
			Target = "pluginB",
			Order = 1,
			Arguments = [
				new CommandArgument { Tokens = [new ArgumentToken { Type = ArgumentType.Variable, Value = "Step1" }] }
			]
		});
		_pluginExecutorMock.Setup(x => x.ExecutePlugin("pluginA", It.IsAny<IEnumerable<object?>>()))
			.Returns("intermediate");
		_pluginExecutorMock.Setup(x => x.ExecutePlugin("pluginB", It.IsAny<IEnumerable<object?>>()))
			.Returns((string _, IEnumerable<object?> args) => args.First());

		// Act
		var result = _subject.ExecuteCommand(command);

		// Assert
		Assert.That(result, Has.Count.EqualTo(2));
		Assert.That(result[0], Is.EqualTo("intermediate"));
		Assert.That(result[1], Is.EqualTo("intermediate"));
	}

	[Test]
	public void ExecuteCommand_SingleVariableToken_PassesRawObject()
	{
		// Arrange
		var command = GetCommandWithActions(new CommandAction
		{
			Target = "pluginA",
			Order = 0,
			Variable = "Result",
			Arguments = []
		}, new CommandAction
		{
			Target = "pluginB",
			Order = 1,
			Arguments = [
				new CommandArgument { Tokens = [new ArgumentToken { Type = ArgumentType.Variable, Value = "Result" }] }
			]
		});
		_pluginExecutorMock.Setup(x => x.ExecutePlugin("pluginA", It.IsAny<IEnumerable<object?>>()))
			.Returns(42);
		object? capturedArg = null;
		_pluginExecutorMock.Setup(x => x.ExecutePlugin("pluginB", It.IsAny<IEnumerable<object?>>()))
			.Callback((string _, IEnumerable<object?> args) => capturedArg = args.First());

		// Act
		_subject.ExecuteCommand(command);

		// Assert
		Assert.That(capturedArg, Is.EqualTo(42));
	}

	[Test]
	public void ExecuteCommand_ExecutesInOrderAscending()
	{
		// Arrange
		var executionOrder = new List<string>();
		var command = GetCommandWithActions(
			new CommandAction { Target = "second", Order = 1, Arguments = [] },
			new CommandAction { Target = "first", Order = 0, Arguments = [] }
		);
		_pluginExecutorMock.Setup(x => x.ExecutePlugin(It.IsAny<string>(), It.IsAny<IEnumerable<object?>>()))
			.Callback((string id, IEnumerable<object?> _) => executionOrder.Add(id));

		// Act
		_subject.ExecuteCommand(command);

		// Assert
		Assert.That(executionOrder, Is.EqualTo(["first", "second"]));
	}
}