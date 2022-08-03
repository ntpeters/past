using past.ConsoleApp.Commands;
using System;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Linq;
using System.Threading.Tasks;

namespace past.ConsoleApp
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
#if DEBUG
            // In debug builds halt execution until a key is pressed if the debug flag was provided to allow attaching a debugger.
            // Check for presence of the debug flag even before any argument parsing is done so that code can be debugged if needed as well.
            if (args.Contains("--debug"))
            {
                var originalForeground = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Error.WriteLine("DEBUG: Ready to attach debugger. Press any key to continue execution...");
                Console.ForegroundColor = originalForeground;
                Console.ReadKey(intercept: true); // Suppress printing the pressed key
            }
#endif // DEBUG
            var rootCommand = new PastCommand();
            return await rootCommand.InvokeAsync(args);
        }
    }
}
