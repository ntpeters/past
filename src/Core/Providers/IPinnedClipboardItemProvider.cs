using past.Core.Wrappers;
using System.Diagnostics.CodeAnalysis;

namespace past.Core.Providers
{
    /// <summary>
    /// Provides metadata of pinned clipboard history items.
    /// </summary>
    public interface IPinnedClipboardItemProvider
    {
        /// <summary>
        /// Gets the IDs of all pinned clipboard history items.
        /// </summary>
        /// <param name="pinnedItemIds">IDs of pinned items in the clipboard history if successful, null otherwise.</param>
        /// <param name="errorMessage">Error message if getting pinned item metadata fails, null otherwise.</param>
        /// <returns>True if getting pinned item IDs succeeds, false otherwise.</returns>
        bool TryGetPinnedClipboardHistoryItemIds([NotNullWhen(true)] out HashSet<string>? pinnedItemIds, [NotNullWhen(false)] out string? errorMessage);

        /// <summary>
        /// Pins the given <paramref name="item"/>.
        /// </summary>
        /// <param name="item">Item to add to pinned items.</param>
        /// <param name="errorMessage">Error message if adding pinned item fails, null otherwise.</param>
        /// <returns>True if adding pinned item succeeds, false otherwise.</returns>
        bool TryAddPinnedClipboardHistoryItem(IClipboardHistoryItemWrapper item, [NotNullWhen(false)] out string? errorMessage);

        /// <summary>
        /// Unpins the given <paramref name="item"/>.
        /// </summary>
        /// <param name="item">Item to remove from pinned items.</param>
        /// <param name="errorMessage">Error message if removing pinned item fails, null otherwise.</param>
        /// <returns>True if removing pinned item succeeds, false otherwise.</returns>
        bool TryRemovePinnedClipboardHistoryItem(IClipboardHistoryItemWrapper item, [NotNullWhen(false)] out string? errorMessage);
    }
}
