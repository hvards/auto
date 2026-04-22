using Auto.InputUtils;

namespace UnitTests.InputUtils;

[TestFixture]
internal class InputExtensionsTests
{
	[Test]
	public void GetTokens_ShouldReturnSingleToken_WhenInputIsSingleCharacter()
	{
		// Act
		var tokens = "a".GetTokens().ToList();

		// Assert
		Assert.That(tokens, Has.Count.EqualTo(1));
		Assert.That(tokens[0].Value, Is.EqualTo("a"));
		Assert.That(tokens[0].InputAction, Is.EqualTo(InputAction.NotSet));
	}

	[Test]
	public void GetTokens_ShouldReturnMultipleTokens_WhenInputHasMultipleCharacters()
	{
		// Act
		var tokens = "abc".GetTokens().ToList();

		// Assert
		Assert.That(tokens, Has.Count.EqualTo(3));
		Assert.That(tokens[0].Value, Is.EqualTo("a"));
		Assert.That(tokens[1].Value, Is.EqualTo("b"));
		Assert.That(tokens[2].Value, Is.EqualTo("c"));
	}

	[TestCase("-", InputAction.Up)]
	[TestCase("+", InputAction.Down)]
	public void GetTokens_ShouldReturnTokenWithAction_WhenInputHasPrefix(string prefix, InputAction expectedAction)
	{
		// Act
		var tokens = $"{{{prefix}LCtrl}}".GetTokens().ToList();

		// Assert
		Assert.That(tokens, Has.Count.EqualTo(1));
		Assert.That(tokens[0].Value, Is.EqualTo("LCtrl"));
		Assert.That(tokens[0].InputAction, Is.EqualTo(expectedAction));
	}

	[Test]
	public void GetTokens_ShouldHandleSleepAction()
	{
		// Act
		var tokens = "{!1000}".GetTokens().ToList();

		// Assert
		Assert.That(tokens, Has.Count.EqualTo(1));
		Assert.That(tokens[0].Value, Is.EqualTo("1000"));
		Assert.That(tokens[0].InputAction, Is.EqualTo(InputAction.Sleep));
	}

	[Test]
	public void GetTokens_ShouldHandleNamedKey()
	{
		// Act
		var tokens = "{Enter}".GetTokens().ToList();

		// Assert
		Assert.That(tokens, Has.Count.EqualTo(1));
		Assert.That(tokens[0].Value, Is.EqualTo("Enter"));
		Assert.That(tokens[0].InputAction, Is.EqualTo(InputAction.NotSet));
	}

	[Test]
	public void GetTokens_ShouldHandleEscapedBraces()
	{
		// Act
		var tokens = "{{}}".GetTokens().ToList();

		// Assert
		Assert.That(tokens, Has.Count.EqualTo(2));
		Assert.That(tokens[0].Value, Is.EqualTo("{"));
		Assert.That(tokens[1].Value, Is.EqualTo("}"));
	}

	[Test]
	public void GetTokens_ShouldHandleMixedInput()
	{
		// Act
		var tokens = "hello{Enter}{+LCtrl}c{-LCtrl}".GetTokens().ToList();

		// Assert
		Assert.That(tokens, Has.Count.EqualTo(9));
		Assert.That(tokens[5].Value, Is.EqualTo("Enter"));
		Assert.That(tokens[5].InputAction, Is.EqualTo(InputAction.NotSet));
		Assert.That(tokens[6].Value, Is.EqualTo("LCtrl"));
		Assert.That(tokens[6].InputAction, Is.EqualTo(InputAction.Down));
		Assert.That(tokens[7].Value, Is.EqualTo("c"));
		Assert.That(tokens[8].Value, Is.EqualTo("LCtrl"));
		Assert.That(tokens[8].InputAction, Is.EqualTo(InputAction.Up));
	}
}
