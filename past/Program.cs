using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.CommandLine.Parsing;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace past
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            var listCommand = new Command("list", "Lists the clipboard history");
            listCommand.Handler = CommandHandler.Create<IConsole, CancellationToken>(ListClipboardHistoryAsync);

            var rootCommand = new RootCommand();
            rootCommand.AddCommand(listCommand);

            return await rootCommand.InvokeAsync(args);
        }

        private static async Task<int> ListClipboardHistoryAsync(IConsole console, CancellationToken cancellationToken)
        {
            return 0;
        }
    }
}
