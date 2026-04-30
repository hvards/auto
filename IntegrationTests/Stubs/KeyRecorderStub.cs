using Auto.Cli.Services;

namespace IntegrationTests.Stubs;

internal sealed class KeyRecorderStub : IKeyRecorder
{
	public HashSet<ushort> NextCombination { get; set; } = [];
	public ushort[] NextSequence { get; set; } = [];
	public string NextInput { get; set; } = string.Empty;

	public void Reset()
	{
		NextCombination = [];
		NextSequence = [];
		NextInput = string.Empty;
	}

	public HashSet<ushort> RecordCombination() => NextCombination;
	public ushort[] RecordSequence() => NextSequence;
	public string RecordInput(bool recordDelay = false) => NextInput;
}
