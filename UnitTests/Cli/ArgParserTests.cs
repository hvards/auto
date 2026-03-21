using Auto.Cli.Services;
using Auto.Models;

namespace UnitTests.Cli;

[TestFixture]
public class ArgParserTests
{
	[TestCase("Highlighted")]
	[TestCase("Clipboard")]
	[TestCase("SomeName")]
	public void ParsePluginArg_VariableToken(string variableName)
	{
		// Act
		var arg = ArgParser.ParsePluginArgument($"%{{{variableName}}}");

		// Assert
		Assert.That(arg.ParameterName, Is.Null);
		Assert.That(arg.Tokens[0].Type, Is.EqualTo(ArgumentType.Variable));
		Assert.That(arg.Tokens[0].Value, Is.EqualTo(variableName));
	}

	[TestCase("notepad.exe")]
	[TestCase("https://example.com")]
	public void ParsePluginArg_PlainText(string text)
	{
		// Act
		var arg = ArgParser.ParsePluginArgument(text);

		// Assert
		Assert.That(arg.Tokens[0].Type, Is.EqualTo(ArgumentType.Text));
		Assert.That(arg.Tokens[0].Value, Is.EqualTo(text));
	}

	[Test]
	public void ParsePluginArg_MixedTokens()
	{
		// Act
		var arg = ArgParser.ParsePluginArgument("prefix_%{Clipboard}_suffix");

		// Assert
		Assert.That(arg.Tokens, Has.Length.EqualTo(3));
		Assert.That(arg.Tokens[0].Type, Is.EqualTo(ArgumentType.Text));
		Assert.That(arg.Tokens[0].Value, Is.EqualTo("prefix_"));
		Assert.That(arg.Tokens[1].Type, Is.EqualTo(ArgumentType.Variable));
		Assert.That(arg.Tokens[1].Value, Is.EqualTo("Clipboard"));
		Assert.That(arg.Tokens[2].Type, Is.EqualTo(ArgumentType.Text));
		Assert.That(arg.Tokens[2].Value, Is.EqualTo("_suffix"));
	}

	[Test]
	public void ParsePowerShellArg_NamedParameter()
	{
		// Act
		var arg = ArgParser.ParsePowerShellArgument("Path=C:\\scripts");

		// Assert
		Assert.That(arg.ParameterName, Is.EqualTo("Path"));
		Assert.That(arg.Tokens[0].Type, Is.EqualTo(ArgumentType.Text));
		Assert.That(arg.Tokens[0].Value, Is.EqualTo("C:\\scripts"));
	}

	[Test]
	public void ParsePowerShellArg_VariableValue()
	{
		// Act
		var arg = ArgParser.ParsePowerShellArgument("Input=%{Clipboard}");

		// Assert
		Assert.That(arg.ParameterName, Is.EqualTo("Input"));
		Assert.That(arg.Tokens[0].Type, Is.EqualTo(ArgumentType.Variable));
		Assert.That(arg.Tokens[0].Value, Is.EqualTo("Clipboard"));
	}

	[Test]
	public void ParsePowerShellArg_Variable_NoParamName()
	{
		// Act
		var arg = ArgParser.ParsePowerShellArgument("%{Clipboard}");

		// Assert
		Assert.That(arg.ParameterName, Is.Null);
		Assert.That(arg.Tokens[0].Type, Is.EqualTo(ArgumentType.Variable));
		Assert.That(arg.Tokens[0].Value, Is.EqualTo("Clipboard"));
	}

	[Test]
	public void ParsePluginArg_MultipleVariables()
	{
		// Act
		var arg = ArgParser.ParsePluginArgument("%{Var1}%{Var2}");

		// Assert
		Assert.That(arg.Tokens, Has.Length.EqualTo(2));
		Assert.That(arg.Tokens[0].Type, Is.EqualTo(ArgumentType.Variable));
		Assert.That(arg.Tokens[0].Value, Is.EqualTo("Var1"));
		Assert.That(arg.Tokens[1].Type, Is.EqualTo(ArgumentType.Variable));
		Assert.That(arg.Tokens[1].Value, Is.EqualTo("Var2"));
	}

	[Test]
	public void ParsePluginArg_EmptyString_ReturnsEmpty()
	{
		// Act
		var arg = ArgParser.ParsePluginArgument("");

		// Assert
		Assert.That(arg.Tokens, Is.Empty);
	}
}
