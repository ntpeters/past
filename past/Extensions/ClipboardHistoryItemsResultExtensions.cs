using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Windows.ApplicationModel.DataTransfer;

namespace past.Extensions
{
    public static class ClipboardHistoryItemsResultExtensions
    {
        public static bool TryGetItem(this ClipboardHistoryItemsResult result, ClipboardItemIdentifier identifier, [NotNullWhen(true)] out ClipboardHistoryItem? clipboardHistoryItem)
        {
            clipboardHistoryItem = null;
            if (identifier.TryGetAsIndex(out int index))
            {
                clipboardHistoryItem = result.Items.ElementAt(index);
            }
            else if (identifier.TryGetAsGuid(out Guid guid))
            {
                clipboardHistoryItem = result.Items.FirstOrDefault(item => new Guid(item.Id) == guid);
            }

            return clipboardHistoryItem != null;
        }

        public static int IndexOf(this ClipboardHistoryItemsResult result, ClipboardHistoryItem item) => result.Items.IndexOf(item);
        public static int IndexOf(this ClipboardHistoryItemsResult result, string id) => result.Items.IndexOf(id);
    }
}
