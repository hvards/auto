using Auto.Cli.Services;
using Auto.Models;
using Auto.PluginUtils;

namespace UnitTests.Cli;

[TestFixture]
public class ArgParserTests
{
	[Test]
	public void ParseValue_Clipboard()
	{
		// Act
		var token = ArgParser.ParseValue("%clipboard");

		// Assert
		Assert.That(token.Type, Is.EqualTo(ArgumentType.Clipboard));
		Assert.That(token.Value, Is.Empty);
	}

	[Test]
	public void ParseValue_Clipboard_CaseInsensitive()
	{
		// Act
		var token = ArgParser.ParseValue("%Clipboard");

		// Assert
		Assert.That(token.Type, Is.EqualTo(ArgumentType.Clipboard));
	}

	[Test]
	public void ParseValue_Highlighted()
	{
		// Act
		var token = ArgParser.ParseValue("%highlighted");

		// Assert
		Assert.That(token.Type, Is.EqualTo(ArgumentType.Highlighted));
		Assert.That(token.Value, Is.Empty);
	}

	[Test]
	public void ParseValue_Plugin()
	{
		// Act
		var token = ArgParser.ParseValue("%plugin:some-guid");

		// Assert
		Assert.That(token.Type, Is.EqualTo(ArgumentType.Plugin));
		Assert.That(token.Value, Is.EqualTo("some-guid"));
	}

	[Test]
	public void ParseValue_PowerShell_Short()
	{
		// Act
		var token = ArgParser.ParseValue("%ps:script.ps1");

		// Assert
		Assert.That(token.Type, Is.EqualTo(ArgumentType.PowerShell));
		Assert.That(token.Value, Is.EqualTo("script.ps1"));
	}

	[Test]
	public void ParseValue_PowerShell_Long()
	{
		// Act
		var token = ArgParser.ParseValue("%powershell:script.ps1");

		// Assert
		Assert.That(token.Type, Is.EqualTo(ArgumentType.PowerShell));
		Assert.That(token.Value, Is.EqualTo("script.ps1"));
	}

	[Test]
	public void ParseValue_PlainText()
	{
		// Act
		var token = ArgParser.ParseValue("notepad.exe");
		
		// Assert
		Assert.That(token.Type, Is.EqualTo(ArgumentType.Text));
		Assert.That(token.Value, Is.EqualTo("notepad.exe"));
	}

	[Test]
	public void ParseValue_TextWithColon_NotMisinterpreted()
	{
		// Act
		var token = ArgParser.ParseValue("https://example.com");

		// Assert
		Assert.That(token.Type, Is.EqualTo(ArgumentType.Text));
		Assert.That(token.Value, Is.EqualTo("https://example.com"));
	}

	[Test]
	public void ParseValue_UnknownPrefix_FallsBackToText()
	{
		// Act
		var token = ArgParser.ParseValue("%unknown:value");

		// Assert
		Assert.That(token.Type, Is.EqualTo(ArgumentType.Text));
		Assert.That(token.Value, Is.EqualTo("%unknown:value"));
	}

	[TestCase("%plugin:")]
	[TestCase("%ps:")]
	public void ParseValue_EmptyPrefixValue_Throws(string input)
	{
		// Act & assert
		Assert.That(() => ArgParser.ParseValue(input), Throws.TypeOf<ArgumentException>());
	}

	[Test]
	public void ParseArgSpec_MissingColon_Throws()
	{
		// Act & assert
		Assert.That(() => ArgParser.ParseArgSpec("nocolon"), Throws.TypeOf<ArgumentException>());
	}

	[Test]
	public void ParseArgSpec_PluginNameKey_ResolvesToGuid()
	{
		// Arrange
		var expectedGuid = PluginLoader.ResolvePlugin("StartProgram");

		// Act
		var (key, arg) = ArgParser.ParseArgSpec("StartProgram:someValue");

		// Assert
		Assert.That(key, Is.EqualTo(expectedGuid));
		Assert.That(arg.Tokens[0].Value, Is.EqualTo("someValue"));
	}

	[Test]
	public void ParsePluginArg_PercentPrefix_NoParamName()
	{
		// Act
		var arg = ArgParser.ParsePluginArg("%{clipboard}");

		// Assert
		Assert.That(arg.ParameterName, Is.Null);
		Assert.That(arg.Tokens[0].Type, Is.EqualTo(ArgumentType.Clipboard));
	}

	[Test]
	public void ParsePluginArg_PlainText()
	{
		// Act
		var arg = ArgParser.ParsePluginArg("notepad.exe");

		// Assert
		Assert.That(arg.Tokens[0].Type, Is.EqualTo(ArgumentType.Text));
		Assert.That(arg.Tokens[0].Value, Is.EqualTo("notepad.exe"));
	}

	[Test]
	public void ParsePluginArg_NestedPlugin()
	{
		// Act
		var arg = ArgParser.ParsePluginArg("%{plugin:854b6621-9ba9-4eae-bafd-89613cac9c5b}");

		// Assert
		Assert.That(arg.ParameterName, Is.Null);
		Assert.That(arg.Tokens[0].Type, Is.EqualTo(ArgumentType.Plugin));
		Assert.That(arg.Tokens[0].Value, Is.EqualTo("854b6621-9ba9-4eae-bafd-89613cac9c5b"));
	}

	[Test]
	public void MatchToToken_PluginNameInToken_ResolvesToGuid()
	{
		// Arrange
		var expectedGuid = PluginLoader.ResolvePlugin("StartProgram");

		// Act
		var arg = ArgParser.ParsePluginArg("%{plugin:StartProgram}");

		// Assert
		Assert.That(arg.Tokens[0].Type, Is.EqualTo(ArgumentType.Plugin));
		Assert.That(arg.Tokens[0].Value, Is.EqualTo(expectedGuid));
	}
}
