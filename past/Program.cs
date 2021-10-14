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

namespace past
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            var listCommand = new Command("list", "Lists the clipboard history");
            var nullOption = new Option<bool>("--nul", "Use the null byte to separate entries");
            listCommand.AddOption(nullOption);
            var indexOption = new Option<bool>("--index", "Print indices with each item");
            listCommand.AddOption(indexOption);
            var allOption = new Option<bool>("--all", "Print all entries, including unsupported types");
            listCommand.AddOption(allOption);
            var ansiOption = new Option<bool>("--ansi", "Enable processing of ANSI control sequences");
            listCommand.AddOption(ansiOption);
            listCommand.Handler = CommandHandler.Create<IConsole, bool, bool, bool, bool, CancellationToken>(ListClipboardHistoryAsync);

            var getCommand = new Command("get", "Gets the item at the specified index from clipboard history");
            var indexArgument = new Argument<int>("index", "The index of the item to get from clipboard history");
            getCommand.AddArgument(indexArgument);
            getCommand.AddOption(ansiOption);
            getCommand.Handler = CommandHandler.Create<IConsole, int, bool, CancellationToken>(GetClipboardHistoryItemAsync);

            var rootCommand = new RootCommand();
            rootCommand.AddCommand(listCommand);
            rootCommand.AddCommand(getCommand);

            rootCommand.AddOption(allOption);
            rootCommand.Handler = CommandHandler.Create<IConsole, bool, CancellationToken>(GetCurrentClipboardValueAsync);

            return await rootCommand.InvokeAsync(args);
        }

        #region Commands
        private static async Task<int> GetCurrentClipboardValueAsync(IConsole console, bool all, CancellationToken cancellationToken)
        {
            // Using the Win32 clipboard API rather than the WinRt clipboard API as that
            // one frequently throws "RPC_E_DISCONNECTED" when trying to access the current
            // clipboard contents.
            try
            {
                // Accessing the current clipboard must be done on an STA thread
                var tsc = new TaskCompletionSource<string?>();
                var staThread = new Thread(() =>
                {
                    try
                    {
                        string? value = null;
                        if (Win32Clipboard.ContainsText(System.Windows.TextDataFormat.UnicodeText))
                        {
                            value = Win32Clipboard.GetText(System.Windows.TextDataFormat.UnicodeText);
                        }
                        else if (Win32Clipboard.ContainsText(System.Windows.TextDataFormat.Text))
                        {
                            value = Win32Clipboard.GetText(System.Windows.TextDataFormat.Text);
                        }
                        else if (all)
                        {
                            if (Win32Clipboard.ContainsImage())
                            {
                                value = "[Unsupported Format: Image]";
                            }
                            else if (Win32Clipboard.ContainsAudio())
                            {
                                value = "[Unsupported Format: Audio]";
                            }
                            else if (Win32Clipboard.ContainsFileDropList())
                            {
                                value = "[Unsupported Format: File Drop List]";
                            }
                            else
                            {
                                var data = Win32Clipboard.GetDataObject();
                                value = $"[Unsupported Format: {string.Join(',', data.GetFormats())}]";
                            }
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
                    console.Error.WriteLine("Timeout while getting current clipboard contents.");
                    return -1;
                }

                var value = await tsc.Task;
                WriteValueToConsole(console, value);
            }
            catch (Exception e)
            {
                console.Error.WriteLine($"Failed to get current clipboard contents. Error: {e}");
                return -1;
            }

            return 0;
        }

        private static async Task<int> GetClipboardHistoryItemAsync(IConsole console, int index, bool ansi, CancellationToken cancellationToken)
        {
            try
            {
                var items = await WinRtClipboard.GetHistoryItemsAsync();
                if (items.Status != ClipboardHistoryItemsResultStatus.Success)
                {
                    console.Error.WriteLine($"Failed to get clipboard history. Result: {items.Status}");
                    return -1;
                }

                if (ansi && !console.IsOutputRedirected && !ConsoleHelpers.TryEnableVirtualTerminalProcessing(out var error))
                {
                    console.Error.WriteLine($"Failed to enable virtual terminal processing. [{error}]");
                }

                var item = items.Items.ElementAt(index);
                var value = await GetClipboardItemValueAsync(item, ansi: ansi);
                WriteValueToConsole(console, value, ansi: ansi);
            }
            catch (Exception e)
            {
                console.Error.WriteLine($"Failed to get clipboard history. Error: {e}");
                return -1;
            }

            return 0;
        }

        private static async Task<int> ListClipboardHistoryAsync(IConsole console, bool nul, bool index, bool all, bool ansi, CancellationToken cancellationToken)
        {
            try
            {
                var items = await WinRtClipboard.GetHistoryItemsAsync();
                if (items.Status != ClipboardHistoryItemsResultStatus.Success)
                {
                    console.Error.WriteLine($"Failed to get clipboard history. Result: {items.Status}");
                    return -1;
                }

                if (items.Items.Count == 0)
                {
                    console.Error.WriteLine("Clipboard history is empty");
                    return 1;
                }

                if (ansi && !console.IsOutputRedirected && !ConsoleHelpers.TryEnableVirtualTerminalProcessing(out var error))
                {
                    console.Error.WriteLine($"Failed to enable virtual terminal processing. [{error}]");
                }

                int i = 0;
                foreach (var item in items.Items)
                {
                    var value = await GetClipboardItemValueAsync(item, all, ansi);
                    WriteValueToConsole(console, value, index ? i : null, i < items.Items.Count - 1 && nul, ansi);
                    i++;
                }
            }
            catch (Exception e)
            {
                console.Error.WriteLine($"Failed to get clipboard history. Error: {e}");
                return -1;
            }

            return 0;
        }
        #endregion Commands

        #region Helpers
        private static void WriteValueToConsole(IConsole console, string? value, int? index = null, bool nul = false, bool ansi = false)
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

            console.Out.Write(outputValue.ToString());
        }

        private static Task<string?> GetClipboardItemValueAsync(ClipboardHistoryItem item, bool all = false, bool ansi = false)
        {
            return GetClipboardItemValueAsync(item.Content, all, ansi);
        }

        private static async Task<string?> GetClipboardItemValueAsync(DataPackageView content, bool all = false, bool ansi = false)
        {
            if (content.Contains(StandardDataFormats.Text))
            {
                return await content.GetTextAsync();
            }
            else if (all)
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
        #endregion Helpers
    }
}
