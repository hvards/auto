using System;
using System.IO;

namespace Auto
{
    public static class Log
    {
        private const string RELATIVE_FILE_PATH = @"\log.txt";

        public static void Info(string log) => LogItem(log, "INFO");
        public static void Error(string log) => LogItem(log, "ERROR");
        private static void LogItem(string log, string level) => File.AppendAllText($"C:\\LOGS\\{RELATIVE_FILE_PATH}", $"{level} {DateTime.Now}: {log}\n");
    }
}
