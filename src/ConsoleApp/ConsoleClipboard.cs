using past.ConsoleApp.Extensions;
using past.Core;
using past.Core.Wrappers;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Rendering;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;

namespace past.ConsoleApp
{
    public class ConsoleClipboard
    {
        private readonly ClipboardManager _clipboard;

        public ConsoleClipboard()
            : this(new ClipboardManager())
        {
        }

        public ConsoleClipboard(ClipboardManager clipboardManager)
        {
            _clipboard = clipboardManager ?? throw new ArgumentNullException(nameof(clipboardManager));
        }

        public void GetClipboardHistoryStatus(InvocationContext context, IConsole console, bool quiet, CancellationToken cancellationToken)
        {
            try
            {
                console.WriteLine($"Clipboard History Enabled: {_clipboard.IsHistoryEnabled()}");
                console.WriteLine($"Clipboard Roaming Enabled: {_clipboard.IsRoamingEnabled()}");
            }
            catch (Exception e)
            {
                console.WriteErrorLine($"Failed to get current clipboard history status. Error: {e}", suppressOutput: quiet);
                context.ExitCode = -1;
            }

            context.ExitCode = 0;
        }

        public async Task<int> GetCurrentClipboardValueAsync(IConsole console, ContentType type, bool ansi, AnsiResetType ansiResetType, bool quiet, CancellationToken cancellationToken)
        {
            try
            {
                string? value = await _clipboard.GetCurrentClipboardValueAsync(type, cancellationToken);
                WriteValueToConsole(console, value, ansi: ansi, ansiResetType: ansiResetType);
            }
            catch (Exception e)
            {
                console.WriteErrorLine($"Failed to get current clipboard contents. Error: {e}", suppressOutput: quiet);
                return -1;
            }

            return 0;
        }

        public async Task<int> GetClipboardHistoryItemAsync(IConsole console, ClipboardItemIdentifier identifier, bool ansi, AnsiResetType ansiResetType, bool setCurrent, ContentType type, bool quiet, CancellationToken cancellationToken)
        {
            try
            {
                if (ansi && !console.IsOutputRedirected && !ConsoleHelpers.TryEnableVirtualTerminalProcessing(out var error))
                {
                    console.WriteErrorLine($"Failed to enable virtual terminal processing. [{error}]", suppressOutput: quiet);
                }

                var (item, setContentStatus) = await _clipboard.GetClipboardHistoryItemAsync(identifier, setCurrent, type, cancellationToken);
                var value = await GetClipboardItemValueAsync(item, ansi: ansi);
                WriteValueToConsole(console, value, ansi: ansi, ansiResetType: ansiResetType);

                if (setCurrent && setContentStatus != SetHistoryItemAsContentStatus.Success)
                {
                    console.WriteErrorLine($"Failed updating the current clipboard content with the selected history item. Error: {setContentStatus}", suppressOutput: quiet);
                }
            }
            catch (Exception e)
            {
                console.WriteErrorLine($"Failed to get clipboard history. Error: {e}", suppressOutput: quiet);
                return -1;
            }

            return 0;
        }

        public async Task<int> ListClipboardHistoryAsync(IConsole console, bool @null, bool index, ContentType type, bool ansi, AnsiResetType ansiResetType, bool quiet, bool id, bool pinned, bool time, CancellationToken cancellationToken)
        {
            try
            {
                var clipboardItems = await _clipboard.ListClipboardHistoryAsync(type, pinned, cancellationToken);

                if (ansi && !console.IsOutputRedirected && !ConsoleHelpers.TryEnableVirtualTerminalProcessing(out var error))
                {
                    console.WriteErrorLine($"Failed to enable virtual terminal processing. [{error}]", suppressOutput: quiet);
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
                    if (WriteValueToConsole(console, value, printIndex, printNull, ansi, ansiResetType, printId, printTimestamp))
                    {
                        outputItemCount++;
                    }
                    i++;
                }
            }
            catch (Exception e)
            {
                console.WriteErrorLine($"Failed to get clipboard history. Error: {e}", suppressOutput: quiet);
                return -1;
            }

            return 0;
        }

        #region Helpers
        private static bool WriteValueToConsole(IConsole console, string? value, int? index = null, bool @null = false, bool ansi = false, AnsiResetType ansiResetType = AnsiResetType.Auto, string? id = null, string? timestamp = null)
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

            console.Write(outputValue.ToString());
            return true;
        }

        private static Task<string?> GetClipboardItemValueAsync(IClipboardHistoryItemWrapper item, ContentType type = ContentType.Text, bool ansi = false)
        {
            return GetClipboardItemValueAsync(item.Content, type, ansi);
        }

        private static async Task<string?> GetClipboardItemValueAsync(DataPackageView content, ContentType type = ContentType.Text, bool ansi = false)
        {
            if (type.HasFlag(ContentType.Text) && content.Contains(StandardDataFormats.Text))
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
        #endregion Helpers
    }
}
