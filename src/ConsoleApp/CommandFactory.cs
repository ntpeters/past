using past.ConsoleApp.Binders;
using past.Core;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading;
using System.Threading.Tasks;

namespace past.ConsoleApp
{
    public class CommandFactory
    {
        #region Private Fields
        private readonly Lazy<Option<ContentType>> _typeOptionLazy;
        private readonly Lazy<Option<bool>> _allOptionLazy;
        private readonly Lazy<Option<bool>> _ansiOptionLazy;
        private readonly Lazy<Option<AnsiResetType>> _ansiResetOptionLazy;
        private readonly Lazy<Option<bool>> _quietOptionLazy;
        private readonly Lazy<Option<bool>> _debugOptionLazy;
        private readonly Lazy<Argument<string>> _identifierArgumentLazy;
        #endregion Private Fields

        #region Public Properties
        public Option<ContentType> TypeOption => _typeOptionLazy.Value;
        public Option<bool> AllOption => _allOptionLazy.Value;
        public Option<bool> AnsiOption => _ansiOptionLazy.Value;
        public Option<AnsiResetType> AnsiResetOption => _ansiResetOptionLazy.Value;
        public Option<bool> QuietOption => _quietOptionLazy.Value;
        public Option<bool> DebugOption => _debugOptionLazy.Value;
        public Argument<string> IdentifierArgument => _identifierArgumentLazy.Value;
        #endregion Public Properties

        public CommandFactory()
        {
            _typeOptionLazy = new(() => CreateTypeOption());
            _allOptionLazy = new(() => CreateAllOption());
            _ansiOptionLazy = new(() => CreateAnsiOption());
            _ansiResetOptionLazy = new(() => CreateAnsiResetOption());
            _quietOptionLazy = new(() => CreateQuietOption());
            _debugOptionLazy = new(() => CreateDebugOption());
            _identifierArgumentLazy = new(() => CreateIdentifierArgument());
        }

        #region Commands
        public Command CreateListCommand(Func<IConsoleWriter, IValueFormatter, ContentType, bool, CancellationToken, Task> handle)
        {
            var listCommand = new Command("list", "Lists the clipboard history");
            var nullOption = new Option<bool>("--null", "Use the null byte to separate entries");
            listCommand.AddOption(nullOption);
            var indexOption = new Option<bool>("--index", "Print indices with each item");
            listCommand.AddOption(indexOption);
            var idOption = new Option<bool>("--id", "Print the ID (GUID) with each item");
            listCommand.AddOption(idOption);
            var timeOption = new Option<bool>("--time", "Print the date and time that each item was copied");
            listCommand.AddOption(timeOption);
            var pinnedOption = new Option<bool>("--pinned", "Print only pinned items");
            listCommand.AddOption(pinnedOption);
            listCommand.SetHandler<IConsoleWriter, IValueFormatter, ContentType, bool, CancellationToken>(
                handle,
                new ConsoleWriterBinder(AnsiOption, AnsiResetOption, QuietOption),
                new ValueFormatterBinder(nullOption, indexOption, idOption, timeOption),
                new ContentTypeBinder(TypeOption, AllOption),
                pinnedOption);
            return listCommand;
        }

        public Command CreateGetCommand(Func<IConsoleWriter, IValueFormatter, ClipboardItemIdentifier, ContentType, bool, CancellationToken, Task> handle)
        {
            var getCommand = new Command("get", "Gets the item at the specified index from clipboard history");
            getCommand.AddArgument(IdentifierArgument);
            var setCurrentOption = new Option<bool>("--set-current", "Sets the current clipboard contents to the returned history item");
            getCommand.AddOption(setCurrentOption);
            getCommand.SetHandler<IConsoleWriter, IValueFormatter, ClipboardItemIdentifier, ContentType, bool, CancellationToken>(
                handle,
                new ConsoleWriterBinder(AnsiOption, AnsiResetOption, QuietOption),
                new ValueFormatterBinder(),
                new ClipboardItemIdentifierBinder(IdentifierArgument),
                new ContentTypeBinder(TypeOption, AllOption),
                setCurrentOption);
            return getCommand;
        }

        public Command CreateStatusCommand(Action<IConsoleWriter, InvocationContext> handle)
        {
            var statusCommand = new Command("status", "Gets the status of the clipboard history settings on this device.");
            statusCommand.SetHandler(
                handle,
                new ConsoleWriterBinder(QuietOption));
            return statusCommand;
        }

        public Command CreateHelpCommand(Func<string, Task> handle)
        {
            var helpCommand = new Command("help");
            var commandArgument = new Argument<string>("command");
            commandArgument.SetDefaultValue(string.Empty);
            helpCommand.AddArgument(commandArgument);
            helpCommand.SetHandler(handle,
                commandArgument);
            return helpCommand;
        }
        #endregion Commands

        #region Options
        private Option<ContentType> CreateTypeOption()
        {
            var typeOption = new Option<ContentType>(
                aliases: new string[] { "--type", "-t" },
                description: "The type of content to read from the clipboard. (default: Text)",
                isDefault: true,
                parseArgument: (result) =>
                {
                    if (result.Tokens.Count == 0)
                    {
                        // Default value
                        return ContentType.Text;
                    }

                    string? typeValue = null;
                    if (result.Tokens?.Count > 0)
                    {
                        typeValue = result.Tokens[0].Value;
                    }

                    if (!Enum.TryParse<ContentType>(typeValue, ignoreCase: true, out var type))
                    {
                        // For some reason this version of System.CommandLine still executes the parse argument delegate
                        // even when argument validation fails, so we'll just return the default type here since it won't
                        // be used anyway...
                        return ContentType.Text;
                    }

                    return type;
                });

            typeOption.AddCompletions(Enum.GetNames<ContentType>());

            typeOption.AddValidator((result) =>
            {
                string? typeValue = null;
                if (result.Tokens?.Count > 0)
                {
                    typeValue = result.Tokens[0].Value;
                }

                if (!string.IsNullOrWhiteSpace(result.Token.Value) && !Enum.TryParse<ContentType>(typeValue, ignoreCase: true, out var type))
                {
                    result.ErrorMessage = $"Invalid type specified. Valid values are: {string.Join(',', Enum.GetNames<ContentType>())}";
                }
            });

            return typeOption;
        }

        private Option<bool> CreateAllOption()
        {
            var allOption = new Option<bool>("--all", "Alias for `--type all`. Overrides the `--type` option if present.");
            return allOption;
        }

        private Option<bool> CreateAnsiOption()
        {
            var ansiOption = new Option<bool>("--ansi", "Enable processing of ANSI control sequences");
            return ansiOption;
        }

        private Option<AnsiResetType> CreateAnsiResetOption()
        {
            var ansiResetOption = new Option<AnsiResetType>(
                "--ansi-reset",
                description: "Controls whether to emit the ANSI reset escape code after printing an item. Auto will only emit ANSI reset when another ANSI escape sequence is detected in that item.",
                isDefault: true,
                parseArgument: (result) =>
                {
                    if (result.Tokens.Count == 0)
                    {
                        // Default value
                        return AnsiResetType.Auto;
                    }

                    string? typeValue = null;
                    if (result.Tokens?.Count > 0)
                    {
                        typeValue = result.Tokens[0].Value;
                    }

                    if (!Enum.TryParse<AnsiResetType>(typeValue, ignoreCase: true, out var type))
                    {
                        // For some reason this version of System.CommandLine still executes the parse argument delegate
                        // even when argument validation fails, so we'll just return the default type here since it won't
                        // be used anyway...
                        return AnsiResetType.Auto;
                    }

                    return type;
                });

            ansiResetOption.AddCompletions(Enum.GetNames<AnsiResetType>());

            ansiResetOption.AddValidator((result) =>
            {
                string? typeValue = null;
                if (result.Tokens?.Count > 0)
                {
                    typeValue = result.Tokens[0].Value;
                }

                if (!string.IsNullOrWhiteSpace(result.Token.Value) && !Enum.TryParse<AnsiResetType>(typeValue, ignoreCase: true, out var type))
                {
                    result.ErrorMessage = $"Invalid type specified. Valid values are: {string.Join(',', Enum.GetNames<AnsiResetType>())}";
                }
            });

            return ansiResetOption;
        }

        private Option<bool> CreateQuietOption()
        {
            var quietOption = new Option<bool>(new string[] { "--quiet", "-q" }, "Suppresses error output");
            return quietOption;
        }

        private Option<bool> CreateDebugOption()
        {
            // Include a hidden debug option to use if it's ever needed, and to allow the args to still be parsed successfully
            // when providing the debug flag for attaching a debugger to debug builds.
            var debugOption = new Option<bool>("--debug",
                "Prints additional diagnostic output." +
                "\n[Debug Builds Only] Halts execution on startup to allow attaching a debugger.");
#if !DEBUG
            // Don't show the debug flag in release builds
            debugOption.IsHidden = true;
#endif // DEBUG
            return debugOption;
        }
        #endregion Options

        #region Arguments
        public Argument<string> CreateIdentifierArgument()
        {
            return new Argument<string>("identifier", "The identifier of the item to get from clipboard history");
        }
        #endregion Arguments
    }
}
