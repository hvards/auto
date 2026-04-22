using System.Collections.Concurrent;
using System.Runtime.ExceptionServices;

namespace Auto.Handlers;

internal class StaThread
{
	private readonly BlockingCollection<Action> _queue = [];

	public StaThread(string name)
	{
		var thread = new Thread(() =>
		{
			foreach (var action in _queue.GetConsumingEnumerable())
				action();
		})
		{
			Name = name,
			IsBackground = true
		};
		thread.SetApartmentState(ApartmentState.STA);
		thread.Start();
	}

	public void Enqueue(Action action) => _queue.Add(action);

	public T? Execute<T>(Func<T> func)
	{
		T? result = default;
		ExceptionDispatchInfo? error = null;
		using var done = new ManualResetEventSlim();
		_queue.Add(() =>
		{
			try { result = func(); }
			catch (Exception ex) { error = ExceptionDispatchInfo.Capture(ex); }
			finally { done.Set(); }
		});
		done.Wait();
		error?.Throw();
		return result;
	}
}

internal static class StaHandler
{
	private static readonly ConcurrentDictionary<Guid, Lazy<StaThread>> Threads = new();

	private static StaThread GetOrCreate(Guid key) =>
		Threads.GetOrAdd(key, k => new Lazy<StaThread>(() => new StaThread($"STA[{k}]"))).Value;

	public static T? Execute<T>(Guid key, Func<T> func) => GetOrCreate(key).Execute(func);

	public static void Enqueue(Guid key, Action action) => GetOrCreate(key).Enqueue(action);
}
