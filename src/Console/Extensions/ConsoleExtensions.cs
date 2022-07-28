using System.CommandLine;
using System.CommandLine.IO;

namespace past.Console.Extensions
{
    public static class ConsoleExtensions
    {
        public static bool WriteError(this IConsole console, string value, bool suppressOutput = false)
        {
            if (!suppressOutput)
            {
                console.Error.Write(value);
                return true;
            }

            return false;
        }


        public static bool WriteErrorLine(this IConsole console, string value, bool suppressOutput = false)
        {
            if (!suppressOutput)
            {
                console.Error.WriteLine(value);
                return true;
            }

            return false;
        }
    }
}
