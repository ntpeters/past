using past.Core.Extensions;
using past.Core.Wrappers;
using System.Windows;
using Windows.ApplicationModel.DataTransfer;

namespace past.Core
{
    /// <inheritdoc cref="IClipboardManager"/>
    public class ClipboardManager : IClipboardManager
    {
        #region Private Fields
        private readonly IWinRtClipboardWrapper _winRtClipboard;
        private readonly IWin32ClipboardWrapper _win32Clipboard;
        private readonly IPinnedClipboardItemProvider _pinnedClipboardItemProvider;
        #endregion Private Fields

        #region Constructors
        /// <summary>
        /// Creates a new <see cref="ClipboardManager"/> using the system clipboard.
        /// </summary>
        public ClipboardManager()
            : this(new WinRtClipboardWrapper(), new Win32ClipboardWrapper(), new PinnedClipboardItemProvider())
        {
        }

        /// <summary>
        /// Creates a new <see cref="ClipboardManager"/> using the provided clipboards.
        /// </summary>
        /// <remarks>
        /// This constructor is provided to support mocking.
        /// </remarks>
        /// <param name="winRtClipboard">Windows Runtime clipboard APIs.</param>
        /// <param name="win32Clipboard">Win32 clipboard APIs.</param>
        /// <param name="pinnedClipboardItemProvider">Provider for pinned clipboard item metadata.</param>
        /// <exception cref="ArgumentNullException"><paramref name="winRtClipboard"/>, <paramref name="win32Clipboard"/>, or <paramref name="pinnedClipboardItemProvider"/> is null.</exception>
        public ClipboardManager(IWinRtClipboardWrapper winRtClipboard, IWin32ClipboardWrapper win32Clipboard, IPinnedClipboardItemProvider pinnedClipboardItemProvider)
        {
            _winRtClipboard = winRtClipboard ?? throw new ArgumentNullException(nameof(winRtClipboard));
            _win32Clipboard = win32Clipboard ?? throw new ArgumentNullException(nameof(win32Clipboard));
            _pinnedClipboardItemProvider = pinnedClipboardItemProvider ?? throw new ArgumentNullException(nameof(pinnedClipboardItemProvider));
        }
        #endregion Constructors

        #region Public Methods
        public async Task<string?> GetCurrentClipboardValueAsync(ContentType type, CancellationToken? cancellationToken = null)
        {
            // Using the Win32 clipboard API rather than the WinRt clipboard API as that
            // one frequently throws "RPC_E_DISCONNECTED" when trying to access the current
            // clipboard contents.

            // Accessing the current clipboard must be done on an STA thread
            var tsc = new TaskCompletionSource<string?>();
            var staThread = new Thread(() =>
            {
                try
                {
                    string? value = null;
                    if (type.HasFlag(ContentType.Text) && _win32Clipboard.ContainsText(TextDataFormat.UnicodeText))
                    {
                        value = _win32Clipboard.GetText(TextDataFormat.UnicodeText);
                    }
                    else if (type.HasFlag(ContentType.Text) && _win32Clipboard.ContainsText(TextDataFormat.Text))
                    {
                        value = _win32Clipboard.GetText(TextDataFormat.Text);
                    }
                    else if (type.HasFlag(ContentType.Image) && _win32Clipboard.ContainsImage())
                    {
                        value = "[Unsupported Format: Image]";
                    }
                    else if (type == ContentType.All)
                    {
                        var data = _win32Clipboard.GetDataObject();
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
                throw new TimeoutException("Timeout while getting current clipboard contents.");
            }

            return await tsc.Task;
        }

        // TODO: Encapsulate return values, or split out setting as the current item
        public async Task<(IClipboardHistoryItemWrapper Item, SetHistoryItemAsContentStatus? SetCurrentStatus)> GetClipboardHistoryItemAsync(ClipboardItemIdentifier identifier, bool setCurrent, ContentType type, CancellationToken? cancellationToken = null)
        {
            var items = await _winRtClipboard.GetHistoryItemsAsync();
            if (items.Status != ClipboardHistoryItemsResultStatus.Success)
            {
                throw new PastException(items.Status.ToErrorCode(), $"Failed to get clipboard history. Result: {items.Status}");
            }

            if (!items.TryGetItem(identifier, out var item))
            {
                throw new PastException(ErrorCode.NotFound, "Failed to get specified clipboard history item");
            }

            if (!type.Supports(item.Content.Contains))
            {
                throw new PastException(ErrorCode.IncompatibleContentType, "Item does not support the specified content type");
            }

            SetHistoryItemAsContentStatus? setContentStatus = null;
            if (setCurrent)
            {
                setContentStatus = _winRtClipboard.SetHistoryItemAsContent(item);
            }

            return (item, setContentStatus);
        }

        public async Task<IEnumerable<IClipboardHistoryItemWrapper>> ListClipboardHistoryAsync(ContentType type, bool pinned, CancellationToken? cancellationToken = null)
        {
            var items = await _winRtClipboard.GetHistoryItemsAsync();
            if (items.Status != ClipboardHistoryItemsResultStatus.Success)
            {
                throw new PastException(items.Status.ToErrorCode(), $"Failed to get clipboard history. Result: {items.Status}");
            }

            if (items.Items.Count == 0)
            {
                throw new PastException(ErrorCode.NotFound, "Clipboard history is empty");
            }

            IEnumerable<IClipboardHistoryItemWrapper> clipboardItems;
            if (pinned)
            {
                if (!_pinnedClipboardItemProvider.TryGetPinnedClipboardHistoryItemIds(out var pinnedItemIds, out var errorMessage))
                {
                    throw new PastException(ErrorCode.NotFound, errorMessage);
                }

                clipboardItems = items.Items.Where(item => pinnedItemIds.Contains(item.Id));
                if (clipboardItems.Count() == 0)
                {
                    throw new PastException(ErrorCode.NotFound, "No pinned items in clipboard history");
                }
            }
            else
            {
                clipboardItems = items.Items;
            }

            var filteredItems = clipboardItems.Where(item => type.Supports(item.Content.Contains));
            if (filteredItems.Count() == 0)
            {
                throw new PastException(ErrorCode.IncompatibleContentType, "No supported items in clipboard history");
            }

            return filteredItems;
        }

        public bool IsHistoryEnabled() => _winRtClipboard.IsHistoryEnabled();

        public bool IsRoamingEnabled() => _winRtClipboard.IsRoamingEnabled();
        #endregion Public Methods
    }
}
