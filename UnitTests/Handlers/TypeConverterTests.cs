using Auto.Handlers;

namespace UnitTests.Handlers;

[TestFixture]
internal class TypeConverterTests
{
	[TestCase("123", true, 123)]
	[TestCase("abc", false, 0)]
	public void TryParse_Int(string input, bool expectedSuccess, int expectedResult)
	{
		var success = TypeConverter.TryParse<int>(input, out var result);
		using (Assert.EnterMultipleScope())
		{
			Assert.That(success, Is.EqualTo(expectedSuccess));
			Assert.That(result, Is.EqualTo(expectedResult));
		}
	}

	[TestCase("true", true, true)]
	[TestCase("false", true, false)]
	[TestCase("False", true, false)]
	[TestCase("True", true, true)]
	[TestCase("0", false, false)]
	[TestCase("thisistrue", false, false)]
	public void TryParse_Bool(string input, bool expectedSuccess, bool expectedResult)
	{
		var success = TypeConverter.TryParse<bool>(input, out var result);
		using (Assert.EnterMultipleScope())
		{
			Assert.That(success, Is.EqualTo(expectedSuccess));
			Assert.That(result, Is.EqualTo(expectedResult));
		}
	}

	[TestCase("2021-01-01", true, "2021-01-01")]
	[TestCase("tomorrow", false, "")]
	public void TryParse_DateTime(string input, bool expectedSuccess, string expectedResult)
	{
		var success = TypeConverter.TryParse<DateTime>(input, out var result);
		var expectedDateTimeResult = default(DateTime);
		if (expectedSuccess)
			expectedDateTimeResult = DateTime.Parse(expectedResult);

		using (Assert.EnterMultipleScope())
		{
			Assert.That(success, Is.EqualTo(expectedSuccess));
			Assert.That(result, Is.EqualTo(expectedDateTimeResult));
		}
	}

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

		Assert.That(result?.Count, Is.EqualTo(3));
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
