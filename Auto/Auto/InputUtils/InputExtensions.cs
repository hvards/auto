namespace Auto.InputUtils;

public static class InputExtensions
{
    public static IEnumerable<InputToken> GetTokens(this string input)
    {
        for (var i = 0; i < input.Length; i++)
        {
            var value = input[i].ToString();
            var action = InputAction.NotSet;
            switch (value)
            {
                case "{":
                    value = GetKey(input, i + 1);
                    i += value.Length + 1;
                    break;
                case "[":
                    // KeyDown: [1:key], KeyUp: [0:key]
                    value = GetKey(input, i+3);
                    action = input[i + 2].Equals('!') ? InputAction.Sleep :
                        input[1 + i] == '1' ? InputAction.Down : InputAction.Up;
                    i += value.Length + 3;
                    break;
            }
            yield return new InputToken { Value = value, InputAction = action };
        }
    }

    private static string GetKey(string input, int startPos) =>
        string.Concat(input[startPos..].TakeWhile(c => !(c.Equals(']') || c.Equals('}'))));
}