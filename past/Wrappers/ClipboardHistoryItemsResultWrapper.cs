using System;
using System.Collections.Generic;
using System.Linq;
using Windows.ApplicationModel.DataTransfer;

namespace past.Wrappers
{
    /// <inheritdoc cref="IClipboardHistoryItemsResultWrapper"/>
    public class ClipboardHistoryItemsResultWrapper : IClipboardHistoryItemsResultWrapper
    {
        private readonly ClipboardHistoryItemsResult _clipboardHistoryItemsResult;
        private readonly Lazy<IReadOnlyList<IClipboardHistoryItemWrapper>> _wrappedClipboardHistoryItemsLazy;

        public ClipboardHistoryItemsResult WrappedInstance => _clipboardHistoryItemsResult;

        public IReadOnlyList<IClipboardHistoryItemWrapper> Items => _wrappedClipboardHistoryItemsLazy.Value;

        public ClipboardHistoryItemsResultStatus Status => _clipboardHistoryItemsResult.Status;

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
