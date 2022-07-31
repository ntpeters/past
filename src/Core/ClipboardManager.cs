using past.Core.Extensions;
using past.Core.Wrappers;
using System.Windows;
using Windows.ApplicationModel.DataTransfer;

namespace past.Core
{
    public class ClipboardManager : IClipboardManager
    {
        #region Private Fields
        private readonly IWinRtClipboardWrapper _winRtClipboard;
        private readonly IWin32ClipboardWrapper _win32Clipboard;
        private readonly IPinnedClipboardItemProvider _pinnedClipboardItemProvider;

        public ClipboardManager()
            : this(new WinRtClipboardWrapper(), new Win32ClipboardWrapper(), new PinnedClipboardItemProvider())
        {
        }

        public ClipboardManager(IWinRtClipboardWrapper winRtClipboard, IWin32ClipboardWrapper win32Clipboard, IPinnedClipboardItemProvider pinnedClipboardItemProvider)
        {
            _winRtClipboard = winRtClipboard ?? throw new ArgumentNullException(nameof(winRtClipboard));
            _win32Clipboard = win32Clipboard ?? throw new ArgumentNullException(nameof(win32Clipboard));
            _pinnedClipboardItemProvider = pinnedClipboardItemProvider ?? throw new ArgumentNullException(nameof(pinnedClipboardItemProvider));
        }
        #endregion Private Fields

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
                throw new Exception($"Failed to get clipboard history. Result: {items.Status}");
            }

            if (!items.TryGetItem(identifier, out var item))
            {
                throw new Exception("Failed to get specified clipboard history item");
            }

            if (!type.Supports(item.Content.Contains))
            {
                throw new Exception("Item does not support the specified content type");
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
                throw new Exception($"Failed to get clipboard history. Result: {items.Status}");
            }

            if (items.Items.Count == 0)
            {
                throw new Exception("Clipboard history is empty");
            }

            IEnumerable<IClipboardHistoryItemWrapper> clipboardItems;
            if (pinned)
            {
                if (!_pinnedClipboardItemProvider.TryGetPinnedClipboardHistoryItemIds(out var pinnedItemIds, out var errorMessage))
                {
                    throw new Exception(errorMessage);
                }

                clipboardItems = items.Items.Where(item => pinnedItemIds.Contains(item.Id));
                if (clipboardItems.Count() == 0)
                {
                    throw new Exception("No pinned items in clipboard history");
                }
            }
            else
            {
                clipboardItems = items.Items;
            }

            var filteredItems = clipboardItems.Where(item => type.Supports(item.Content.Contains));
            if (filteredItems.Count() == 0)
            {
                throw new Exception("No supported items in clipboard history");
            }

            return filteredItems;
        }

        public bool IsHistoryEnabled() => _winRtClipboard.IsHistoryEnabled();

        public bool IsRoamingEnabled() => _winRtClipboard.IsRoamingEnabled();
        #endregion Public Methods
    }
}
