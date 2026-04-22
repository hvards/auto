namespace AutoContracts;

public interface ICommand
{
	/// <summary>
	/// Plugin name.
	/// </summary>
	string Name { get; }

	/// <summary>
	/// Plugin description.
	/// </summary>
	string Description { get; }

	/// <summary>
	/// Unique plugin identifier.
	/// </summary>
	Guid Id { get; }

	/// <summary>
	/// Return type of the <c>Execute</c> function, if <c>Execute</c> throws an exception the default value of this
	/// type will be used.
	/// </summary>
	Type ReturnType { get; }

	/// <summary>
	/// Used to help configure plugin correctly.
	/// </summary>
	List<PluginArgument> ExpectedArguments { get; }

	/// <summary>
	/// If <c>true</c>, the plugin is executed on a dedicated STA thread owned by this plugin.
	/// Required for WinForms/WPF UI, single-threaded-apartment COM. The same thread is reused across invocations.
	/// Blocking delays subsequent calls to this plugin, not others.
	/// </summary>
	bool RequiresSta { get; }

	/// <summary>
	/// Called once, completing before any <c>Execute</c> call.
	/// </summary>
	void Init();

	/// <summary>
	///	Executes plugin and return result.
	/// </summary>
	/// <param name="args">Arguments as defined in <c>ExpectedArgument</c>, if correctly configured.</param>
	/// <returns>
	/// If return object is input for another plugin it will be used as is, if not the <c>.ToString()</c> method will
	/// be called instead.
	/// </returns>
	object? Execute(object?[] args);
}
