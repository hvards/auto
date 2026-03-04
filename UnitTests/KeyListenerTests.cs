using Auto;
using Auto.Commands;
using Auto.Native;
using Moq;

namespace UnitTests;

[TestFixture]
public class KeyListenerTests
{
	private Mock<ICommandProvider> _commandProvider;
	private Mock<IExecute> _execute;
	private Mock<INativeMethods> _nativeMethods;

	[SetUp]
	public void SetUp()
	{
		_commandProvider = new Mock<ICommandProvider>();
		_execute = new Mock<IExecute>();
		_nativeMethods = new Mock<INativeMethods>();

		_nativeMethods.Setup(x => x.GetCurrentProcessHandle()).Returns(21);
		_nativeMethods.Setup(x => x.SetKeyboardHook(It.IsAny<NativeMethods.LowLevelKeyboardProc>(), 21)).Returns(1002);

	}

	[Test]
	public void Constructor_SetsKeyboardHook()
	{
		_ = new KeyListener(_commandProvider.Object, _execute.Object, _nativeMethods.Object);
		_nativeMethods.Verify(x => x.GetCurrentProcessHandle(), Times.Once);
		_nativeMethods.Verify(x => x.SetKeyboardHook(It.IsAny<NativeMethods.LowLevelKeyboardProc>(), 21), Times.Once);
	}
}