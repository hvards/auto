using Auto.Handlers;

namespace UnitTests;

[TestFixture]
public class ConverterTests
{
	[Test]
	public void ConvertValueTypes_successfully()
	{
		var argumentValue = TypeConverter.Convert("50", typeof(int));
		Assert.That(argumentValue, Is.EqualTo(50));
		argumentValue = TypeConverter.Convert("true", typeof(bool));
		Assert.That(argumentValue, Is.EqualTo(true));
		argumentValue = TypeConverter.Convert("b", typeof(char));
		Assert.That(argumentValue, Is.EqualTo('b'));
	}

	[Test]
	public void ConvertIntList()
	{
		var input = "12;13;14";
		var expectedResult = new List<int> { 12, 13, 14 };
		var result = TypeConverter.Convert(input, typeof(List<int>));

		Assert.That(result.Count, Is.EqualTo(3));
		for (var i = 0; i < result.Count; i++)
			Assert.That(result[i], Is.EqualTo(expectedResult[i]));
	}

	[Test]
	public void ConvertToBoolArray()
	{
		var input = "false;true;false";
		var expectedResult = new bool[3] { false, true, false };
		var result = TypeConverter.Convert(input, typeof(bool[]));

		Assert.That(result, Is.EqualTo(expectedResult));

	}
}