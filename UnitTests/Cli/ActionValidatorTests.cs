using Auto.Cli.Services;
using Auto.Models;

namespace UnitTests.Cli;

[TestFixture]
internal class ActionValidatorTests
{
	[Test]
	public void ComputeOrder_EmptyActions_ReturnsEmptyDict()
	{
		// Act
		var result = ActionValidator.ComputeOrder([]);

		// Assert
		Assert.That(result, Is.Empty);
	}

	[TestCase("Clipboard")]
	[TestCase("Highlighted")]
	public void ComputeOrder_PredefinedVariables_OrderZero(string variableName)
	{
		// Arrange
		var actions = new[]
		{
			new CommandAction
			{
				Type = ActionType.Plugin, Target = "plugin",
				Arguments = [new CommandArgument {
					Tokens = [new ArgumentToken { Type = ArgumentType.Variable, Value = variableName }]
				}]
			}
		};

		// Act
		var orders = ActionValidator.ComputeOrder(actions);

		// Assert
		Assert.That(orders[actions[0]], Is.Zero);
	}

	[Test]
	public void ComputeOrder_UndefinedVariable_Throws()
	{
		// Arrange
		var actions = new[]
		{
			new CommandAction
			{
				Type = ActionType.Plugin, Target = "pluginA",
				Arguments = [new CommandArgument {
					Tokens = [new ArgumentToken { Type = ArgumentType.Variable, Value = "unknown" }]
				}]
			}
		};

		// Act & assert
		Assert.That(() => ActionValidator.ComputeOrder(actions), Throws.TypeOf<ArgumentException>()
			.With.Message.Contains("unknown"));
	}

	[Test]
	public void ComputeOrder_DuplicateVariableNames_Throws()
	{
		// Arrange
		var actions = new[]
		{
			new CommandAction
			{
				Type = ActionType.Plugin, Target = "pluginA",
				Variable = "Result",
				Arguments = []
			},
			new CommandAction
			{
				Type = ActionType.Plugin, Target = "pluginB",
				Variable = "Result",
				Arguments = []
			}
		};

		// Act & assert
		Assert.That(() => ActionValidator.ComputeOrder(actions), Throws.TypeOf<ArgumentException>()
			.With.Message.Contains("Duplicate variable names"));
	}

	[Test]
	public void ComputeOrder_TextTokens_NeverValidated()
	{
		// Arrange
		var actions = new[]
		{
			new CommandAction
			{
				Type = ActionType.Plugin, Target = "plugin",
				Arguments = [new CommandArgument { Tokens = [new ArgumentToken { Type = ArgumentType.Text, Value = "anything" }] }]
			}
		};

		// Act & assert
		Assert.DoesNotThrow(() => ActionValidator.ComputeOrder(actions));
	}

	[Test]
	public void ComputeOrder_ProducerBeforeConsumer()
	{
		// Arrange
		var actions = new[]
		{
			new CommandAction
			{
				Type = ActionType.Plugin, Target = "consumer",
				Arguments = [new CommandArgument { Tokens = [new ArgumentToken { Type = ArgumentType.Variable, Value = "Result" }] }]
			},
			new CommandAction
			{
				Type = ActionType.Plugin, Target = "producer",
				Variable = "Result",
				Arguments = []
			}
		};

		// Act
		var orders = ActionValidator.ComputeOrder(actions);

		// Assert
		Assert.That(orders[actions[1]], Is.LessThan(orders[actions[0]]));
	}

	[Test]
	public void ComputeOrder_LinearChain()
	{
		// Arrange
		var actions = new[]
		{
			new CommandAction
			{
				Type = ActionType.Plugin, Target = "A", Variable = "X",
				Arguments = []
			},
			new CommandAction
			{
				Type = ActionType.Plugin, Target = "B", Variable = "Y",
				Arguments = [
					new CommandArgument { Tokens = [new ArgumentToken { Type = ArgumentType.Variable, Value = "X" }] }
				]
			},
			new CommandAction
			{
				Type = ActionType.Plugin, Target = "C",
				Arguments = [
					new CommandArgument { Tokens = [new ArgumentToken { Type = ArgumentType.Variable, Value = "Y" }] }
				]
			}
		};

		// Act
		var orders = ActionValidator.ComputeOrder(actions);

		// Assert
		Assert.That(orders[actions[0]], Is.EqualTo(0));
		Assert.That(orders[actions[1]], Is.EqualTo(1));
		Assert.That(orders[actions[2]], Is.EqualTo(2));
	}

	[Test]
	public void ComputeOrder_ValidForwardChain()
	{
		// Arrange
		var actions = new[]
		{
			new CommandAction
			{
				Type = ActionType.Plugin, Target = "pluginA",
				Variable = "Step1",
				Arguments = []
			},
			new CommandAction
			{
				Type = ActionType.Plugin, Target = "pluginB",
				Variable = "Step2",
				Arguments = [new CommandArgument {
					Tokens = [new ArgumentToken { Type = ArgumentType.Variable, Value = "Step1" }]
				}]
			},
			new CommandAction
			{
				Type = ActionType.Plugin, Target = "pluginC",
				Arguments = [new CommandArgument {
					Tokens = [new ArgumentToken { Type = ArgumentType.Variable, Value = "Step2" }]
				}]
			}
		};

		// Act
		var orders = ActionValidator.ComputeOrder(actions);

		// Assert
		Assert.That(orders[actions[0]], Is.LessThan(orders[actions[1]]));
		Assert.That(orders[actions[1]], Is.LessThan(orders[actions[2]]));
	}

	[Test]
	public void ComputeOrder_Diamond()
	{
		// Arrange
		var actions = new[]
		{
			new CommandAction
			{
				Type = ActionType.Plugin, Target = "A", Variable = "X",
				Arguments = []
			},
			new CommandAction
			{
				Type = ActionType.Plugin, Target = "B", Variable = "Y",
				Arguments = [
					new CommandArgument { Tokens = [new ArgumentToken { Type = ArgumentType.Variable, Value = "X" }] }
				]
			},
			new CommandAction
			{
				Type = ActionType.Plugin, Target = "C", Variable = "Z",
				Arguments = [
					new CommandArgument { Tokens = [new ArgumentToken { Type = ArgumentType.Variable, Value = "X" }] }
				]
			},
			new CommandAction
			{
				Type = ActionType.Plugin, Target = "D",
				Arguments =
				[
					new CommandArgument { Tokens = [new ArgumentToken { Type = ArgumentType.Variable, Value = "Y" }] },
					new CommandArgument { Tokens = [new ArgumentToken { Type = ArgumentType.Variable, Value = "Z" }] }
				]
			}
		};

		// Act
		var orders = ActionValidator.ComputeOrder(actions);

		// Assert
		Assert.That(orders[actions[0]], Is.EqualTo(0));
		Assert.That(orders[actions[1]], Is.EqualTo(1));
		Assert.That(orders[actions[2]], Is.EqualTo(1));
		Assert.That(orders[actions[3]], Is.EqualTo(2));
	}

	[Test]
	public void ComputeOrder_IndependentActions_AllGetOrderZero()
	{
		// Arrange
		var actions = new[]
		{
			new CommandAction { Type = ActionType.Plugin, Target = "A", Arguments = [] },
			new CommandAction { Type = ActionType.Plugin, Target = "B", Arguments = [] },
			new CommandAction { Type = ActionType.Plugin, Target = "C", Arguments = [] }
		};

		// Act
		var orders = ActionValidator.ComputeOrder(actions);

		// Assert
		Assert.That(orders.Values, Is.All.EqualTo(0));
	}

	[Test]
	public void ComputeOrder_UnreferencedVariable_StaysAtZero()
	{
		// Arrange
		var actions = new[]
		{
			new CommandAction
			{
				Type = ActionType.Plugin, Target = "A", Variable = "X",
				Arguments = []
			},
			new CommandAction
			{
				Type = ActionType.Plugin, Target = "B",
				Arguments = [new CommandArgument {
					Tokens = [new ArgumentToken { Type = ArgumentType.Text, Value = "literal" }]
				}]
			}
		};

		// Act
		var orders = ActionValidator.ComputeOrder(actions);

		// Assert
		Assert.That(orders[actions[0]], Is.EqualTo(0));
		Assert.That(orders[actions[1]], Is.EqualTo(0));
	}
}