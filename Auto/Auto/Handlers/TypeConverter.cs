namespace Auto.Handlers;

public static class TypeConverter
{
	private delegate bool TryParseDelegate<T>(string input, out T result);

	// ReSharper disable once UnusedMember.Global
	public static bool TryParse<T>(string input, out T result)
	{
		var method = typeof(T).GetMethod("TryParse", [typeof(string), typeof(T).MakeByRefType()]);
		if (method == null)
		{
			result = default;
			return false;
		}

		var tryParse = (TryParseDelegate<T>)Delegate.CreateDelegate(typeof(TryParseDelegate<T>), method);
		return tryParse(input, out result);
	}

	private static dynamic GetDefaultValue(Type type) => type.IsValueType ? Activator.CreateInstance(type) : null;
	private static bool IsList(Type type) => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>);

	private static dynamic ConvertToArray(string arg, Type type, bool array)
	{
		var res = arg.Split(";").Select(x =>
		{
			var success = TryParseType(x, type, out var res);
			return success ? res : GetDefaultValue(type);
		});

		return array
			? res.ToArray()
			: res.ToList();
	}

	private static bool TryParseType(string input, Type type, out dynamic result)
	{
		result = GetDefaultValue(type);

		var method = typeof(TypeConverter).GetMethod("TryParse")!.MakeGenericMethod(type);
		var parameters = new object[] { input, result };
		var success = (bool)method.Invoke(null, parameters)!;
		result = parameters[1];
		return success;
	}

	public static dynamic Convert(string input, Type type)
	{
		if (!IsList(type) && !type.IsArray)
			return TryParseType(input, type, out var res)
				? res
				: input;

		var elementType = type.IsArray
			? type.GetElementType()
			: type.GetGenericArguments().Single();
		return ConvertToArray(input, elementType, type.IsArray);
	}
}