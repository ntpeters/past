using System.Diagnostics.CodeAnalysis;
using Windows.ApplicationModel.DataTransfer;

namespace past.Core.Wrappers
{
    /// <inheritdoc cref="IClipboardHistoryItemsResultWrapper"/>
    [ExcludeFromCodeCoverage(Justification = "Wrappers are not intended to be tested, as they exist solely to enable dependency injection of non-mockable APIs.")]
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
                var wrappedItems = new List<IClipboardHistoryItemWrapper>();
                int index = 0;
                foreach (var item in _clipboardHistoryItemsResult.Items)
                {
                    wrappedItems.Add(new ClipboardHistoryItemWrapper(item)
                    {
                        // We need to preserve the original index so that it can still be referenced after filtering the items if needed
                        Index = index
                    });
                    index++;
                }
                return wrappedItems;
            });
        }
    }
}
