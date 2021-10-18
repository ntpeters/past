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

namespace past
{
    public partial class Program
    {
        public static async Task<int> Main(string[] args)
        {
            var listCommand = new Command("list", "Lists the clipboard history");
            var nullOption = new Option<bool>("--nul", "Use the null byte to separate entries");
            listCommand.AddOption(nullOption);
            var indexOption = new Option<bool>("--index", "Print indices with each item");
            listCommand.AddOption(indexOption);
            listCommand.Handler = CommandHandler.Create<IConsole, bool, bool, ContentType, bool, bool, bool, bool, CancellationToken>(ListClipboardHistoryAsync);

            var getCommand = new Command("get", "Gets the item at the specified index from clipboard history");
            var indexArgument = new Argument<int>("index", "The index of the item to get from clipboard history");
            getCommand.AddArgument(indexArgument);
            var setCurrentOption = new Option<bool>("--set-current", "Sets the current clipboard contents to the returned history item");
            getCommand.AddOption(setCurrentOption);
            getCommand.Handler = CommandHandler.Create<IConsole, int, bool, bool, ContentType, bool, bool, bool, CancellationToken>(GetClipboardHistoryItemAsync);

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
            var allOption = new Option<bool>("--all", "Alias for `--type all`");
            rootCommand.AddGlobalOption(allOption);

            var ansiOption = new Option<bool>("--ansi", "Enable processing of ANSI control sequences");
            rootCommand.AddGlobalOption(ansiOption);

            var quietOption = new Option<bool>(new string[] { "--quiet", "-q" }, "Suppresses error output");
            var silentOption = new Option<bool>(new string[] { "--silent", "-s" }, "Suppresses all output");
            rootCommand.AddGlobalOption(quietOption);
            rootCommand.AddGlobalOption(silentOption);

            rootCommand.Handler = CommandHandler.Create<IConsole, ContentType, bool, bool, bool, bool, CancellationToken>(GetCurrentClipboardValueAsync);

            return await rootCommand.InvokeAsync(args);
        }

        #region Commands
        private static async Task<int> GetCurrentClipboardValueAsync(IConsole console, ContentType type, bool all, bool ansi, bool quiet, bool silent, CancellationToken cancellationToken)
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
                WriteValueToConsole(console, value, ansi: ansi, silent: silent);
            }
            catch (Exception e)
            {
                console.WriteErrorLine($"Failed to get current clipboard contents. Error: {e}", suppressOutput: quiet || silent);
                return -1;
            }

            return 0;
        }

        private static async Task<int> GetClipboardHistoryItemAsync(IConsole console, int index, bool ansi, bool setCurrent, ContentType type, bool all, bool quiet, bool silent, CancellationToken cancellationToken)
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
                WriteValueToConsole(console, value, ansi: ansi, silent: silent);

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

        private static async Task<int> ListClipboardHistoryAsync(IConsole console, bool nul, bool index, ContentType type, bool all, bool ansi, bool quiet, bool silent, CancellationToken cancellationToken)
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

                if (ansi && !console.IsOutputRedirected && !ConsoleHelpers.TryEnableVirtualTerminalProcessing(out var error))
                {
                    console.WriteErrorLine($"Failed to enable virtual terminal processing. [{error}]", suppressOutput: quiet || silent);
                }

                int i = 0;
                foreach (var item in items.Items)
                {
                    var value = await GetClipboardItemValueAsync(item, type, ansi);
                    WriteValueToConsole(console, value, index ? i : null, i < items.Items.Count - 1 && nul, ansi, silent);
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
        private static void WriteValueToConsole(IConsole console, string? value, int? index = null, bool nul = false, bool ansi = false, bool silent = false)
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

            outputValue.Append(value);

            if (ansi)
            {
                outputValue.Append(Ansi.Text.AttributesOff.EscapeSequence);
            }

            if (nul)
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
