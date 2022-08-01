using past.Core;
using System;
using System.CommandLine.Invocation;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;

namespace past.ConsoleApp
{
    /// <summary>
    /// Writes values from the system clipboard and clipboard history to a console.
    /// </summary>
    public class ConsoleClipboard
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
        /// <summary>
        /// Writes the current status of the system clipboard history and clipboard roaming settings.
        /// </summary>
        /// <param name="consoleWriter">Writer to write to the console with.</param>
        /// <param name="context">Invocation context to set the exit code.</param>
        /// <exception cref="ArgumentNullException"><paramref name="consoleWriter"/> or <paramref name="context"/> is null.</exception>
        public void GetClipboardHistoryStatus(IConsoleWriter consoleWriter, InvocationContext context)
        {
            _ = consoleWriter ?? throw new ArgumentNullException(nameof(consoleWriter));
            _ = context ?? throw new ArgumentNullException(nameof(context));

            try
            {
                consoleWriter.WriteLine($"Clipboard History Enabled: {_clipboard.IsHistoryEnabled()}");
                consoleWriter.WriteLine($"Clipboard Roaming Enabled: {_clipboard.IsRoamingEnabled()}");

               context.ExitCode = 0;
            }
            catch (Exception e)
            {
                consoleWriter.WriteErrorLine($"Failed to get current clipboard history status. Error: {e.Message}");
                context.ExitCode = -1;
            }
        }

        /// <summary>
        /// Writes the value currently stored on the clipboard to the provided console writer, if compatible with the specified type.
        /// </summary>
        /// <param name="consoleWriter">Writer to write to the console with.</param>
        /// <param name="formatter">Formatter to use for formatting the value.</param>
        /// <param name="type">Content type to filter on.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> representing request cancellation.</param>
        /// <returns>Zero on success, a negative value otherwise.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="consoleWriter"/> or <paramref name="formatter"/> is null.</exception>
        public async Task<int> GetCurrentClipboardValueAsync(IConsoleWriter consoleWriter, IValueFormatter formatter, ContentType type, CancellationToken cancellationToken)
        {
            _ = consoleWriter ?? throw new ArgumentNullException(nameof(consoleWriter));
            _ = formatter ?? throw new ArgumentNullException(nameof(formatter));

            try
            {
                string? value = await _clipboard.GetCurrentClipboardValueAsync(type, cancellationToken);
                consoleWriter.WriteValue(value, formatter);
            }
            catch (Exception e)
            {
                consoleWriter.WriteErrorLine($"Failed to get current clipboard contents. Error: {e.Message}");
                return -1;
            }

            return 0;
        }

        /// <summary>
        /// Writes the specified item from the clipboard history to the provided console writer, if compatible with the specified type,
        /// and optionally sets it as the current clipboard value.
        /// </summary>
        /// <param name="consoleWriter">Writer to write to the console with.</param>
        /// <param name="formatter">Formatter to use for formatting the value.</param>
        /// <param name="identifier">Identifier describing an item in the clipboard history.</param>
        /// <param name="type">Content type to filter on.</param>
        /// <param name="setCurrent">Whether to set the specified item as the current clipboard value.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> representing request cancellation.</param>
        /// <returns>Zero on success, a negative value otherwise.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="consoleWriter"/>, <paramref name="formatter"/>, or <paramref name="identifier"/> is null.
        /// </exception>
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
            catch (Exception e)
            {
                consoleWriter.WriteErrorLine($"Failed to get clipboard history. Error: {e.Message}");
                return -1;
            }

            return 0;
        }

        /// <summary>
        /// Writes all items in the clipboard history to the provided console writer,
        /// filtered on <paramref name="type"/> and <paramref name="pinned"/>.
        /// </summary>
        /// <param name="consoleWriter">Writer to write to the console with.</param>
        /// <param name="formatter">Formatter to use for formatting the value.</param>
        /// <param name="type">Content type to filter on.</param>
        /// <param name="pinned">Whether to only return pinned clipboard items.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> representing request cancellation.</param>
        /// <returns>Zero on success, a negative value otherwise.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="consoleWriter"/> or <paramref name="formatter"/> is null.</exception>
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
                    await consoleWriter.WriteItemAsync(item, type, index, formatter, emitLineEnding: index < clipboardItems.Count() - 1);
                    index++;
                }
            }
            catch (Exception e)
            {
                consoleWriter.WriteErrorLine($"Failed to get clipboard history. Error: {e.Message}");
                return -1;
            }

            return 0;
        }
        #endregion Public Methods
    }
}
