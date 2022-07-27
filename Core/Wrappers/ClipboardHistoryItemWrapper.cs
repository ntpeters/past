using Windows.ApplicationModel.DataTransfer;

namespace past.Core.Wrappers
{
    /// <inheritdoc cref="IClipboardHistoryItemWrapper"/>
    public class ClipboardHistoryItemWrapper : IClipboardHistoryItemWrapper
    {
        private readonly ClipboardHistoryItem _clipboardHistoryItem;

        public ClipboardHistoryItem WrappedInstance => _clipboardHistoryItem;

        public DataPackageView Content => _clipboardHistoryItem.Content;

        public string Id => _clipboardHistoryItem.Id;

        public DateTimeOffset Timestamp => _clipboardHistoryItem.Timestamp;
        public ClipboardHistoryItemWrapper(ClipboardHistoryItem clipboardHistoryItem)
        {
            _clipboardHistoryItem = clipboardHistoryItem ?? throw new ArgumentNullException(nameof(clipboardHistoryItem));
        }
    }
}
