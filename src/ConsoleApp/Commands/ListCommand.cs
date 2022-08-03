using past.ConsoleApp.Binders;
using past.Core;
using System;
using System.CommandLine;
using System.Threading;
using System.Threading.Tasks;

namespace past.ConsoleApp.Commands
{
    /// <summary>
    /// Prints the items currently stored in the clipboardHistory,
    /// </summary>
    public class ListCommand : Command
    {
        /// <summary>
        /// Creates a new <see cref="ListCommand"/> with the given options and handler.
        /// </summary>
        /// <param name="typeOption">Option for specifying content type.</param>
        /// <param name="allOption">Option alias for ContentType.All</param>
        /// <param name="ansiOption">Option for enabling VT processing.</param>
        /// <param name="ansiResetOption">Option for enabling emission of ANSI reset afte printed values.</param>
        /// <param name="quietOption">Option for suppressing error output.</param>
        /// <param name="handler">Command handler.</param>
        public ListCommand(
            Option<ContentType> typeOption,
            Option<bool> allOption,
            Option<bool> ansiOption,
            Option<AnsiResetType> ansiResetOption,
            Option<bool> quietOption,
            Func<IConsoleWriter, IValueFormatter, ContentType, bool, CancellationToken, Task> handler)
            : base("list", "Lists the clipboard history")
        {
            var nullOption = new Option<bool>("--null", "Use the null byte to separate entries");
            this.AddOption(nullOption);

            var indexOption = new Option<bool>("--index", "Print indices with each item");
            this.AddOption(indexOption);

            var idOption = new Option<bool>("--id", "Print the ID (GUID) with each item");
            this.AddOption(idOption);

            var timeOption = new Option<bool>("--time", "Print the date and time that each item was copied");
            this.AddOption(timeOption);

            var pinnedOption = new Option<bool>("--pinned", "Print only pinned items");
            this.AddOption(pinnedOption);

            this.SetHandler<IConsoleWriter, IValueFormatter, ContentType, bool, CancellationToken>(
                handler,
                new ConsoleWriterBinder(ansiOption, ansiResetOption, quietOption),
                new ValueFormatterBinder(nullOption, indexOption, idOption, timeOption),
                new ContentTypeBinder(typeOption, allOption),
                pinnedOption);
        }
    }
}
