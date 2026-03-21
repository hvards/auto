namespace Auto.Models;

public class CommandArgument
{
	public string? ParameterName { get; set; }
	public required ArgumentToken[] Tokens { get; set; }
}
