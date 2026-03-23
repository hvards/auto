using Auto.InputUtils;

namespace UnitTests.InputUtils;

[TestFixture]
internal class InputExtensionsTests
{
	[Test]
	public void GetTokens_ShouldReturnSingleToken_WhenInputIsSingleCharacter()
	{
		// Arrange
		var input = "a";

		// Act
		var tokens = input.GetTokens().ToList();

		// Assert
		Assert.That(tokens, Has.Count.EqualTo(1));
		Assert.That(tokens[0].Value, Is.EqualTo("a"));
		Assert.That(tokens[0].InputAction, Is.EqualTo(InputAction.NotSet));
	}

	[Test]
	public void GetTokens_ShouldReturnMultipleTokens_WhenInputHasMultipleCharacters()
	{
		// Arrange
		var input = "abc";

		// Act
		var tokens = input.GetTokens().ToList();

		// Assert
		Assert.That(tokens, Has.Count.EqualTo(3));
		Assert.That(tokens[0].Value, Is.EqualTo("a"));
		Assert.That(tokens[1].Value, Is.EqualTo("b"));
		Assert.That(tokens[2].Value, Is.EqualTo("c"));
	}

	[TestCase(0, InputAction.Up)]
	[TestCase(1, InputAction.Down)]
	public void GetTokens_ShouldReturnTokenWithAction_WhenInputHasBrackets(int action, InputAction expectedAction)
	{
		// Arrange
		var input = $"[{action}:a]";

		// Act
		var tokens = input.GetTokens().ToList();

		// Assert
		Assert.That(tokens, Has.Count.EqualTo(1));
		Assert.That(tokens[0].Value, Is.EqualTo("a"));
		Assert.That(tokens[0].InputAction, Is.EqualTo(expectedAction));
	}

	[Test]
	public void GetTokens_ShouldHandleSleepAction_WhenInputHasSleepAction()
	{
		// Arrange 
		var input = "[!:1000]";

		// Act
		var tokens = input.GetTokens().ToList();

		// Assert
		Assert.That(tokens, Has.Count.EqualTo(1));
		Assert.That(tokens[0].Value, Is.EqualTo("1000"));
		Assert.That(tokens[0].InputAction, Is.EqualTo(InputAction.Sleep));
	}
}