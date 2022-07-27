using Windows.ApplicationModel.DataTransfer;
using WinRtClipboard = Windows.ApplicationModel.DataTransfer.Clipboard;

namespace past.Core.Wrappers
{
    /// <inheritdoc cref="IWinRtClipboardWrapper"/>
    public class WinRtClipboardWrapper : IWinRtClipboardWrapper
    {
        public async Task<IClipboardHistoryItemsResultWrapper> GetHistoryItemsAsync()
        {
            var items = await WinRtClipboard.GetHistoryItemsAsync();
            return new ClipboardHistoryItemsResultWrapper(items);
        }

        public SetHistoryItemAsContentStatus SetHistoryItemAsContent(IClipboardHistoryItemWrapper item) => WinRtClipboard.SetHistoryItemAsContent(item.WrappedInstance);

        public bool IsHistoryEnabled() => WinRtClipboard.IsHistoryEnabled();

        public bool IsRoamingEnabled() => WinRtClipboard.IsRoamingEnabled();

    }
}
