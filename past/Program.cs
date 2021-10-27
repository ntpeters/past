using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.CommandLine.Parsing;
using System.CommandLine.Rendering;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using WinRtClipboard = Windows.ApplicationModel.DataTransfer.Clipboard;
using Win32Clipboard = System.Windows.Clipboard;
using System.IO;
using past.Extensions;
using System.Text.RegularExpressions;
using System.Text.Json;

namespace past
{
    public partial class Program
    {
        private enum AnsiResetType
        {
            Auto,
            On,
            Off
        }

        public static async Task<int> Main(string[] args)
        {
            var listCommand = new Command("list", "Lists the clipboard history");
            var nullOption = new Option<bool>("--null", "Use the null byte to separate entries");
            listCommand.AddOption(nullOption);
            var indexOption = new Option<bool>("--index", "Print indices with each item");
            listCommand.AddOption(indexOption);
            var idOption = new Option<bool>("--id", "Print the ID (GUID) with each item");
            listCommand.AddOption(idOption);
            var pinnedOption = new Option<bool>("--pinned", "Print only pinned items");
            listCommand.AddOption(pinnedOption);
            listCommand.Handler = CommandHandler.Create<IConsole, bool, bool, ContentType, bool, bool, AnsiResetType, bool, bool, bool, bool, CancellationToken>(ListClipboardHistoryAsync);

            var getCommand = new Command("get", "Gets the item at the specified index from clipboard history");
            var indexArgument = new Argument<int>("index", "The index of the item to get from clipboard history");
            getCommand.AddArgument(indexArgument);
            var setCurrentOption = new Option<bool>("--set-current", "Sets the current clipboard contents to the returned history item");
            getCommand.AddOption(setCurrentOption);
            getCommand.Handler = CommandHandler.Create<IConsole, int, bool, AnsiResetType, bool, ContentType, bool, bool, bool, CancellationToken>(GetClipboardHistoryItemAsync);

            var rootCommand = new RootCommand();
            rootCommand.AddCommand(listCommand);
            rootCommand.AddCommand(getCommand);

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

                    var typeValue = result.Tokens?.FirstOrDefault()?.Value;
                    if (!Enum.TryParse<ContentType>(typeValue, ignoreCase: true, out var type))
                    {
                        // For some reason this version of System.CommandLine still executes the parse argument delegate
                        // even when argument validation fails, so we'll just return the default type here since it won't
                        // be used anyway...
                        return ContentType.Text;
                    }

                    return type;
                });
            typeOption.AddSuggestions(Enum.GetNames<ContentType>());
            typeOption.AddValidator((OptionResult result) =>
            {
                var typeValue = result.Tokens?.FirstOrDefault()?.Value;
                if (result.Token != null && !Enum.TryParse<ContentType>(typeValue, ignoreCase: true, out var type))
                {
                    return $"Invalid type specified. Valid values are: {string.Join(',', Enum.GetNames<ContentType>())}";
                }
                return null;
            });
            rootCommand.AddGlobalOption(typeOption);
            var allOption = new Option<bool>("--all", "Alias for `--type all`. Overrides the `--type` option if present.");
            rootCommand.AddGlobalOption(allOption);

            var ansiOption = new Option<bool>("--ansi", "Enable processing of ANSI control sequences");
            rootCommand.AddGlobalOption(ansiOption);

            var ansiResetOption = new Option<AnsiResetType>(
                alias: "--ansi-reset",
                description: "Controls whether to emit the ANSI reset escape code after printing an item. Auto will only emit ANSI reset when another ANSI escape sequence is detected in that item.",
                isDefault: true,
                parseArgument: (ArgumentResult result) =>
                {
                    if (result.Tokens.Count == 0)
                    {
                        // Default value
                        return AnsiResetType.Auto;
                    }

                    var typeValue = result.Tokens?.FirstOrDefault()?.Value;
                    if (!Enum.TryParse<AnsiResetType>(typeValue, ignoreCase: true, out var type))
                    {
                        // For some reason this version of System.CommandLine still executes the parse argument delegate
                        // even when argument validation fails, so we'll just return the default type here since it won't
                        // be used anyway...
                        return AnsiResetType.Auto;
                    }

                    return type;
                });
            ansiResetOption.AddSuggestions(Enum.GetNames<AnsiResetType>());
            ansiResetOption.AddValidator((OptionResult result) =>
            {
                var typeValue = result.Tokens?.FirstOrDefault()?.Value;
                if (result.Token != null && !Enum.TryParse<AnsiResetType>(typeValue, ignoreCase: true, out var type))
                {
                    return $"Invalid type specified. Valid values are: {string.Join(',', Enum.GetNames<AnsiResetType>())}";
                }
                return null;
            });
            rootCommand.AddGlobalOption(ansiResetOption);

            var quietOption = new Option<bool>(new string[] { "--quiet", "-q" }, "Suppresses error output");
            var silentOption = new Option<bool>(new string[] { "--silent", "-s" }, "Suppresses all output");
            rootCommand.AddGlobalOption(quietOption);
            rootCommand.AddGlobalOption(silentOption);

            rootCommand.Handler = CommandHandler.Create<IConsole, ContentType, bool, bool, AnsiResetType, bool, bool, CancellationToken>(GetCurrentClipboardValueAsync);

            return await rootCommand.InvokeAsync(args);
        }

        #region Commands
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

        private static async Task<int> ListClipboardHistoryAsync(IConsole console, bool @null, bool index, ContentType type, bool all, bool ansi, AnsiResetType ansiResetType, bool quiet, bool silent, bool id, bool pinned, CancellationToken cancellationToken)
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

                var filteredItemCount = items.Items.Count(item => type.Supports(item.Content.Contains));
                if (filteredItemCount == 0)
                {
                    console.WriteErrorLine("No supported items in clipboard history", suppressOutput: quiet || silent);
                    return 2;
                }

                if (ansi && !console.IsOutputRedirected && !ConsoleHelpers.TryEnableVirtualTerminalProcessing(out var error))
                {
                    console.WriteErrorLine($"Failed to enable virtual terminal processing. [{error}]", suppressOutput: quiet || silent);
                }

                if (pinned)
                {
                    // TODO: Get pinned clipboard history items
                    // Pinned item IDs can be read from:
                    // "C:/Users/nathpete/AppData/Local/Microsoft/Windows/Clipboard/Pinned/{B4A94277-E0ED-4617-8152-9A862BC2E5F0}/metadata.json"
                    console.WriteErrorLine("Listing pinned clipboard history items is not yet supported", suppressOutput: quiet || silent);
                    return -1;
                }

                int i = 0;
                foreach (var item in items.Items)
                {
                    var value = await GetClipboardItemValueAsync(item, type, ansi);
                    int? printIndex = index ? i : null;
                    string? printId = id ? item.Id : null;
                    bool printNull = i < filteredItemCount - 1 && @null;
                    WriteValueToConsole(console, value, printIndex, printNull, ansi, ansiResetType, silent, printId);
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
        private static void WriteValueToConsole(IConsole console, string? value, int? index = null, bool @null = false, bool ansi = false, AnsiResetType ansiResetType = AnsiResetType.Auto, bool silent = false, string? id = null)
        {
            if (value == null)
            {
                return;
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

            console.Write(outputValue.ToString(), suppressOutput: silent);
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
