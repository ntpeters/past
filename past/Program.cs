using past.Extensions;
using past.Wrappers;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.CommandLine.Parsing;
using System.CommandLine.Rendering;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;

namespace past
{
    public partial class Program
    {
        // TODO: Remove this. Temporary static instance to help break apart larger refactor.
        private static readonly Lazy<ClipboardManager> _clipboard = new(() => new ClipboardManager());

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

            // TODO: Refactor this
            // All global options must be defined first so they can be passed when setting handlers for subcommands
            var rootCommand = new RootCommand();
            var typeOption = new Option<ContentType>(
                aliases: new string[] { "--type", "-t" },
                description: "The type of content to read from the clipboard. (default: Text)",
                isDefault: true,
                parseArgument: (ArgumentResult result) =>
                {
                    if (result.Tokens.Count == 0)
                    {
                        // Default value
                        return ContentType.Default;
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
            typeOption.AddValidator((OptionResult result) =>
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
            rootCommand.AddGlobalOption(typeOption);
            var allOption = new Option<bool>("--all", "Alias for `--type all`. Overrides the `--type` option if present.");
            rootCommand.AddGlobalOption(allOption);

            var ansiOption = new Option<bool>("--ansi", "Enable processing of ANSI control sequences");
            rootCommand.AddGlobalOption(ansiOption);

            var ansiResetOption = new Option<AnsiResetType>(
                "--ansi-reset",
                description: "Controls whether to emit the ANSI reset escape code after printing an item. Auto will only emit ANSI reset when another ANSI escape sequence is detected in that item.",
                isDefault: true,
                parseArgument: (ArgumentResult result) =>
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
            ansiResetOption.AddValidator((OptionResult result) =>
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
            rootCommand.AddGlobalOption(ansiResetOption);

            var quietOption = new Option<bool>(new string[] { "--quiet", "-q" }, "Suppresses error output");
            var silentOption = new Option<bool>(new string[] { "--silent", "-s" }, "Suppresses all output");
            rootCommand.AddGlobalOption(quietOption);
            rootCommand.AddGlobalOption(silentOption);

            // Include a hidden debug option to use if it's ever needed, and to allow the args to still be parsed successfully
            // when providing the debug flag for attaching a debugger to debug builds.
            var debugOption = new Option<bool>("--debug",
                "Prints additional diagnostic output." +
                "\n[Debug Builds Only] Halts execution on startup to allow attaching a debugger.");
#if !DEBUG
            // Don't show the debug flag in release builds
            debugOption.IsHidden = true;
#endif // DEBUG
            rootCommand.AddGlobalOption(debugOption);

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
            listCommand.SetHandler<IConsole, bool, bool, ContentType, bool, bool, AnsiResetType, bool, bool, bool, bool, bool, CancellationToken>(
                ListClipboardHistoryAsync,
                nullOption, indexOption, typeOption, allOption, ansiOption, ansiResetOption, quietOption, silentOption, idOption, pinnedOption, timeOption);

            var getCommand = new Command("get", "Gets the item at the specified index from clipboard history");
            var indexArgument = new Argument<int>("index", "The index of the item to get from clipboard history");
            getCommand.AddArgument(indexArgument);
            var setCurrentOption = new Option<bool>("--set-current", "Sets the current clipboard contents to the returned history item");
            getCommand.AddOption(setCurrentOption);
            getCommand.SetHandler<IConsole, int, bool, AnsiResetType, bool, ContentType, bool, bool, bool, CancellationToken>(
                GetClipboardHistoryItemAsync,
                indexArgument, ansiOption, ansiResetOption, setCurrentOption, typeOption, allOption, quietOption, silentOption);

            var statusCommand = new Command("status", "Gets the status of the clipboard history settings on this device.");
            statusCommand.SetHandler<InvocationContext, IConsole, bool, bool, CancellationToken>(
                GetClipboardHistoryStatus,
                quietOption, silentOption);

            var helpCommand = new Command("help");
            var commandArgument = new Argument<string>("command");
            commandArgument.SetDefaultValue(string.Empty);
            helpCommand.AddArgument(commandArgument);
            helpCommand.SetHandler<string>(async (string command) =>
                {
                    if (string.IsNullOrWhiteSpace(command))
                    {
                        await Main(new string[] { "--help" });
                    }
                    else
                    {
                        await Main(new string[] { "--help", command });
                    }
                },
                commandArgument);

            rootCommand.AddCommand(listCommand);
            rootCommand.AddCommand(getCommand);
            rootCommand.AddCommand(statusCommand);
            rootCommand.AddCommand(helpCommand);

            rootCommand.SetHandler<IConsole, ContentType, bool, bool, AnsiResetType, bool, bool, CancellationToken>(
                GetCurrentClipboardValueAsync,
                typeOption, allOption, ansiOption, ansiResetOption, quietOption, silentOption);

            return await rootCommand.InvokeAsync(args);
        }

        #region Commands
        private static void GetClipboardHistoryStatus(InvocationContext context, IConsole console, bool quiet, bool silent, CancellationToken cancellationToken)
        {
            try
            {
                console.WriteLine($"Clipboard History Enabled: {_clipboard.Value.IsHistoryEnabled()}", suppressOutput: silent);
                console.WriteLine($"Clipboard Roaming Enabled: {_clipboard.Value.IsRoamingEnabled()}", suppressOutput: silent);
            }
            catch (Exception e)
            {
                console.WriteErrorLine($"Failed to get current clipboard history status. Error: {e}", suppressOutput: quiet || silent);
                context.ExitCode = -1;
            }

            context.ExitCode = 0;
        }

        private static async Task<int> GetCurrentClipboardValueAsync(IConsole console, ContentType type, bool all, bool ansi, AnsiResetType ansiResetType, bool quiet, bool silent, CancellationToken cancellationToken)
        {
            try
            {
                type = ResolveContentType(console, type, all, quiet, silent);
                string? value = await _clipboard.Value.GetCurrentClipboardValueAsync(type, cancellationToken);
                WriteValueToConsole(console, value, ansi: ansi, ansiResetType: ansiResetType, silent: silent);
            }
            catch (Exception e)
            {
                console.WriteErrorLine($"Failed to get current clipboard contents. Error: {e}", suppressOutput: quiet || silent);
                return -1;
            }

            return 0;
        }

        private static async Task<int> GetClipboardHistoryItemAsync(IConsole console, int index, bool ansi, AnsiResetType ansiResetType, bool setCurrent, ContentType type, bool all, bool quiet, bool silent, CancellationToken cancellationToken)
        {
            try
            {
                type = ResolveContentType(console, type, all, quiet, silent);
                if (ansi && !console.IsOutputRedirected && !ConsoleHelpers.TryEnableVirtualTerminalProcessing(out var error))
                {
                    console.WriteErrorLine($"Failed to enable virtual terminal processing. [{error}]", suppressOutput: quiet || silent);
                }

                var (item, setContentStatus) = await _clipboard.Value.GetClipboardHistoryItemAsync(index, setCurrent, type, cancellationToken);
                var value = await GetClipboardItemValueAsync(item, ansi: ansi);
                WriteValueToConsole(console, value, ansi: ansi, ansiResetType: ansiResetType, silent: silent);

                if (setCurrent && setContentStatus != SetHistoryItemAsContentStatus.Success)
                {
                    console.WriteErrorLine($"Failed updating the current clipboard content with the selected history item. Error: {setContentStatus}", suppressOutput: quiet || silent);
                }
            }
            catch (Exception e)
            {
                console.WriteErrorLine($"Failed to get clipboard history. Error: {e}", suppressOutput: quiet || silent);
                return -1;
            }

            return 0;
        }

        private static async Task<int> ListClipboardHistoryAsync(IConsole console, bool @null, bool index, ContentType type, bool all, bool ansi, AnsiResetType ansiResetType, bool quiet, bool silent, bool id, bool pinned, bool time, CancellationToken cancellationToken)
        {
            try
            {
                type = ResolveContentType(console, type, all);

                var clipboardItems = await _clipboard.Value.ListClipboardHistoryAsync(type, pinned, cancellationToken);

                if (ansi && !console.IsOutputRedirected && !ConsoleHelpers.TryEnableVirtualTerminalProcessing(out var error))
                {
                    console.WriteErrorLine($"Failed to enable virtual terminal processing. [{error}]", suppressOutput: quiet || silent);
                }

                int i = 0;
                int outputItemCount = 0;
                foreach (var item in clipboardItems)
                {
                    var value = await GetClipboardItemValueAsync(item, type, ansi);
                    int? printIndex = index ? i : null;
                    string? printId = id ? item.Id : null;
                    string? printTimestamp = time ? item.Timestamp.ToString() : null;
                    bool printNull = outputItemCount < clipboardItems.Count() && @null;
                    if (WriteValueToConsole(console, value, printIndex, printNull, ansi, ansiResetType, silent, printId, printTimestamp))
                    {
                        outputItemCount++;
                    }
                    i++;
                }
            }
            catch (Exception e)
            {
                console.WriteErrorLine($"Failed to get clipboard history. Error: {e}", suppressOutput: quiet || silent);
                return -1;
            }

            return 0;
        }
        #endregion Commands

        #region Helpers
        private static bool WriteValueToConsole(IConsole console, string? value, int? index = null, bool @null = false, bool ansi = false, AnsiResetType ansiResetType = AnsiResetType.Auto, bool silent = false, string? id = null, string? timestamp = null)
        {
            if (value == null)
            {
                return false;
            }

            var outputValue = new StringBuilder();
            if (index != null)
            {
                outputValue.Append($"{index}:");
            }

            if (!string.IsNullOrWhiteSpace(id))
            {
                outputValue.Append($"{id}:");
            }

            if (!string.IsNullOrWhiteSpace(timestamp))
            {
                outputValue.Append($"{timestamp}:");
            }

            outputValue.Append(value);

            switch (ansiResetType)
            {
                case AnsiResetType.Auto:
                    bool shouldEmitAnsiReset = ansi;
                    if (!ansi)
                    {
                        if (console.IsOutputRedirected)
                        {
                            // Don't emit ANSI reset if output was redirected and ANSI processing was not enabled
                            shouldEmitAnsiReset = false;
                        }
                        else
                        {
                            // Emit ANSI reset if the current terminal probably supports ANSI sequences based on TERM and COLORTERM, even if ANSI processing was not enabled
                            var termValue = Environment.GetEnvironmentVariable("TERM");
                            var colorTermValue = Environment.GetEnvironmentVariable("COLORTERM");
                            shouldEmitAnsiReset = termValue == "xterm-256color" || colorTermValue == "24bit" || colorTermValue == "truecolor";
                        }
                    }

                    if (shouldEmitAnsiReset && Regex.IsMatch(value, "\\e\\[[0-9;]*m"))
                    {
                        // Emit ANSI reset if the value contains ANSI escape sequences and either ANSI processing was enabled or the current terminal probably supports them based on TERM and COLORTERM
                        outputValue.Append(Ansi.Text.AttributesOff.EscapeSequence);
                    }
                    break;
                case AnsiResetType.On:
                    outputValue.Append(Ansi.Text.AttributesOff.EscapeSequence);
                    break;
                case AnsiResetType.Off: // Fallthrough
                default: break;
            }

            if (@null)
            {
                outputValue.Append('\0');
            }
            else
            {
                outputValue.Append('\n');
            }

            return console.Write(outputValue.ToString(), suppressOutput: silent);
        }

        private static Task<string?> GetClipboardItemValueAsync(IClipboardHistoryItemWrapper item, ContentType type = ContentType.Text, bool ansi = false)
        {
            return GetClipboardItemValueAsync(item.Content, type, ansi);
        }

        private static async Task<string?> GetClipboardItemValueAsync(DataPackageView content, ContentType type = ContentType.Text, bool ansi = false)
        {
            if ((type == ContentType.Text || type == ContentType.All) && content.Contains(StandardDataFormats.Text))
            {
                return await content.GetTextAsync();
            }
            else if (type == ContentType.All)
            {
                var message = new StringBuilder();
                if (ansi)
                {
                    message.Append(Ansi.Color.Foreground.Red.EscapeSequence);
                }

                message.Append($"[Unsupported Format: {string.Join(',', content.AvailableFormats)}]");
                return message.ToString();
            }

            return null;
        }

        private static ContentType ResolveContentType(IConsole console, ContentType typeOption, bool allOption, bool quiet = false, bool silent = false)
        {
            if (typeOption.TryResolve(allOption, out var resolvedType))
            {
                console.WriteErrorLine($"All option provided, overriding type selection of '{typeOption}'", suppressOutput: quiet || silent);
            }

            return resolvedType;
        }
        #endregion Helpers
    }
}
