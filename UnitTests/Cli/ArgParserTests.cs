using Auto.Cli.Services;
using Auto.Models;

namespace UnitTests.Cli;

[TestFixture]
public class ArgParserTests
{
	// ParseValue

	[Test]
	public void ParseValue_Clipboard()
	{
		// Act
		var token = ArgParser.ParseValue("%clipboard");

		// Assert
		Assert.That(token.Type, Is.EqualTo(ArgumentType.Clipboard));
		Assert.That(token.Value, Is.Null);
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
		Assert.That(token.Value, Is.Null);
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
		// Act & Assert
		Assert.That(() => ArgParser.ParseValue(input), Throws.TypeOf<ArgumentException>());
	}

	// ParseAction

	[Test]
	public void ParseAction_Plugin()
	{
		// Act
		var token = ArgParser.ParseAction("plugin:some-guid");

		// Assert
		Assert.That(token.Type, Is.EqualTo(ArgumentType.Plugin));
		Assert.That(token.Value, Is.EqualTo("some-guid"));
	}

	[Test]
	public void ParseAction_PowerShell()
	{
		// Act
		var token = ArgParser.ParseAction("ps:script.ps1");

		// Assert
		Assert.That(token.Type, Is.EqualTo(ArgumentType.PowerShell));
		Assert.That(token.Value, Is.EqualTo("script.ps1"));
	}

	[Test]
	public void ParseAction_PercentPrefix_DelegatesToParseValue()
	{
		// Act
		var token = ArgParser.ParseAction("%plugin:some-guid");

		// Assert
		Assert.That(token.Type, Is.EqualTo(ArgumentType.Plugin));
		Assert.That(token.Value, Is.EqualTo("some-guid"));
	}

	[Test]
	public void ParseAction_Clipboard_BareKeyword()
	{
		// Act
		var token = ArgParser.ParseAction("clipboard");

		// Assert
		Assert.That(token.Type, Is.EqualTo(ArgumentType.Clipboard));
		Assert.That(token.Value, Is.Null);
	}

	[Test]
	public void ParseAction_Clipboard_WithColon_IgnoresValue()
	{
		// Act
		var token = ArgParser.ParseAction("clipboard:ignored");

		// Assert
		Assert.That(token.Type, Is.EqualTo(ArgumentType.Clipboard));
		Assert.That(token.Value, Is.Null);
	}

	[Test]
	public void ParseAction_Highlighted_BareKeyword()
	{
		// Act
		var token = ArgParser.ParseAction("highlighted");

		// Assert
		Assert.That(token.Type, Is.EqualTo(ArgumentType.Highlighted));
		Assert.That(token.Value, Is.Null);
	}

	[Test]
	public void ParseAction_UnknownBareKeyword_Throws()
	{
		// Act & Assert
		Assert.That(() => ArgParser.ParseAction("nocolon"), Throws.TypeOf<ArgumentException>());
	}

	[Test]
	public void ParseAction_UnknownType_FallsBackToText()
	{
		// Act
		var token = ArgParser.ParseAction("badtype:val");

		// Assert
		Assert.That(token.Type, Is.EqualTo(ArgumentType.Text));
		Assert.That(token.Value, Is.EqualTo("%badtype:val"));
	}

	// ParseArgSpec

	[Test]
	public void ParseArgSpec_SplitsOnFirstColon()
	{
		// Act
		var (key, arg) = ArgParser.ParseArgSpec("script:someValue");

		// Assert
		Assert.That(key, Is.EqualTo("script"));
		Assert.That(arg.Tokens[0].Value, Is.EqualTo("someValue"));
	}

	[Test]
	public void ParseArgSpec_MissingColon_Throws()
	{
		// Act & Assert
		Assert.That(() => ArgParser.ParseArgSpec("nocolon"), Throws.TypeOf<ArgumentException>());
	}

	// ParsePluginArg

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
}