namespace Auto.Interfaces;

public interface IPluginExecutor
{
	object ExecutePlugin(string id, IEnumerable<object> args);
}