using past.Extensions;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.CommandLine.Parsing;
using System.CommandLine.Rendering;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Win32Clipboard = System.Windows.Clipboard;
using WinRtClipboard = Windows.ApplicationModel.DataTransfer.Clipboard;

namespace past
{
    public partial class Program
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
                console.WriteLine($"Clipboard History Enabled: {WinRtClipboard.IsHistoryEnabled()}", suppressOutput: silent);
                console.WriteLine($"Clipboard Roaming Enabled: {WinRtClipboard.IsRoamingEnabled()}", suppressOutput: silent);
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
            // Using the Win32 clipboard API rather than the WinRt clipboard API as that
            // one frequently throws "RPC_E_DISCONNECTED" when trying to access the current
            // clipboard contents.
            try
            {
                type = ResolveContentType(console, type, all, quiet, silent);

                // Accessing the current clipboard must be done on an STA thread
                var tsc = new TaskCompletionSource<string?>();
                var staThread = new Thread(() =>
                {
                    try
                    {
                        string? value = null;
                        if (type == ContentType.Text || type == ContentType.All)
                        {
                            if (Win32Clipboard.ContainsText(System.Windows.TextDataFormat.UnicodeText))
                            {
                                value = Win32Clipboard.GetText(System.Windows.TextDataFormat.UnicodeText);
                            }
                            else if (Win32Clipboard.ContainsText(System.Windows.TextDataFormat.Text))
                            {
                                value = Win32Clipboard.GetText(System.Windows.TextDataFormat.Text);
                            }
                        }
                        else if ((type == ContentType.Image || type == ContentType.All) && Win32Clipboard.ContainsImage())
                        {
                            value = "[Unsupported Format: Image]";
                        }
                        else if ((type == ContentType.Image || type == ContentType.All) && Win32Clipboard.ContainsFileDropList())
                        {
                            value = "[Unsupported Format: File Drop List]";
                        }
                        else if (type == ContentType.All)
                        {
                            var data = Win32Clipboard.GetDataObject();
                            value = $"[Unsupported Format: {string.Join(',', data.GetFormats())}]";
                        }

                        tsc.SetResult(value);
                    }
                    catch (Exception e)
                    {
                        tsc.SetException(e);
                    }
                });

                staThread.SetApartmentState(ApartmentState.STA);
                staThread.Start();

                if (!staThread.Join(millisecondsTimeout: 500))
                {
                    console.WriteErrorLine("Timeout while getting current clipboard contents.", suppressOutput: quiet || silent);
                    return -1;
                }

                var value = await tsc.Task;
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

                var items = await WinRtClipboard.GetHistoryItemsAsync();
                if (items.Status != ClipboardHistoryItemsResultStatus.Success)
                {
                    console.WriteErrorLine($"Failed to get clipboard history. Result: {items.Status}", suppressOutput: quiet || silent);
                    return -1;
                }

                if (ansi && !console.IsOutputRedirected && !ConsoleHelpers.TryEnableVirtualTerminalProcessing(out var error))
                {
                    console.WriteErrorLine($"Failed to enable virtual terminal processing. [{error}]", suppressOutput: quiet || silent);
                }

                var item = items.Items.ElementAt(index);
                var value = await GetClipboardItemValueAsync(item, ansi: ansi);
                WriteValueToConsole(console, value, ansi: ansi, ansiResetType: ansiResetType, silent: silent);

                if (setCurrent)
                {
                    var setContentStatus = WinRtClipboard.SetHistoryItemAsContent(item);
                    if (setContentStatus != SetHistoryItemAsContentStatus.Success)
                    {
                        console.WriteErrorLine($"Failed updating the current clipboard content with the selected history item. Error: {setContentStatus}", suppressOutput: quiet || silent);
                    }
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

                var items = await WinRtClipboard.GetHistoryItemsAsync();
                if (items.Status != ClipboardHistoryItemsResultStatus.Success)
                {
                    console.WriteErrorLine($"Failed to get clipboard history. Result: {items.Status}", suppressOutput: quiet || silent);
                    return -1;
                }

                if (items.Items.Count == 0)
                {
                    console.WriteErrorLine("Clipboard history is empty", suppressOutput: quiet || silent);
                    return 1;
                }

                IEnumerable<ClipboardHistoryItem> clipboardItems;
                if (pinned)
                {
                    // TODO: Get pinned clipboard history items
                    // Pinned item IDs can be read from: %LOCALAPPDATA%/Microsoft/Windows/Clipboard/Pinned/{GUID}/metadata.json
                    var localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                    var pinnedClipboardPath = Path.Combine(localAppDataPath, "Microsoft/Windows/Clipboard/Pinned");
                    var pinnedClipboardItemMetadataDirectory = Directory.EnumerateDirectories(pinnedClipboardPath).First(directoryPath => File.Exists(Path.Combine(directoryPath, "metadata.json")));
                    string? pinnedClipboardItemMetadataPath = null;
                    foreach (var directoryPath in Directory.EnumerateDirectories(pinnedClipboardPath))
                    {
                        var metadataPath = Path.Combine(directoryPath, "metadata.json");
                        if (File.Exists(metadataPath))
                        {
                            pinnedClipboardItemMetadataPath = metadataPath;
                            break;
                        }
                    }
                    if (string.IsNullOrWhiteSpace(pinnedClipboardItemMetadataPath))
                    {
                        console.WriteErrorLine("Failed to retrieve pinned clipboard history items", suppressOutput: quiet || silent);
                        return -1;
                    }

                    var pinnedClipboardItemMetadataJson = File.ReadAllText(pinnedClipboardItemMetadataPath);
                    var pinnedClipboardItemMetadata = JsonDocument.Parse(pinnedClipboardItemMetadataJson);
                    var pinnedClipboardItemIds = pinnedClipboardItemMetadata.RootElement.GetProperty("items").EnumerateObject().Select(property => property.Name);
                    clipboardItems = items.Items.Where(item => pinnedClipboardItemIds.Contains(item.Id));
                    if (clipboardItems.Count() == 0)
                    {
                        console.WriteErrorLine("No pinned items in clipboard history", suppressOutput: quiet || silent);
                        return 2;
                    }
                }
                else
                {
                    clipboardItems = items.Items;
                }

                var filteredItemCount = clipboardItems.Count(item => type.Supports(item.Content.Contains));
                if (filteredItemCount == 0)
                {
                    console.WriteErrorLine("No supported items in clipboard history", suppressOutput: quiet || silent);
                    return 2;
                }

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
                    bool printNull = outputItemCount < filteredItemCount && @null;
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

        private static Task<string?> GetClipboardItemValueAsync(ClipboardHistoryItem item, ContentType type = ContentType.Text, bool ansi = false)
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
