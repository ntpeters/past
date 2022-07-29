using Windows.ApplicationModel.DataTransfer;

namespace past.Core.Wrappers
{
    /// <inheritdoc cref="IClipboardHistoryItemsResultWrapper"/>
    public class ClipboardHistoryItemsResultWrapper : IClipboardHistoryItemsResultWrapper
    {
        #region Private Fields
        private readonly ClipboardHistoryItemsResult _clipboardHistoryItemsResult;
        private readonly Lazy<IReadOnlyList<IClipboardHistoryItemWrapper>> _wrappedClipboardHistoryItemsLazy;
        #endregion Private Fields

        #region Public Properties
        public ClipboardHistoryItemsResult WrappedInstance => _clipboardHistoryItemsResult;

        public IReadOnlyList<IClipboardHistoryItemWrapper> Items => _wrappedClipboardHistoryItemsLazy.Value;

        public ClipboardHistoryItemsResultStatus Status => _clipboardHistoryItemsResult.Status;
        #endregion Public Properties

        public ClipboardHistoryItemsResultWrapper(ClipboardHistoryItemsResult clipboardHistoryItemsResult)
        {
            _clipboardHistoryItemsResult = clipboardHistoryItemsResult ?? throw new ArgumentNullException(nameof(clipboardHistoryItemsResult));

            // Wrap the list items lazily and preserve the wrapped instances for future calls
            _wrappedClipboardHistoryItemsLazy = new Lazy<IReadOnlyList<IClipboardHistoryItemWrapper>>(() =>
            {
                return _clipboardHistoryItemsResult.Items.Select(item => new ClipboardHistoryItemWrapper(item)).ToList();
            });
        }
    }
}
