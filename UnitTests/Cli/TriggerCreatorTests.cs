using Auto.Cli.Services;

using Moq;

namespace UnitTests.Cli;

[TestFixture]
internal class TriggerCreatorTests
{
	private Mock<IKeyRecorder> _recorderMock;
	private TriggerCreator _subject;

	[SetUp]
	public void SetUp()
	{
		_recorderMock = new Mock<IKeyRecorder>();
		_subject = new TriggerCreator(_recorderMock.Object);
	}

	[Test]
	public void GetCombination_Null_ReturnsEmpty()
	{
		// Act
		var result = _subject.GetCombination(null);

		// Assert
		Assert.That(result, Is.Empty);
	}

	[Test]
	public void GetCombination_EmptyArray_RecordsCombination()
	{
		// Arrange
		_recorderMock.Setup(r => r.Record(false)).Returns([162, 91, 82]);

		// Act
		var result = _subject.GetCombination([]);

		// Assert
		Assert.That(result.SetEquals(new HashSet<ushort> { 162, 91, 82 }));
		_recorderMock.Verify(r => r.Record(false), Times.Once);
	}

	[Test]
	public void GetCombination_WithKeys_ParsesWithoutRecording()
	{
		// Act
		var result = _subject.GetCombination(["LCtrl", "LWin", "R"]);

		// Assert
		Assert.That(result.SetEquals(new HashSet<ushort> { 162, 91, 82 }));
		_recorderMock.Verify(r => r.Record(It.IsAny<bool>()), Times.Never);
	}

	[Test]
	public void GetSequence_Null_ReturnsEmpty()
	{
		// Act
		var result = _subject.GetSequence(null);

		// Assert
		Assert.That(result, Is.Empty);
	}

	[Test]
	public void GetSequence_EmptyArray_RecordsSequence()
	{
		// Arrange
		_recorderMock.Setup(r => r.Record(true)).Returns([65, 83, 70]);

		// Act
		var result = _subject.GetSequence([]);

		// Assert
		Assert.That(result, Is.EqualTo(new ushort[] { 65, 83, 70 }));
		_recorderMock.Verify(r => r.Record(true), Times.Once);
	}

	[Test]
	public void GetSequence_WithKeys_ParsesWithoutRecording()
	{
		// Act
		var result = _subject.GetSequence(["A", "S", "F"]);

		// Assert
		Assert.That(result, Is.EqualTo(new ushort[] { 65, 83, 70 }));
		_recorderMock.Verify(r => r.Record(It.IsAny<bool>()), Times.Never);
	}

	[Test]
	public void CreateTrigger_NullBoth_ReturnsEmptyTrigger()
	{
		// Act
		var result = _subject.CreateTrigger(null, null);

		// Assert
		Assert.That(result.Combination, Is.Empty);
		Assert.That(result.Sequence, Is.Empty);
	}

	[Test]
	public void CreateTrigger_WithKeys_ParsesBoth()
	{
		// Act
		var result = _subject.CreateTrigger(["LCtrl", "R"], ["A", "S"]);

		// Assert
		Assert.That(result.Combination.SetEquals(new HashSet<ushort> { 162, 82 }));
		Assert.That(result.Sequence, Is.EqualTo(new ushort[] { 65, 83 }));
	}

	[Test]
	public void CreateTrigger_EmptyArrays_RecordsBoth()
	{
		// Arrange
		_recorderMock.Setup(r => r.Record(false)).Returns([162, 82]);
		_recorderMock.Setup(r => r.Record(true)).Returns([65, 83]);

		// Act
		var result = _subject.CreateTrigger([], []);

		// Assert
		Assert.That(result.Combination.SetEquals(new HashSet<ushort> { 162, 82 }));
		Assert.That(result.Sequence, Is.EqualTo(new ushort[] { 65, 83 }));
	}
}