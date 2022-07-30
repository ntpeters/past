using past.ConsoleApp.Extensions;
using past.Core;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
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

        #region Public Methods
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

        public async Task<int> GetCurrentClipboardValueAsync(IConsoleWriter consoleWriter, ContentType type, CancellationToken cancellationToken)
        {
            try
            {
                string? value = await _clipboard.GetCurrentClipboardValueAsync(type, cancellationToken);
                consoleWriter.WriteValue(value);
            }
            catch (Exception e)
            {
                consoleWriter.WriteErrorLine($"Failed to get current clipboard contents. Error: {e}");
                return -1;
            }

            return 0;
        }

        public async Task<int> GetClipboardHistoryItemAsync(IConsoleWriter consoleWriter, ClipboardItemIdentifier identifier, ContentType type, bool setCurrent, CancellationToken cancellationToken)
        {
            try
            {
                var (item, setContentStatus) = await _clipboard.GetClipboardHistoryItemAsync(identifier, setCurrent, type, cancellationToken);
                await consoleWriter.WriteItemAsync(item, type);

                if (setCurrent && setContentStatus != SetHistoryItemAsContentStatus.Success)
                {
                    consoleWriter.WriteErrorLine($"Failed updating the current clipboard content with the selected history item. Error: {setContentStatus}");
                }
            }
            catch (Exception e)
            {
                consoleWriter.WriteErrorLine($"Failed to get clipboard history. Error: {e}");
                return -1;
            }

            return 0;
        }

        public async Task<int> ListClipboardHistoryAsync(IConsoleWriter consoleWriter, IValueFormatter formatter, ContentType type, bool pinned, CancellationToken cancellationToken)
        {
            try
            {
                var clipboardItems = await _clipboard.ListClipboardHistoryAsync(type, pinned, cancellationToken);

                int i = 0;
                foreach (var item in clipboardItems)
                {
                    await consoleWriter.WriteItemAsync(item, type, i, formatter);
                    i++;
                }
            }
            catch (Exception e)
            {
                consoleWriter.WriteErrorLine($"Failed to get clipboard history. Error: {e}");
                return -1;
            }

            return 0;
        }
        #endregion Public Methods
    }
}
