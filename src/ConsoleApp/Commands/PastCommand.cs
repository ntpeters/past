using past.ConsoleApp.Binders;
using past.ConsoleApp.Output;
using past.Core;
using past.Core.Models;
using System;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Threading;
using System.Threading.Tasks;

namespace past.ConsoleApp.Commands
{
    /// <summary>
    /// Writes the current value stored on the system clipboard, when run with no subcommand.
    /// Provides subcommands for interacting with the clipboard history.
    /// </summary>
    public class PastCommand : RootCommand
    {
        /// <summary>
        /// Creates a new <see cref="PastCommand"/> with the system clipboard.
        /// </summary>
        public PastCommand()
            : this(new ConsoleClipboard())
        {
        }

        /// <summary>
        /// Creates a new <see cref="PastCommand"/> using the given <see cref="IConsoleClipboard"/> for handlers.
        /// </summary>
        /// <param name="consoleClipboard"><see cref="IConsoleClipboard"/> providing the handlers for most commands.</param>
        public PastCommand(IConsoleClipboard consoleClipboard)
            : base("Gets the current contents of the clipboard, when no subcommand is specified.")
        {
            _ = consoleClipboard ?? throw new ArgumentNullException(nameof(consoleClipboard));

            // Ensure command name is stable for testing
            Name = "past";

            // Shared Arguments & Options
            var identifierArgument = CreateIdentifierArgument();
            identifierArgument.HelpName = "index|id";
            var typeOption = CreateTypeOption();
            var allOption = new Option<bool>("--all", "Alias for `--type all`. Overrides the `--type` option if present.");
            var ansiOption = new Option<bool>("--ansi", "Enable processing of ANSI control sequences");
            var ansiResetOption = CreateAnsiResetOption();

            // Global Options
            var quietOption = new Option<bool>(new string[] { "--quiet", "-q" }, "Suppresses error output");

            // Include a hidden debug option to use if it's ever needed, and to allow the args to still be parsed successfully
            // when providing the debug flag for attaching a debugger to debug builds.
            var debugOption = new Option<bool>("--debug",
                "Prints additional diagnostic output." +
                "\n[Debug Builds Only] Halts execution on startup to allow attaching a debugger.");
#if !DEBUG
            // Don't show the debug flag in release builds
            debugOption.IsHidden = true;
#endif // DEBUG

            this.AddOption(typeOption);
            this.AddOption(allOption);
            this.AddOption(ansiOption);
            this.AddOption(ansiResetOption);
            this.AddGlobalOption(quietOption);
            this.AddGlobalOption(debugOption);

            var listCommand = new ListCommand(
                typeOption,
                allOption,
                ansiOption,
                ansiResetOption,
                quietOption,
                consoleClipboard.ListClipboardHistoryAsync);

            var getCommand = new GetCommand(
                identifierArgument,
                typeOption,
                allOption,
                ansiOption,
                ansiResetOption,
                quietOption,
                consoleClipboard.GetClipboardHistoryItemAsync);

            var statusCommand = new StatusCommand(quietOption, consoleClipboard.GetClipboardHistoryStatus);

            var pinCommand = new PinCommand(
                identifierArgument,
                quietOption,
                consoleClipboard.PinClipboardItemAsync);

            var unpinCommand = new UnpinCommand(
                identifierArgument,
                quietOption,
                consoleClipboard.UnpinClipboardItemAsync);

            this.AddCommand(listCommand);
            this.AddCommand(getCommand);
            this.AddCommand(statusCommand);
            this.AddCommand(pinCommand);
            this.AddCommand(unpinCommand);

            SetHandler(
                consoleClipboard.GetCurrentClipboardValueAsync,
                new ConsoleWriterBinder(ansiOption, ansiResetOption, quietOption),
                new ValueFormatterBinder(),
                new ContentTypeBinder(typeOption, allOption));
        }

        /// <summary>
        /// Sets the command's handler to the provided <paramref name="handler"/> with the given binders.
        /// </summary>
        /// <remarks>
        /// This is a thin, strongly typed wrapper around <see cref="Handler.SetHandler{T1, T2, T3, T4}(Command, Func{T1, T2, T3, T4, Task}, System.CommandLine.Binding.IValueDescriptor[])"/>.
        /// <br/>
        /// The only purpose of this method is to ensure that the handler parameter is strongly typed so that the original concrete type
        /// is preserved for handler type validation in tests.
        /// </remarks>
        /// <param name="handler">Command handler.</param>
        /// <param name="consoleWriterBinder">Binder for <see cref="ConsoleWriter"/>.</param>
        /// <param name="valueFormatterBinder">Binder for <see cref="ValueFormatter"/>.</param>
        /// <param name="contentTypeBinder">Binder for <see cref="ContentType"/>.</param>
        private void SetHandler(
            Func<IConsoleWriter, IValueFormatter, ContentType, CancellationToken, Task<int>> handler, ConsoleWriterBinder consoleWriterBinder,
            ValueFormatterBinder valueFormatterBinder,
            ContentTypeBinder contentTypeBinder
            )
        {
            this.SetHandler<IConsoleWriter, IValueFormatter, ContentType, CancellationToken>(
                handler,
                consoleWriterBinder,
                valueFormatterBinder,
                contentTypeBinder);
        }

        /// <summary>
        /// Creates the identifier argument.
        /// </summary>
        /// <returns>The created argument.</returns>
        private static Argument<string> CreateIdentifierArgument()
        {
            var identifierArgument = new Argument<string>("identifier", "The index or ID of the item to get from clipboard history");

            identifierArgument.AddValidator(result =>
            {
                string? typeValue = null;
                if (result.Tokens.Count > 0)
                {
                    typeValue = result.Tokens[0].Value;
                }

                if (!ClipboardItemIdentifier.TryParse(typeValue, out _))
                {
                    result.ErrorMessage = $"Invalid identifier specified. Identifier must be either a positive integer or a GUID.";
                }
            });

            return identifierArgument;
        }

        /// <summary>
        /// Creates the ANSI reset option.
        /// </summary>
        /// <returns>The created option.</returns>
        private static Option<AnsiResetType> CreateAnsiResetOption()
        {
            var ansiResetOption = new Option<AnsiResetType>(
                "--ansi-reset",
                description: "Controls whether to emit the ANSI reset escape code after printing an item. Auto will only emit ANSI reset when another ANSI escape sequence is detected in that item.",
                isDefault: true,
                parseArgument: (result) => EnumArgumentParser(result, AnsiResetType.Auto));

            ansiResetOption.AddCompletions(Enum.GetNames<AnsiResetType>());
            ansiResetOption.AddValidator(EnumOptionValidator<AnsiResetType>);

            return ansiResetOption;
        }

        /// <summary>
        /// Creates the type option.
        /// </summary>
        /// <returns>The created option.</returns>
        private static Option<ContentType> CreateTypeOption()
        {
            var typeOption = new Option<ContentType>(
                aliases: new string[] { "--type", "-t" },
                description: "The type of content to read from the clipboard.",
                isDefault: true,
                parseArgument: (result) => EnumArgumentParser(result, ContentType.Text));

            typeOption.AddCompletions(Enum.GetNames<ContentType>());
            typeOption.AddValidator(EnumOptionValidator<ContentType>);

            return typeOption;
        }

        /// <summary>
        /// Parses the value from a <paramref name="result"/> as a <typeparamref name="TEnum"/>.
        /// </summary>
        /// <typeparam name="TEnum">Type of the enum value to parse.</typeparam>
        /// <param name="result">Result to parse the enum value from.</param>
        /// <param name="defaultValue">Default value to use when the argument was not provided.</param>
        /// <returns>The parsed value.</returns>
        private static TEnum EnumArgumentParser<TEnum>(ArgumentResult result, TEnum defaultValue)
            where TEnum : struct, Enum
        {
            if (result.Tokens.Count == 0)
            {
                // Default value
                return defaultValue;
            }

            string? typeValue = null;
            if (result.Tokens.Count > 0)
            {
                typeValue = result.Tokens[0].Value;
            }

            if (!Enum.TryParse<TEnum>(typeValue, ignoreCase: true, out var type))
            {
                // For some reason this version of System.CommandLine still executes the parse argument delegate
                // even when argument validation fails, so we'll just return the default type here since it won't
                // be used anyway...
                return defaultValue;
            }

            return type;
        }

        /// <summary>
        /// Validates the value of a <paramref name="result"/> is a valid <typeparamref name="TEnum"/> value.
        /// </summary>
        /// <typeparam name="TEnum">Type of the enum to validate the value for.</typeparam>
        /// <param name="result">Result to validate the value from.</param>
        private static void EnumOptionValidator<TEnum>(OptionResult result)
            where TEnum : struct, Enum
        {
            string? typeValue = null;
            if (result.Tokens.Count > 0)
            {
                typeValue = result.Tokens[0].Value;
            }

            if (!string.IsNullOrWhiteSpace(result.Token.Value) && !Enum.TryParse<TEnum>(typeValue, ignoreCase: true, out _))
            {
                result.ErrorMessage = $"Invalid type specified. Valid values are: {string.Join(',', Enum.GetNames<TEnum>())}";
            }
        }
    }
}
