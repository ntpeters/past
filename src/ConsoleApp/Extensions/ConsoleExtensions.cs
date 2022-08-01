using System.CommandLine;
using System.CommandLine.IO;

namespace past.ConsoleApp.Extensions
{
    /// <summary>
    /// Extensions for <see cref="IConsole"/>.
    /// </summary>
    public static class ConsoleExtensions
    {
        /// <summary>
        /// Writes the specified string to the error stream, unless <paramref name="suppressOutput"/> is true.
        /// </summary>
        /// <param name="console">The <see cref="IConsole"/> to write to the error stream of.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="suppressOutput">Whether to suppress writing the value.</param>
        /// <returns>True if the value was written, false otherwise.</returns>
        public static bool WriteError(this IConsole console, string value, bool suppressOutput = false)
        {
            if (!suppressOutput)
            {
                console.Error.Write(value);
                return true;
            }

            return false;
        }


        /// <summary>
        /// Writes the specified string to the error stream, followed by the current environment's line terminator,
        /// unless <paramref name="suppressOutput"/> is true.
        /// </summary>
        /// <param name="console">The <see cref="IConsole"/> to write to the error stream of.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="suppressOutput">Whether to suppress writing the value.</param>
        /// <returns>True if the value was written, false otherwise.</returns>
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
