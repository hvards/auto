using Auto;
using System.Windows.Forms;
using Auto.Command;
using Command = Auto.Command.Command;

namespace UnitTests;

[TestFixture]
public class TriggerTests
{
    [TestCase(Keys.A, Keys.B)]
    [TestCase(Keys.A, Keys.A, Keys.B)]
    [TestCase(Keys.B, Keys.A, Keys.B)]
    public void IsMacroOrKeyComboPressed_ShouldReturnTrue_IfMacroPressed(params Keys[] keys)
    {
        var command = new Command
            { Trigger = new Trigger { Sequence = new[] { (ushort)Keys.A, (ushort)Keys.B, (ushort)Keys.C } } };
        foreach (var key in keys)
            Assert.That(command.Trigger.Check(new HashSet<ushort>(), (ushort)key), Is.False);
        Assert.That(command.Trigger.Check(new HashSet<ushort>(), (ushort)Keys.C), Is.True);
    }
    
    [TestCase(Keys.A, Keys.C)]
    [TestCase(Keys.A, Keys.B, Keys.B, Keys.C)]
    public void IsMacroOrKeyComboPressed_ShouldReturnFalse_IfMacroNotPressed(params Keys[] keys)
    {
        var command = new Command
            { Trigger = new Trigger { Sequence = new[] { (ushort)Keys.A, (ushort)Keys.B, (ushort)Keys.C } } };
        foreach (var key in keys)
            Assert.That(command.Trigger.Check(new HashSet<ushort>(), (ushort)key), Is.False);
    }

    [Test]
    public void IsMacroOrKeyComboPressed_ShouldReturnTrue_IfKeyCombinationPressed()
    {
        var combination = new HashSet<ushort> { (ushort)Keys.A, (ushort)Keys.B, (ushort)Keys.ControlKey };
        var command = new Command { Trigger = new Trigger { Combination = combination } };

        Assert.That(command.Trigger.Check(combination, 0), Is.True);
    }

    [TestCase(Keys.A, Keys.ControlKey)]
    [TestCase(Keys.A, Keys.B, Keys.ControlKey, Keys.C)]
    [TestCase(Keys.C, Keys.D, Keys.E)]
    public void IsMacroOrKeyComboPressed_ShouldReturnFalse_IfKeyCombinationNotPressed(params Keys[] keys)
    {
        var combination = new HashSet<ushort> { (ushort)Keys.A, (ushort)Keys.B, (ushort)Keys.ControlKey };
        var command = new Command { Trigger = new Trigger { Combination = combination } };

        Assert.That(command.Trigger.Check(keys.Select(x => (ushort)x).ToHashSet(), 0),
            Is.False);
    }
}
