namespace Auto.Handlers;

public static class StaHandler
{
	public static T Execute<T>(Func<T> func)
	{
		var result = default(T);
		var thread = new Thread(() =>
		{
			result = func();
		});

		thread.SetApartmentState(ApartmentState.STA);
		thread.Start();
		thread.Join();

		return result;
	}
}