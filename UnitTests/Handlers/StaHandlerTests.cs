using Auto.Handlers;

namespace UnitTests.Handlers;

[TestFixture]
internal class StaHandlerTests
{
	[Test]
	public void Execute_RunsOnStaThread()
	{
		// Arrange
		var key = Guid.NewGuid();

		// Act
		var apartment = StaHandler.Execute(key, () => Thread.CurrentThread.GetApartmentState());

		// Assert
		Assert.That(apartment, Is.EqualTo(ApartmentState.STA));
	}

	[Test]
	public void Execute_SameKey_ReusesThread()
	{
		// Arrange
		var key = Guid.NewGuid();

		// Act
		var first = StaHandler.Execute(key, () => Environment.CurrentManagedThreadId);
		var second = StaHandler.Execute(key, () => Environment.CurrentManagedThreadId);

		// Assert
		Assert.That(second, Is.EqualTo(first));
	}

	[Test]
	public void Execute_DifferentKeys_UseDifferentThreads()
	{
		// Act
		var threadA = StaHandler.Execute(Guid.NewGuid(), () => Environment.CurrentManagedThreadId);
		var threadB = StaHandler.Execute(Guid.NewGuid(), () => Environment.CurrentManagedThreadId);

		// Assert
		Assert.That(threadB, Is.Not.EqualTo(threadA));
	}

	[Test]
	public void Execute_PropagatesExceptionWithOriginalStackTrace()
	{
		// Arrange
		var key = Guid.NewGuid();

		// Act
		var ex = Assert.Throws<InvalidOperationException>(
			() => StaHandler.Execute<int>(key, ThrowException));

		// Assert
		Assert.That(ex!.Message, Is.EqualTo("message1"));
		Assert.That(ex.StackTrace, Does.Contain(nameof(ThrowException)));
	}

	private static int ThrowException() => throw new InvalidOperationException("message1");
}
