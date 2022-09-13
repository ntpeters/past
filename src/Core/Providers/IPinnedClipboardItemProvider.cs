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
    }
}
