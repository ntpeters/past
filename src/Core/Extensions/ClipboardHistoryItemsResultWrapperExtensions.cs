using past.Core.Wrappers;
using System.Diagnostics.CodeAnalysis;

namespace past.Core.Extensions
{
    public static class ClipboardHistoryItemsResultWrapperExtensions
    {
        public static bool TryGetItem(this IClipboardHistoryItemsResultWrapper result, ClipboardItemIdentifier identifier, [NotNullWhen(true)] out IClipboardHistoryItemWrapper? clipboardHistoryItem)
        {
            clipboardHistoryItem = null;
            if (identifier.TryGetAsIndex(out int? index))
            {
                clipboardHistoryItem = result.Items.ElementAt(index.Value);
            }
            else if (identifier.TryGetAsGuid(out Guid? guid))
            {
                clipboardHistoryItem = result.Items.FirstOrDefault(item => new Guid(item.Id) == guid);
            }

            return clipboardHistoryItem != null;
        }
    }
}
