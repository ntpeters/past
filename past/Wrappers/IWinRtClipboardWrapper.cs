using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;

namespace past.Wrappers
{
    /// <inheritdoc cref="Clipboard"/>
    /// <remarks>
    /// This is a thin wrapper around <see cref="Clipboard"/> to support mocking.
    /// Only the methods used by this project are exposed through this wrapper.
    /// </remarks>
    public interface IWinRtClipboardWrapper
    {
        /// <inheritdoc cref="Clipboard.GetHistoryItemsAsync"/>
        /// <remarks>
        /// Return value is wrapped as an <see cref="IClipboardHistoryItemsResultWrapper"/>
        /// </remarks>
        public Task<IClipboardHistoryItemsResultWrapper> GetHistoryItemsAsync();

        /// <inheritdoc cref="Clipboard.SetHistoryItemAsContent(ClipboardHistoryItem)"/>
        public SetHistoryItemAsContentStatus SetHistoryItemAsContent(IClipboardHistoryItemWrapper item);

        /// <inheritdoc cref="Clipboard.IsHistoryEnabled"/>
        public bool IsHistoryEnabled();

        /// <inheritdoc cref="Clipboard.IsRoamingEnabled"/>
        public bool IsRoamingEnabled();

    }
}
