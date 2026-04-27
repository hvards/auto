using Auto;
using Auto.Commands;
using Auto.Models;
using Auto.Native;

using Moq;

using static Auto.Native.Constants;

namespace UnitTests;

[TestFixture]
internal class KeyListenerTests
{
	private const ushort Ctrl = 0x11;
	private const ushort A = 0x41;

	private Mock<ICommandProvider> _commandProvider;
	private Mock<IExecute> _execute;
	private Mock<INativeMethods> _nativeMethods;
	private NativeMethods.LowLevelKeyboardProc _callback = null!;

	[SetUp]
	public void SetUp()
	{
		_commandProvider = new Mock<ICommandProvider>();
		_execute = new Mock<IExecute>();
		_nativeMethods = new Mock<INativeMethods>();

		_nativeMethods.Setup(x => x.GetCurrentProcessHandle()).Returns(21);
		_nativeMethods.Setup(x => x.IsKeyPressed(It.IsAny<int>())).Returns(true);
		_nativeMethods
			.Setup(x => x.SetKeyboardHook(It.IsAny<NativeMethods.LowLevelKeyboardProc>(), 21))
			.Callback<NativeMethods.LowLevelKeyboardProc, nint>((cb, _) => _callback = cb)
			.Returns(1002);

		Command? command = new();
		_commandProvider
			.Setup(x => x.TryGetCommand(It.Is<HashSet<ushort>>(s => s.SetEquals(new[] { Ctrl, A })), A, out command))
			.Returns(true);

	}

	[Test]
	public void Constructor_SetsKeyboardHook()
	{
		// Act
		_ = new KeyListener(_commandProvider.Object, _execute.Object, _nativeMethods.Object);

		// Assert
		_nativeMethods.Verify(x => x.GetCurrentProcessHandle(), Times.Once);
		_nativeMethods.Verify(x => x.SetKeyboardHook(It.IsAny<NativeMethods.LowLevelKeyboardProc>(), 21), Times.Once);
	}

	[Test]
	public void HoldingTriggerKey_CommandOnce()
	{
		// Arrange
		_ = new KeyListener(_commandProvider.Object, _execute.Object, _nativeMethods.Object);

		// Act
		Send(WM_KEYDOWN, Ctrl);
		Send(WM_KEYDOWN, A);
		Send(WM_KEYDOWN, A); // auto-repeat
		Send(WM_KEYDOWN, A); // auto-repeat
		Send(WM_KEYUP, A);
		Send(WM_KEYUP, Ctrl);

		// Assert
		_execute.Verify(x => x.QueueCommand(It.IsAny<Command>()), Times.Once);
	}

	[Test]
	public void ReleasingAndRepressingTriggerKey_CommandEachTime()
	{
		// Arrange
		_ = new KeyListener(_commandProvider.Object, _execute.Object, _nativeMethods.Object);

		// Act
		Send(WM_KEYDOWN, Ctrl);
		Send(WM_KEYDOWN, A);
		Send(WM_KEYUP, A);
		Send(WM_KEYDOWN, A);
		Send(WM_KEYUP, A);
		Send(WM_KEYUP, Ctrl);

		// Assert
		_execute.Verify(x => x.QueueCommand(It.IsAny<Command>()), Times.Exactly(2));
	}

	[Test]
	public void MissedKeyUp_DoNotTriggerCommandIfReleased()
	{
		// Arrange
		_ = new KeyListener(_commandProvider.Object, _execute.Object, _nativeMethods.Object);
		Send(WM_KEYDOWN, Ctrl);
		_nativeMethods.Setup(x => x.IsKeyPressed(Ctrl)).Returns(false);

		// Act
		Send(WM_KEYDOWN, A);

		// Assert
		_execute.Verify(x => x.QueueCommand(It.IsAny<Command>()), Times.Never);
	}

	[Test]
	public void MissedKeyUp_TriggersCommandAgainWithIntermediateKey()
	{
		// Arrange
		const ushort b = 0x42;
		_ = new KeyListener(_commandProvider.Object, _execute.Object, _nativeMethods.Object);

		// Trigger command, keys added to suppressedRepeats
		Send(WM_KEYDOWN, Ctrl);
		Send(WM_KEYDOWN, A);

		// A released without keyup event
		_nativeMethods.Setup(x => x.IsKeyPressed(A)).Returns(false);

		// A is removed from pressed keys sets
		Send(WM_KEYDOWN, b);
		Send(WM_KEYUP, b);

		// A now pressed again
		_nativeMethods.Setup(x => x.IsKeyPressed(A)).Returns(true);

		// Ctrl still pressed, command triggered
		Send(WM_KEYDOWN, A);

		// Assert
		_execute.Verify(x => x.QueueCommand(It.IsAny<Command>()), Times.Exactly(2));
	}

	private void Send(nint wParam, ushort vkCode)
	{
		var input = new KeyboardInput { wVk = vkCode };
		_callback(0, wParam, ref input);
	}
}
