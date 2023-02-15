using past.ConsoleApp.Binders;
using past.ConsoleApp.Output;
using past.Core.Models;
using System;
using System.CommandLine;
using System.Threading;
using System.Threading.Tasks;

namespace past.ConsoleApp.Commands
{
    /// <summary>
    /// Pins an item in clipboard history.
    /// </summary>
    public class PinCommand : Command
    {
        /// <summary>
        /// Creates a new <see cref="PinCommand"/> with the given argument, option, and handler.
        /// </summary>
        /// <remarks>
        /// The <paramref name="quietOption"/> is not added to the command's options by this constructor,
        /// as it is expected to be set as a global option.
        /// This option is only included as a parameter since it is required for a type binding.
        /// </remarks>
        /// <param name="identifierArgument">Argument for identifying an item in the clipboard history.</param>
        /// <param name="quietOption">Option for suppressing error output.</param>
        /// <param name="handler">Command handler.</param>
        /// <exception cref="ArgumentNullException"><paramref name="identifierArgument"/>, <paramref name="quietOption"/>, or <paramref name="handler"/> is null.</exception>
        public PinCommand(
            Argument<string> identifierArgument,
            Option<bool> quietOption,
            Func<IConsoleWriter, ClipboardItemIdentifier, CancellationToken, Task<int>> handler)
            : base("pin", "[Experimental] Pins the specified item from clipboard history")
        {
            _ = identifierArgument ?? throw new ArgumentNullException(nameof(identifierArgument));
            _ = quietOption ?? throw new ArgumentNullException(nameof(quietOption));
            _ = handler ?? throw new ArgumentNullException(nameof(handler));

            this.AddArgument(identifierArgument);

            this.SetHandler<IConsoleWriter, ClipboardItemIdentifier, CancellationToken>(
                handler,
                new ConsoleWriterBinder(quietOption),
                new ClipboardItemIdentifierBinder(identifierArgument));
        }
    }
}
