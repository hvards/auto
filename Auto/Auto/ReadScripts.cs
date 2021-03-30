using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using static Auto.Constants;

namespace Auto
{
    public static class ReadScripts
    {
        public static List<Script> GetScripts(string[] files)
        {
            var scripts = new List<Script>();

            foreach (var file in files)
            {
                var lines = File.ReadAllLines(file, Encoding.GetEncoding("iso-8859-1"));

                foreach (var line in lines)
                {
                    try
                    {
                        var temp = line.Split('|');
                        var command = temp[0];
                        var args = temp[3].Split(';');
                        var macro = temp[1].Length > 0 ? temp[1].Split(';').Select(c => (ushort)KeyMap[c]).ToArray() : Array.Empty<ushort>();
                        var combo = temp[2].Length > 0 ? temp[2].Split(';').Select(c => (ushort)KeyMap[c]).ToHashSet() : new HashSet<ushort>();
                        scripts.Add(new Script { Command = command, KeyCombo = combo, Macro = macro, CommandArgs = args });
                    }
                    catch
                    {
                        Log.Info($"Error loading script: {line}");
                    }
                }
                Log.Info($"ReadScripts: {scripts.Count()}");
            }
            return scripts;
        }
    }
}
