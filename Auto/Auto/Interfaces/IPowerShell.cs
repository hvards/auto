namespace Auto.Interfaces;

public interface IPowerShell
{
	string Execute(string file, IList<(string name, string value)> parameters);
}