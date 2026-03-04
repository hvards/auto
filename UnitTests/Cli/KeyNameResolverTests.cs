using Auto.Cli.Services;

namespace UnitTests.Cli;

[TestFixture]
public class KeyNameResolverTests
{
	[TestCase("LControlKey", (ushort)162)]
	[TestCase("LCtrl", (ushort)162)]
	[TestCase("Ctrl", (ushort)162)]
	[TestCase("LMenu", (ushort)164)]
	[TestCase("LAlt", (ushort)164)]
	[TestCase("LShiftKey", (ushort)160)]
	[TestCase("LShift", (ushort)160)]
	[TestCase("LWin", (ushort)91)]
	[TestCase("Win", (ushort)91)]
	[TestCase("A", (ushort)65)]
	[TestCase("F12", (ushort)123)]
	[TestCase("Space", (ushort)32)]
	public void ParseKey_KnownNames(string name, ushort expected)
	{
		Assert.That(KeyNameResolver.ParseKey(name), Is.EqualTo(expected));
	}

	[TestCase("lcontrolkey", (ushort)162)]
	[TestCase("LCTRL", (ushort)162)]
	public void ParseKey_CaseInsensitive(string name, ushort expected)
	{
		// Act
		var result = KeyNameResolver.ParseKey(name);

		// Assert
		Assert.That(result, Is.EqualTo(expected));
	}

	[Test]
	public void ParseKey_NumericFallback()
	{
		Assert.That(KeyNameResolver.ParseKey("200"), Is.EqualTo(200));
	}

	[Test]
	public void ParseKey_Unknown_Throws()
	{
		Assert.Throws<ArgumentException>(() => KeyNameResolver.ParseKey("NonExistentKey"));
	}

	[TestCase((ushort)162, "LControlKey")]
	[TestCase((ushort)164, "LMenu")]
	[TestCase((ushort)91, "LWin")]
	[TestCase((ushort)65, "A")]
	[TestCase((ushort)200, "200")]
	public void FormatKey_ReturnsCanonicalName(ushort code, string expected)
	{
		Assert.That(KeyNameResolver.FormatKey(code), Is.EqualTo(expected));
	}

	[Test]
	public void ParseCombination_MultipleKeys()
	{
		var result = KeyNameResolver.ParseCombination("LCtrl+LWin+LAlt+R");
		Assert.That(result.SetEquals(new HashSet<ushort> { 162, 91, 164, 82 }));
	}

	[Test]
	public void ParseSequence_MultipleKeys()
	{
		var result = KeyNameResolver.ParseSequence("A,S,F,C,E");
		Assert.That(result, Is.EqualTo(new ushort[] { 65, 83, 70, 67, 69 }));
	}

	[Test]
	public void FormatCombination_SortedOutput()
	{
		var combo = new HashSet<ushort> { 164, 91, 162, 82 };
		var result = KeyNameResolver.FormatCombination(combo);
		Assert.That(result, Is.EqualTo("R+LWin+LControlKey+LMenu"));
	}

	[Test]
	public void FormatSequence_PreservesOrder()
	{
		var seq = new ushort[] { 65, 83, 70 };
		var result = KeyNameResolver.FormatSequence(seq);
		Assert.That(result, Is.EqualTo("A,S,F"));
	}

	[Test]
	public void FormatCombination_Empty_ReturnsEmpty()
	{
		// Act
		var result = KeyNameResolver.FormatCombination(new HashSet<ushort>());

		// Assert
		Assert.That(result, Is.EqualTo(""));
	}

	[Test]
	public void FormatCombination_Null_ReturnsEmpty()
	{
		// Act
		var result = KeyNameResolver.FormatCombination(null!);

		// Assert
		Assert.That(result, Is.EqualTo(""));
	}

	[Test]
	public void FormatSequence_Empty_ReturnsEmpty()
	{
		// Act
		var result = KeyNameResolver.FormatSequence([]);

		// Assert
		Assert.That(result, Is.EqualTo(""));
	}

	[Test]
	public void FormatSequence_Null_ReturnsEmpty()
	{
		// Act
		var result = KeyNameResolver.FormatSequence(null!);

		// Assert
		Assert.That(result, Is.EqualTo(""));
	}
}