using past.Core.Extensions;
using past.Core.Models;
using past.Core.Providers;
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
            // one requires access from a UI thread.
            // Trying to access the current clipboard contents from a non-UI thread with
            // the WinRt clipboard API will throw "RPC_E_DISCONNECTED".

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
                        value = "[Unsupported Format: Image support coming soon]";
                    }
                    else if (type == ContentType.All)
                    {
                        var data = _win32Clipboard.GetDataObject();
                        value = $"[Unsupported Format: {string.Join(',', data.GetFormats())}]";
                    }

                    if (value != null)
                    {
                        tsc.SetResult(value);
                    }
                    else
                    {
                        tsc.SetException(new PastException(ErrorCode.IncompatibleContentType, "Item does not support the specified content type"));
                    }

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

        public async Task<IClipboardHistoryItemWrapper> GetClipboardHistoryItemAsync(ClipboardItemIdentifier identifier, ContentType type, CancellationToken? cancellationToken = null)
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

            return item;
        }

        public async Task<IEnumerable<IClipboardHistoryItemWrapper>> GetClipboardHistoryAsync(ContentType type, bool pinned, CancellationToken? cancellationToken = null)
        {
            var items = await _winRtClipboard.GetHistoryItemsAsync();
            if (items.Status != ClipboardHistoryItemsResultStatus.Success)
            {
                throw new PastException(items.Status.ToErrorCode(), $"Failed to get clipboard history. Result: {items.Status}");
            }

            if (items.Items.Count == 0)
            {
                return items.Items;
            }

            IEnumerable<IClipboardHistoryItemWrapper> clipboardItems;
            if (pinned)
            {
                if (!_pinnedClipboardItemProvider.TryGetPinnedClipboardHistoryItemIds(out var pinnedItemIds, out var errorMessage))
                {
                    throw new PastException(ErrorCode.NotFound, errorMessage);
                }

                clipboardItems = items.Items.Where(item => pinnedItemIds.Contains(item.Id));
                if (clipboardItems.Any())
                {
                    return clipboardItems;
                }
            }
            else
            {
                clipboardItems = items.Items;
            }

            var filteredItems = clipboardItems.Where(item => type.Supports(item.Content.Contains));
            return filteredItems;
        }

        public async Task PinClipboardItemAsync(ClipboardItemIdentifier identifier, CancellationToken? cancellationToken = null)
        {
            _ = identifier ?? throw new ArgumentNullException(nameof(identifier));

            var item = await GetClipboardHistoryItemAsync(identifier, ContentType.All, cancellationToken);
            if (!_pinnedClipboardItemProvider.TryAddPinnedClipboardHistoryItem(item, out var errorMessage))
            {
                throw new PastException(ErrorCode.NotFound, errorMessage);
            }
        }

        public async Task UnpinClipboardItemAsync(ClipboardItemIdentifier identifier, CancellationToken? cancellationToken = null)
        {
            _ = identifier ?? throw new ArgumentNullException(nameof(identifier));

            var item = await GetClipboardHistoryItemAsync(identifier, ContentType.All, cancellationToken);
            if (!_pinnedClipboardItemProvider.TryRemovePinnedClipboardHistoryItem(item, out var errorMessage))
            {
                throw new PastException(ErrorCode.NotFound, errorMessage);
            }
        }

        public SetHistoryItemAsContentStatus SetHistoryItemAsContent(IClipboardHistoryItemWrapper item) => _winRtClipboard.SetHistoryItemAsContent(item);

        public bool IsHistoryEnabled() => _winRtClipboard.IsHistoryEnabled();

        public bool IsRoamingEnabled() => _winRtClipboard.IsRoamingEnabled();
        #endregion Public Methods
    }
}
