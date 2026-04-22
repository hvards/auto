using Auto.Handlers;
using Auto.InputUtils;

using static Auto.Native.Constants;

namespace Auto.Tasks;

internal interface ISendInput
{
	void Keyboard(string input);
}

internal class SendInput(IKeyboardHandler keyboardHandler) : ISendInput
{
	public static bool BlockInput { get; private set; }

	public void Keyboard(string input)
	{
		BlockInput = true;
		try
		{
			SendKeyboardTokens(input);
		}
		finally
		{
			BlockInput = false;
		}
	}

	private void SendKeyboardTokens(string input)
	{
		foreach (var token in input.GetTokens())
		{
			switch (token.InputAction)
			{
				case InputAction.NotSet:
					keyboardHandler.SendChar(token.Value);
					break;
				case InputAction.Down:
					keyboardHandler.SendChar(token.Value, WM_KEYDOWN);
					break;
				case InputAction.Up:
					keyboardHandler.SendChar(token.Value, WM_KEYUP);
					break;
				case InputAction.Sleep:
					Thread.Sleep(int.Parse(token.Value));
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}
}
