using past.ConsoleApp.Binders;
using past.ConsoleApp.Output;
using past.Core;
using past.Core.Models;
using System;
using System.CommandLine;
using System.Threading;
using System.Threading.Tasks;

namespace past.ConsoleApp.Commands
{
    /// <summary>
    /// Writes the value of the specified item from clipboard history.
    /// </summary>
    public class GetCommand : Command
    {
        /// <summary>
        /// Creates a new <see cref="PinCommand"/> with the given arugment, options, and handler.
        /// </summary>
        /// <remarks>
        /// The <paramref name="quietOption"/> is not added to the command's options by this constructor,
        /// as it is expected to be set as a global option.
        /// This option is only included as a parameter since it is required for a type binding.
        /// </remarks>
        /// <param name="identifierArgument">Argument for identifying an item in the clipboard history.</param>
        /// <param name="typeOption">Option for specifying content type.</param>
        /// <param name="allOption">Option alias for ContentType.All</param>
        /// <param name="ansiOption">Option for enabling VT processing.</param>
        /// <param name="ansiResetOption">Option for enabling emission of ANSI reset afte printed values.</param>
        /// <param name="quietOption">Option for suppressing error output.</param>
        /// <param name="handler">Command handler.</param>
        public GetCommand(
            Argument<string> identifierArgument,
            Option<ContentType> typeOption,
            Option<bool> allOption,
            Option<bool> ansiOption,
            Option<AnsiResetType> ansiResetOption,
            Option<bool> quietOption,
            Func<IConsoleWriter, IValueFormatter, ClipboardItemIdentifier, ContentType, bool, CancellationToken, Task<int>> handler)
            : base("get", "Gets the item at the specified index from clipboard history")
        {
            _ = identifierArgument ?? throw new ArgumentNullException(nameof(identifierArgument));
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

            this.AddArgument(identifierArgument);
            var setCurrentOption = new Option<bool>("--set-current", "Sets the current clipboard contents to the returned history item");

            this.AddOption(setCurrentOption);

            this.SetHandler<IConsoleWriter, IValueFormatter, ClipboardItemIdentifier, ContentType, bool, CancellationToken>(
                handler,
                new ConsoleWriterBinder(ansiOption, ansiResetOption, quietOption),
                new ValueFormatterBinder(),
                new ClipboardItemIdentifierBinder(identifierArgument),
                new ContentTypeBinder(typeOption, allOption),
                setCurrentOption);
        }
    }
}
