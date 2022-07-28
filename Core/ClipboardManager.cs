using past.Core.Extensions;
using past.Core.Wrappers;
using System.IO;
using System.Text.Json;
using System.Windows;
using Windows.ApplicationModel.DataTransfer;

namespace past.Core
{
    public class ClipboardManager
    {
        private readonly IWinRtClipboardWrapper _winRtClipboard;
        private readonly IWin32ClipboardWrapper _win32Clipboard;

        public ClipboardManager()
            : this(new WinRtClipboardWrapper(), new Win32ClipboardWrapper())
        {
        }

        public ClipboardManager(IWinRtClipboardWrapper winRtClipboard, IWin32ClipboardWrapper win32Clipboard)
        {
            _winRtClipboard = winRtClipboard ?? throw new ArgumentNullException(nameof(winRtClipboard));
            _win32Clipboard = win32Clipboard ?? throw new ArgumentNullException(nameof(win32Clipboard));
        }

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
                    else if (type.HasFlag(ContentType.File) && _win32Clipboard.ContainsFileDropList())
                    {
                        value = "[Unsupported Format: File Drop List]";
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

            // TODO: Filter on content type
            if (!items.TryGetItem(identifier, out var item))
            {
                throw new Exception("Failed to get specified clipboard history item");
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

            // TODO: Refactor out into separate method or provider
            if (pinned)
            {
                // TODO: Get pinned clipboard history items
                // Pinned item IDs can be read from: %LOCALAPPDATA%/Microsoft/Windows/Clipboard/Pinned/{GUID}/metadata.json
                var localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                var pinnedClipboardPath = Path.Combine(localAppDataPath, "Microsoft/Windows/Clipboard/Pinned");
                var pinnedClipboardItemMetadataDirectory = Directory.EnumerateDirectories(pinnedClipboardPath).First(directoryPath => File.Exists(Path.Combine(directoryPath, "metadata.json")));
                string? pinnedClipboardItemMetadataPath = null;
                foreach (var directoryPath in Directory.EnumerateDirectories(pinnedClipboardPath))
                {
                    var metadataPath = Path.Combine(directoryPath, "metadata.json");
                    if (File.Exists(metadataPath))
                    {
                        pinnedClipboardItemMetadataPath = metadataPath;
                        break;
                    }
                }
                if (string.IsNullOrWhiteSpace(pinnedClipboardItemMetadataPath))
                {
                    throw new Exception("Failed to retrieve pinned clipboard history items");
                }

                var pinnedClipboardItemMetadataJson = File.ReadAllText(pinnedClipboardItemMetadataPath);
                var pinnedClipboardItemMetadata = JsonDocument.Parse(pinnedClipboardItemMetadataJson);
                var pinnedClipboardItemIds = pinnedClipboardItemMetadata.RootElement.GetProperty("items").EnumerateObject().Select(property => property.Name);
                clipboardItems = items.Items.Where(item => pinnedClipboardItemIds.Contains(item.Id));
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
    }
}
