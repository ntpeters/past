using System;
using System.Collections.Generic;
using Windows.ApplicationModel.DataTransfer;

namespace past.Extensions
{
    public static class ReadOnlyListExtensions
    {
        public static int IndexOf(this IReadOnlyList<ClipboardHistoryItem> items, ClipboardHistoryItem item) => IndexOf(items, item.Id);

        public static int IndexOf(this IReadOnlyList<ClipboardHistoryItem> items, string id)
        {
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].Id == id)
                {
                    return i;
                }
            }

            throw new ArgumentOutOfRangeException(id);
        }
    }
}
