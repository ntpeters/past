using System.CommandLine;
using System.CommandLine.IO;

namespace past.Extensions
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

        public static bool Write(this IConsole console, string value, bool suppressOutput = false)
        {
            if (!suppressOutput)
            {
                console.Out.Write(value);
                return true;
            }

            return false;
        }

        public static bool WriteLine(this IConsole console, string value, bool suppressOutput = false)
        {
            if (!suppressOutput)
            {
                console.Out.WriteLine(value);
                return true;
            }

            return false;
        }
    }
}
