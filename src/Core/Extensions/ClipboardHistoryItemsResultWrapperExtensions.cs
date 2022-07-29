using past.Core.Wrappers;
using System.Diagnostics.CodeAnalysis;
using Windows.ApplicationModel.DataTransfer;

namespace past.Core.Extensions
{
    public static class ClipboardHistoryItemsResultWrapperExtensions
    {
        public static bool TryGetItem(this IClipboardHistoryItemsResultWrapper result, ClipboardItemIdentifier identifier, [NotNullWhen(true)] out IClipboardHistoryItemWrapper? clipboardHistoryItem)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            if (identifier == null)
            {
                throw new ArgumentNullException(nameof(identifier));
            }

            clipboardHistoryItem = null;
            if (result.Status != ClipboardHistoryItemsResultStatus.Success)
            {
                return false;
            }

            if (identifier.TryGetAsIndex(out int? index))
            {
                clipboardHistoryItem = result.Items.ElementAtOrDefault(index.Value);
            }
            else if (identifier.TryGetAsGuid(out Guid? guid))
            {
                clipboardHistoryItem = result.Items.FirstOrDefault(item => new Guid(item.Id) == guid);
            }

            return clipboardHistoryItem != null;
        }
    }
}
