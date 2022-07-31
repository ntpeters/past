using past.Core.Wrappers;
using Windows.ApplicationModel.DataTransfer;

namespace past.Core
{
    public interface IClipboardManager
    {
        Task<string?> GetCurrentClipboardValueAsync(ContentType type, CancellationToken? cancellationToken = null);

        Task<(IClipboardHistoryItemWrapper Item, SetHistoryItemAsContentStatus? SetCurrentStatus)> GetClipboardHistoryItemAsync(ClipboardItemIdentifier identifier, bool setCurrent, ContentType type, CancellationToken? cancellationToken = null);

        Task<IEnumerable<IClipboardHistoryItemWrapper>> ListClipboardHistoryAsync(ContentType type, bool pinned, CancellationToken? cancellationToken = null);

        bool IsHistoryEnabled();

        bool IsRoamingEnabled();
    }
}
