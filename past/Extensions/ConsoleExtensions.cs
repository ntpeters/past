using System.CommandLine;
using System.CommandLine.IO;

namespace past.Extensions
{
    public static class ConsoleExtensions
    {
        public static void WriteError(this IConsole console, string value, bool suppressOutput = false)
        {
            if (!suppressOutput)
            {
                console.Error.Write(value);
            }
        }


        public static void WriteErrorLine(this IConsole console, string value, bool suppressOutput = false)
        {
            if (!suppressOutput)
            {
                console.Error.WriteLine(value);
            }
        }

        public static void Write(this IConsole console, string value, bool suppressOutput = false)
        {
            if (!suppressOutput)
            {
                console.Out.Write(value);
            }
        }

        public static void WriteLine(this IConsole console, string value, bool suppressOutput = false)
        {
            if (!suppressOutput)
            {
                console.Out.WriteLine(value);
            }
        }
    }
}
