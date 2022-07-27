using Windows.ApplicationModel.DataTransfer;

namespace past.Core.Wrappers
{
    /// <inheritdoc cref="ClipboardHistoryItemsResult"/>
    /// <remarks>
    /// This is a thin wrapper around <see cref="ClipboardHistoryItemsResult"/> to support mocking.
    /// </remarks>
    public interface IClipboardHistoryItemsResultWrapper : IInstanceWrapper<ClipboardHistoryItemsResult>
    {
        /// <inheritdoc cref="ClipboardHistoryItemsResult.Items"/>
        /// <remarks>
        /// Returned list elements are each wrapped as an <see cref="IClipboardHistoryItemWrapper"/>
        /// </remarks>
        IReadOnlyList<IClipboardHistoryItemWrapper> Items { get; }

        /// <inheritdoc cref="ClipboardHistoryItemsResult.Status"/>
        ClipboardHistoryItemsResultStatus Status { get; }
    }
}
