using past.ConsoleApp.Output;
using past.Core;
using past.Core.Models;
using past.Core.Wrappers;
using System;
using System.Collections.Generic;
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
                bool historyEnabled = _clipboard.IsHistoryEnabled();
                bool roamingEnabled = _clipboard.IsRoamingEnabled();

                consoleWriter.WriteLine($"Clipboard History Enabled: {historyEnabled}");
                consoleWriter.WriteLine($"Clipboard Roaming Enabled: {roamingEnabled}");

                context.ExitCode = (int)ErrorCode.Success;
                context.ExitCode |= historyEnabled ? 1 : 0;
                context.ExitCode |= roamingEnabled ? 2 : 0;
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

            return (int)ErrorCode.Success;
        }

        public async Task<int> GetClipboardHistoryItemAsync(IConsoleWriter consoleWriter, IValueFormatter formatter, ClipboardItemIdentifier identifier, ContentType type, bool setCurrent, CancellationToken cancellationToken)
        {
            _ = consoleWriter ?? throw new ArgumentNullException(nameof(consoleWriter));
            _ = formatter ?? throw new ArgumentNullException(nameof(formatter));
            _ = identifier ?? throw new ArgumentNullException(nameof(identifier));

            try
            {
                var item = await _clipboard.GetClipboardHistoryItemAsync(identifier, type, cancellationToken);
                await consoleWriter.WriteItemAsync(item, type, formatter: formatter);

                if (setCurrent)
                {
                    var setContentStatus = _clipboard.SetHistoryItemAsContent(item);
                    if (setContentStatus != SetHistoryItemAsContentStatus.Success)
                    {
                        consoleWriter.WriteErrorLine($"Failed updating the current clipboard content with the selected history item. Error: {setContentStatus}");
                    }
                }
            }
            catch (PastException e)
            {
                consoleWriter.WriteErrorLine($"Failed to get clipboard history. Error: {e.Message}");
                return (int)e.ErrorCode;
            }

            return (int)ErrorCode.Success;
        }

        public async Task<int> ListClipboardHistoryAsync(IConsoleWriter consoleWriter, IValueFormatter formatter, ContentType type, bool pinned, CancellationToken cancellationToken)
        {
            _ = consoleWriter ?? throw new ArgumentNullException(nameof(consoleWriter));
            _ = formatter ?? throw new ArgumentNullException(nameof(formatter));

            IEnumerable<IClipboardHistoryItemWrapper> clipboardItems;
            try
            {
                clipboardItems = await _clipboard.GetClipboardHistoryAsync(type, pinned, cancellationToken);

                int index = 0;
                foreach (var item in clipboardItems)
                {
                    await consoleWriter.WriteItemAsync(item, type, formatter, emitLineEnding: index < clipboardItems.Count() - 1);
                    index++;
                }
            }
            catch (PastException e)
            {
                consoleWriter.WriteErrorLine($"Failed to get clipboard history. Error: {e.Message}");
                return (int)e.ErrorCode;
            }

            return clipboardItems.Count();
        }

        public async Task<int> PinClipboardItemAsync(IConsoleWriter consoleWriter, ClipboardItemIdentifier identifier, CancellationToken cancellationToken)
        {
            _ = consoleWriter ?? throw new ArgumentNullException(nameof(consoleWriter));
            _ = identifier ?? throw new ArgumentNullException(nameof(identifier));

            try
            {
                await _clipboard.PinClipboardItemAsync(identifier, cancellationToken);
            }
            catch (PastException e)
            {
                consoleWriter.WriteErrorLine($"Failed to pin clipboard history item. Error: {e.Message}");
                return (int)e.ErrorCode;
            }

            return (int)ErrorCode.Success;
        }

        public async Task<int> UnpinClipboardItemAsync(IConsoleWriter consoleWriter, ClipboardItemIdentifier identifier, CancellationToken cancellationToken)
        {
            _ = consoleWriter ?? throw new ArgumentNullException(nameof(consoleWriter));
            _ = identifier ?? throw new ArgumentNullException(nameof(identifier));

            try
            {
                await _clipboard.UnpinClipboardItemAsync(identifier, cancellationToken);
            }
            catch (PastException e)
            {
                consoleWriter.WriteErrorLine($"Failed to unpin clipboard history item. Error: {e.Message}");
                return (int)e.ErrorCode;
            }

            return (int)ErrorCode.Success;
        }
        #endregion Public Methods
    }
}
