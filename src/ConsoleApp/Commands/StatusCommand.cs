using past.ConsoleApp.Binders;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;

namespace past.ConsoleApp.Commands
{
    /// <summary>
    /// Writes the current status of the clipboard history and roaming system settings.
    /// </summary>
    public class StatusCommand : Command
    {
        /// <summary>
        /// Creates a new <see cref="StatusCommand"/> with the given option and handler.
        /// </summary>
        /// <remarks>
        /// The <paramref name="quietOption"/> is not added to the command's options by this constructor,
        /// as it is expected to be set as a global option.
        /// This option is only included as a parameter since it is required for a type binding.
        /// </remarks>
        /// <param name="quietOption">Option for suppressing error output.</param>
        /// <param name="handler">Command handler.</param>
        public StatusCommand(Option<bool> quietOption, Action<IConsoleWriter, InvocationContext> handler)
            : base("status", "Gets the status of the clipboard history settings on this device")
        {
            _ = quietOption ?? throw new ArgumentNullException(nameof(quietOption));
            _ = handler ?? throw new ArgumentNullException(nameof(handler));

            this.SetHandler(handler, new ConsoleWriterBinder(quietOption));
        }
    }
}
