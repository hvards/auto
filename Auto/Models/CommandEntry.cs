namespace Auto.Models;

internal class CommandEntry : CommandBase
{
	public string Name { get; set; } = string.Empty;
	public string Description { get; set; } = string.Empty;
	public Guid Id { get; set; }
}