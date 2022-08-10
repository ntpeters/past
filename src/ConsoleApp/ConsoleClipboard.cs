using past.Core;
using System;
using System.CommandLine.Invocation;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;

namespace past.ConsoleApp
{
    /// <inheritdoc cref="IConsoleClipboard"/>
    public class ConsoleClipboard : IConsoleClipboard
    {
        #region Private Fields
        private readonly IClipboardManager _clipboard;
        #endregion Private Fields

        #region Constructors
        /// <summary>
        /// Creates a new <see cref="ConsoleClipboard"/> using the system clipboard.
        /// </summary>
        public ConsoleClipboard()
            : this(new ClipboardManager())
        {
        }

        /// <summary>
        /// Creates a new <see cref="ConsoleClipboard"/> using the provided clipboard manager.
        /// </summary>
        /// <remarks>This constructor is provided to support mocking.</remarks>
        /// <param name="clipboardManager">Clipboard manager to use for interacting with the clipboard.</param>
        /// <exception cref="ArgumentNullException"><paramref name="clipboardManager"/> is null.</exception>
        public ConsoleClipboard(IClipboardManager clipboardManager)
        {
            _clipboard = clipboardManager ?? throw new ArgumentNullException(nameof(clipboardManager));
        }
        #endregion Constructors

        #region Public Methods
        public void GetClipboardHistoryStatus(IConsoleWriter consoleWriter, InvocationContext context)
        {
            _ = consoleWriter ?? throw new ArgumentNullException(nameof(consoleWriter));
            _ = context ?? throw new ArgumentNullException(nameof(context));

            try
            {
                consoleWriter.WriteLine($"Clipboard History Enabled: {_clipboard.IsHistoryEnabled()}");
                consoleWriter.WriteLine($"Clipboard Roaming Enabled: {_clipboard.IsRoamingEnabled()}");

               context.ExitCode = (int)ErrorCode.Success;
            }
            catch (PastException e)
            {
                consoleWriter.WriteErrorLine($"Failed to get current clipboard history status. Error: {e.Message}");
                context.ExitCode = (int)e.ErrorCode;
            }
        }

        public async Task<int> GetCurrentClipboardValueAsync(IConsoleWriter consoleWriter, IValueFormatter formatter, ContentType type, CancellationToken cancellationToken)
        {
            _ = consoleWriter ?? throw new ArgumentNullException(nameof(consoleWriter));
            _ = formatter ?? throw new ArgumentNullException(nameof(formatter));

            try
            {
                string? value = await _clipboard.GetCurrentClipboardValueAsync(type, cancellationToken);
                consoleWriter.WriteValue(value, formatter);
            }
            catch (PastException e)
            {
                consoleWriter.WriteErrorLine($"Failed to get current clipboard contents. Error: {e.Message}");
                return (int)e.ErrorCode;
            }

            return 0;
        }

        public async Task<int> GetClipboardHistoryItemAsync(IConsoleWriter consoleWriter, IValueFormatter formatter, ClipboardItemIdentifier identifier, ContentType type, bool setCurrent, CancellationToken cancellationToken)
        {
            _ = consoleWriter ?? throw new ArgumentNullException(nameof(consoleWriter));
            _ = formatter ?? throw new ArgumentNullException(nameof(formatter));
            _ = identifier ?? throw new ArgumentNullException(nameof(identifier));

            try
            {
                var (item, setContentStatus) = await _clipboard.GetClipboardHistoryItemAsync(identifier, setCurrent, type, cancellationToken);
                await consoleWriter.WriteItemAsync(item, type, formatter: formatter);

                if (setCurrent && setContentStatus != SetHistoryItemAsContentStatus.Success)
                {
                    consoleWriter.WriteErrorLine($"Failed updating the current clipboard content with the selected history item. Error: {setContentStatus}");
                }
            }
            catch (PastException e)
            {
                consoleWriter.WriteErrorLine($"Failed to get clipboard history. Error: {e.Message}");
                return (int)e.ErrorCode;
            }

            return 0;
        }

        public async Task<int> ListClipboardHistoryAsync(IConsoleWriter consoleWriter, IValueFormatter formatter, ContentType type, bool pinned, CancellationToken cancellationToken)
        {
            _ = consoleWriter ?? throw new ArgumentNullException(nameof(consoleWriter));
            _ = formatter ?? throw new ArgumentNullException(nameof(formatter));

            try
            {
                var clipboardItems = await _clipboard.ListClipboardHistoryAsync(type, pinned, cancellationToken);

                int index = 0;
                foreach (var item in clipboardItems)
                {
                    _ = consoleWriter.WriteItemAsync(item, type, index, formatter, emitLineEnding: index < clipboardItems.Count() - 1);
                    index++;
                }
            }
            catch (PastException e)
            {
                consoleWriter.WriteErrorLine($"Failed to get clipboard history. Error: {e.Message}");
                return (int)e.ErrorCode;
            }

            return 0;
        }
        #endregion Public Methods
    }
}
