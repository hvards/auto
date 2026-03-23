using System.Windows.Forms;

using Auto.Models;

namespace UnitTests.Commands;

[TestFixture]
internal class TriggerTests
{
	[TestCase(Keys.A, Keys.B)]
	[TestCase(Keys.A, Keys.A, Keys.B)]
	[TestCase(Keys.B, Keys.A, Keys.B)]
	public void IsMacroOrKeyComboPressed_ShouldReturnTrue_IfMacroPressed(params Keys[] keys)
	{
		// Arrange
		var command = new Command
		{
			Trigger = new Trigger
			{
				Sequence = [(ushort)Keys.A, (ushort)Keys.B, (ushort)Keys.C]
			}
		};

		// Act & Assert
		foreach (var key in keys)
			Assert.That(command.Trigger.Check([], (ushort)key), Is.False);
		Assert.That(command.Trigger.Check([], (ushort)Keys.C), Is.True);
	}

	[TestCase(Keys.A, Keys.C)]
	[TestCase(Keys.A, Keys.B, Keys.B, Keys.C)]
	public void IsMacroOrKeyComboPressed_ShouldReturnFalse_IfMacroNotPressed(params Keys[] keys)
	{
		// Arrange
		var command = new Command
		{
			Trigger = new Trigger
			{
				Sequence = [(ushort)Keys.A, (ushort)Keys.B, (ushort)Keys.C]
			}
		};

		// Act & Assert
		foreach (var key in keys)
			Assert.That(command.Trigger.Check([], (ushort)key), Is.False);
	}

	[Test]
	public void IsMacroOrKeyComboPressed_ShouldReturnTrue_IfKeyCombinationPressed()
	{
		// Arrange
		var combination = new HashSet<ushort> { (ushort)Keys.A, (ushort)Keys.B, (ushort)Keys.ControlKey };
		var command = new Command { Trigger = new Trigger { Combination = combination } };

		// Act
		var result = command.Trigger.Check(combination, 0);

		// Assert
		Assert.That(result, Is.True);
	}

	[TestCase(Keys.A, Keys.ControlKey)]
	[TestCase(Keys.A, Keys.B, Keys.ControlKey, Keys.C)]
	[TestCase(Keys.C, Keys.D, Keys.E)]
	public void IsMacroOrKeyComboPressed_ShouldReturnFalse_IfKeyCombinationNotPressed(params Keys[] keys)
	{
		// Arrange
		var combination = new HashSet<ushort> { (ushort)Keys.A, (ushort)Keys.B, (ushort)Keys.ControlKey };
		var command = new Command { Trigger = new Trigger { Combination = combination } };

		// Act
		var result = command.Trigger.Check([.. keys.Select(x => (ushort)x)], 0);

		// Assert
		Assert.That(result, Is.False);
	}
}