using past.ConsoleApp.Binders;
using past.ConsoleApp.Output;
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
        /// <remarks>
        /// The <paramref name="quietOption"/> is not added to the command's options by this constructor,
        /// as it is expected to be set as a global option.
        /// This option is only included as a parameter since it is required for a type binding.
        /// </remarks>
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
            Func<IConsoleWriter, IValueFormatter, ContentType, bool, CancellationToken, Task<int>> handler)
            : base("list", "Lists the clipboard history")
        {
            _ = typeOption ?? throw new ArgumentNullException(nameof(typeOption));
            _ = allOption ?? throw new ArgumentNullException(nameof(allOption));
            _ = ansiOption ?? throw new ArgumentNullException(nameof(ansiOption));
            _ = ansiResetOption ?? throw new ArgumentNullException(nameof(ansiResetOption));
            _ = quietOption ?? throw new ArgumentNullException(nameof(quietOption));
            _ = handler ?? throw new ArgumentNullException(nameof(handler));

            // Add Shared Options
            this.AddOption(typeOption);
            this.AddOption(allOption);
            this.AddOption(ansiOption);
            this.AddOption(ansiResetOption);

            // Add List Specific Options
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
