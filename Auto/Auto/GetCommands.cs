using System.Text;
using static Auto.Constants;

namespace Auto;

public class GetCommands
{
    public static List<Command> Execute(string[] files)
    {
        var commands = new List<Command>();

        foreach (var file in files)
        {
            var lines = File.ReadAllLines(file, Encoding.GetEncoding("iso-8859-1"));

            foreach (var line in lines)
            {
                try
                {
                    var temp = line.Split('|');
                    var keyword = temp[0];
                    var args = temp[3].Split(';');
                    var macro = temp[1].Length > 0 ? temp[1].Split(';').Select(c => (ushort)KeyMap[c]).ToArray() : Array.Empty<ushort>();
                    var combo = temp[2].Length > 0 ? temp[2].Split(';').Select(c => (ushort)KeyMap[c]).ToHashSet() : new HashSet<ushort>();
                    commands.Add(new Command { Keyword = keyword, KeyCombo = combo, Macro = macro, args = args });
                }
                catch
                {
                    Log.Info($"Error loading command: {line}");
                }
            }
            Log.Info($"GetCommands: {commands.Count}");
        }
        return commands;
    }
}