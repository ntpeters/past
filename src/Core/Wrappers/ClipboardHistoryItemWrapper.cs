using System.Diagnostics.CodeAnalysis;
using Windows.ApplicationModel.DataTransfer;

namespace past.Core.Wrappers
{
    /// <inheritdoc cref="IClipboardHistoryItemWrapper"/>
    [ExcludeFromCodeCoverage(Justification = "Wrappers are not intended to be tested, as they exist solely to enable dependency injection of non-mockable APIs.")]
    public class ClipboardHistoryItemWrapper : IClipboardHistoryItemWrapper
    {
        #region Private Fields
        private readonly ClipboardHistoryItem _clipboardHistoryItem;
        #endregion Private Fields

        #region Public Properties
        public ClipboardHistoryItem WrappedInstance => _clipboardHistoryItem;

        public DataPackageView Content => _clipboardHistoryItem.Content;

        public string Id => _clipboardHistoryItem.Id;

        public DateTimeOffset Timestamp => _clipboardHistoryItem.Timestamp;
        #endregion Public Properties

        public ClipboardHistoryItemWrapper(ClipboardHistoryItem clipboardHistoryItem)
        {
            _clipboardHistoryItem = clipboardHistoryItem ?? throw new ArgumentNullException(nameof(clipboardHistoryItem));
        }
    }
}
