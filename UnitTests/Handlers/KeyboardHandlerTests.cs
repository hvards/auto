using System.Windows.Forms;

using Auto.Handlers;
using Auto.Native;
using Auto.Native.Models;

using Moq;

using static Auto.Native.Constants;

namespace UnitTests.Handlers;

[TestFixture]
internal class KeyboardHandlerTests
{
	private Mock<INativeMethods> _nativeMethods;

	private KeyboardHandler _subject;

	[SetUp]
	public void SetUp()
	{
		_nativeMethods = new Mock<INativeMethods>();

		_subject = new KeyboardHandler(_nativeMethods.Object);
	}

	[TestCase]
	[TestCase((ushort)12)]
	[TestCase((ushort)12, (ushort)125)]
	public void ReleaseAllKeys_ReleasesKeys(params ushort[] pressedKeys)
	{
		foreach (var key in pressedKeys)
		{
			_nativeMethods.Setup(x => x.IsKeyPressed(key)).Returns(true);
		}

		_subject.ReleaseAllKeys();

		_nativeMethods.Verify(x => x.SendKeyboardInput(It.IsAny<KeyboardInput[]>()), Times.Exactly(pressedKeys.Length));
		foreach (var key in pressedKeys)
		{
			_nativeMethods.Verify(x =>
				x.SendKeyboardInput(It.Is<KeyboardInput[]>(inputs =>
					inputs.Length == 1 && IsKeyPress(inputs[0], key, false))));
		}
	}

	[Test]
	public void ClickKey_SendsPressAndRelease_IfNoAction()
	{
		_subject.ClickKey(20, null);
		_nativeMethods.Verify(x => x.SendKeyboardInput(It.IsAny<KeyboardInput[]>()), Times.Once());
		_nativeMethods.Verify(x => x.SendKeyboardInput(It.Is<KeyboardInput[]>(inputs =>
			inputs.Length == 2 && IsKeyPress(inputs[0], 20, true) && IsKeyPress(inputs[1], 20, false))));
	}

	[Test]
	public void CopyHighlightedText_SendsControlVKeys()
	{
		_subject.CopyHighlightedText();
		_nativeMethods.Verify(x => x.SendKeyboardInput(It.IsAny<KeyboardInput[]>()), Times.Once());
		_nativeMethods.Verify(x => x.SendKeyboardInput(It.Is<KeyboardInput[]>(inputs =>
			inputs.Length == 4 &&
			IsKeyPress(inputs[0], (ushort)Keys.LControlKey, true) &&
			IsKeyPress(inputs[1], (ushort)Keys.C, true) &&
			IsKeyPress(inputs[2], (ushort)Keys.C, false) &&
			IsKeyPress(inputs[3], (ushort)Keys.LControlKey, false)
		)));
	}

	[Test]
	public void SendChar_SendsCorrectKey_WhenNameIsUsed()
	{
		var count = 0;
		foreach (var key in Enum.GetValues<Keys>())
		{
			var name = Enum.GetName(key);
			if (name!.Length == 1) continue;

			_subject.SendChar(name);

			_nativeMethods.Verify(x => x.SendKeyboardInput(It.Is<KeyboardInput[]>(inputs =>
				inputs.Length == 2 &&
				IsKeyPress(inputs[0], (ushort)key, true) &&
				IsKeyPress(inputs[1], (ushort)key, false)
			)));
			count++;
		}

		_nativeMethods.Verify(x => x.SendKeyboardInput(It.IsAny<KeyboardInput[]>()), Times.Exactly(count));
	}

	[TestCase(1, (ushort)16)]
	[TestCase(2, (ushort)162)]
	[TestCase(6, (ushort)162, (ushort)165)]
	public void SendChar_SendsModifier_WhenRequired(int modifier, params ushort[] modifierCode)
	{
		_nativeMethods.Setup(x => x.KeyScan('a')).Returns(new KeyScanResult
		{
			Modifier = modifier,
			VirtualKey = 1
		});
		_subject.SendChar("a");
		_nativeMethods.Verify(x => x.SendKeyboardInput(It.IsAny<KeyboardInput[]>()), Times.Once);

		if (modifierCode.Length == 1)
		{
			_nativeMethods.Verify(x => x.SendKeyboardInput(It.Is<KeyboardInput[]>(inputs =>
				inputs.Length == 4 &&
				IsKeyPress(inputs[0], modifierCode[0], true) &&
				IsKeyPress(inputs[1], 1, true) &&
				IsKeyPress(inputs[2], 1, false) &&
				IsKeyPress(inputs[3], modifierCode[0], false)
				)), Times.Once);
		}
		else
		{
			_nativeMethods.Verify(x => x.SendKeyboardInput(It.Is<KeyboardInput[]>(inputs =>
				inputs.Length == 6 &&
				IsKeyPress(inputs[0], modifierCode[0], true) &&
				IsKeyPress(inputs[1], modifierCode[1], true) &&
				IsKeyPress(inputs[2], 1, true) &&
				IsKeyPress(inputs[3], 1, false) &&
				IsKeyPress(inputs[4], modifierCode[1], false) &&
				IsKeyPress(inputs[5], modifierCode[0], false)
				)), Times.Once);
		}
	}


	[Test]
	public void SendChar_SendsWithoutModifier_WhenNotRequired()
	{
		_nativeMethods.Setup(x => x.KeyScan('a')).Returns(new KeyScanResult
		{
			Modifier = 0,
			VirtualKey = 1
		});

		_subject.SendChar("a");

		_nativeMethods.Verify(x => x.SendKeyboardInput(It.IsAny<KeyboardInput[]>()), Times.Once);
		_nativeMethods.Verify(x => x.SendKeyboardInput(It.Is<KeyboardInput[]>(inputs =>
			inputs.Length == 2 &&
			IsKeyPress(inputs[0], 1, true) &&
			IsKeyPress(inputs[1], 1, false)
			)), Times.Once);
	}

	private static bool IsKeyPress(KeyboardInput input, ushort key, bool down)
	{
		return input.wVk == key &&
			   (down && input.dwFlags == (int)KeyEventF.KeyDown || input.dwFlags == (int)KeyEventF.KeyUp) &&
			   input.dwExtraInfo == IGNORE_INPUT;
	}
}
