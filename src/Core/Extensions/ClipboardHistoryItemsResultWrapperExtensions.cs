using past.Core.Models;
using past.Core.Wrappers;
using System.Diagnostics.CodeAnalysis;
using Windows.ApplicationModel.DataTransfer;

namespace past.Core.Extensions
{
    /// <summary>
    /// Extensions for <see cref="ClipboardHistoryItemsResultWrapper"/>.
    /// </summary>
    public static class ClipboardHistoryItemsResultWrapperExtensions
    {
        /// <summary>
        /// Gets the <see cref="IClipboardHistoryItemWrapper"/> described by the given identifier.
        /// </summary>
        /// <param name="result">Result to get the item from.</param>
        /// <param name="identifier">Identifier describing which item to get.</param>
        /// <param name="clipboardHistoryItem">The item if found, otherwise null.</param>
        /// <returns>True if the item was found, false otherwise.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="result"/> or <paramref name="identifier"/> is null.</exception>
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
                clipboardHistoryItem = result.Items.FirstOrDefault(item => Guid.Parse(item.Id) == guid.Value);
            }

            return clipboardHistoryItem != null;
        }
    }
}
