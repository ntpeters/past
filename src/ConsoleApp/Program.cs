using past.ConsoleApp.Binders;
using past.Core;
using System;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Linq;
using System.Threading;
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

            var consoleClipboard = new ConsoleClipboard();
            var commandFactory = new CommandFactory();

            var rootCommand = new RootCommand();
            rootCommand.AddGlobalOption(commandFactory.TypeOption);
            rootCommand.AddGlobalOption(commandFactory.AllOption);
            rootCommand.AddGlobalOption(commandFactory.AnsiOption);
            rootCommand.AddGlobalOption(commandFactory.AnsiResetOption);
            rootCommand.AddGlobalOption(commandFactory.QuietOption);
            rootCommand.AddGlobalOption(commandFactory.DebugOption);

            var listCommand = commandFactory.CreateListCommand(consoleClipboard.ListClipboardHistoryAsync);
            var getCommand = commandFactory.CreateGetCommand(consoleClipboard.GetClipboardHistoryItemAsync);
            var statusCommand = commandFactory.CreateStatusCommand(consoleClipboard.GetClipboardHistoryStatus);
            var helpCommand = commandFactory.CreateHelpCommand(async (command) =>
                {
                    if (string.IsNullOrWhiteSpace(command))
                    {
                        await Main(new string[] { "--help" });
                    }
                    else
                    {
                        await Main(new string[] { "--help", command });
                    }
                });

            rootCommand.AddCommand(listCommand);
            rootCommand.AddCommand(getCommand);
            rootCommand.AddCommand(statusCommand);
            rootCommand.AddCommand(helpCommand);

            rootCommand.SetHandler<IConsole, ContentType, bool, AnsiResetType, bool, CancellationToken>(
                consoleClipboard.GetCurrentClipboardValueAsync,
                new ContentTypeBinder(commandFactory.TypeOption, commandFactory.AllOption), commandFactory.AnsiOption, commandFactory.AnsiResetOption, commandFactory.QuietOption);

            return await rootCommand.InvokeAsync(args);
        }
    }
}
