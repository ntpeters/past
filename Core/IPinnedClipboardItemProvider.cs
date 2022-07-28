using System.Diagnostics.CodeAnalysis;

namespace past.Core
{
    public interface IPinnedClipboardItemProvider
    {
        bool TryGetPinnedClipboardHistoryItemIds([NotNullWhen(true)] out HashSet<string>? pinnedItemIds, [NotNullWhen(false)] out string? errorMessage);
    }
}
