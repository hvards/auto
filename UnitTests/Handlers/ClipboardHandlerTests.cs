using Auto.Handlers;

using Moq;

namespace UnitTests.Handlers;

[TestFixture]
internal class ClipboardHandlerTests
{
	private Mock<IKeyboardHandler> _keyboardHandlerMock;
	private ClipboardHandler _subject;

	[SetUp]
	public void SetUp()
	{
		_keyboardHandlerMock = new Mock<IKeyboardHandler>();
		_subject = new ClipboardHandler(_keyboardHandlerMock.Object);
	}

	[Test]
	public void GetClipboardText_ShouldCopyHighlightedText_WhenParameterIsTrue()
	{
		// Act
		_subject.GetClipboardText(true);

		// Assert
		_keyboardHandlerMock.Verify(x => x.ReleaseAllKeys(), Times.Once);
		_keyboardHandlerMock.Verify(x => x.CopyHighlightedText(), Times.Once);
	}

	[Test]
	public void GetClipboardText_ShouldNotCopyHighlightedText_WhenParameterIsFalse()
	{
		// Act
		_subject.GetClipboardText();

		// Assert
		_keyboardHandlerMock.Verify(x => x.ReleaseAllKeys(), Times.Never);
		_keyboardHandlerMock.Verify(x => x.CopyHighlightedText(), Times.Never);
	}
}