using Windows.ApplicationModel.DataTransfer;

namespace past.Core.Wrappers
{
    /// <inheritdoc cref="ClipboardHistoryItem"/>
    /// <remarks>
    /// This is a thin wrapper around <see cref="ClipboardHistoryItem"/> to support mocking.
    /// </remarks>
    public interface IClipboardHistoryItemWrapper : IInstanceWrapper<ClipboardHistoryItem>
    {
        /// <inheritdoc cref="ClipboardHistoryItem.Content"/>
        DataPackageView Content { get; }

        /// <inheritdoc cref="ClipboardHistoryItem.Id"/>
        string Id { get; }

        /// <inheritdoc cref="ClipboardHistoryItem.Timestamp"/>
        DateTimeOffset Timestamp { get; }

        /// <summary>
        /// The original index of the item in the clipboard history.
        /// </summary>
        int Index { get; }
    }
}
