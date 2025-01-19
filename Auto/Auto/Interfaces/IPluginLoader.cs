using Auto.Command;

namespace Auto.Interfaces;

public interface IPluginLoader
{
	Dictionary<string, Plugin> CreateCommands();
}