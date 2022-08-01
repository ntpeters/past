using past.Core.Wrappers;
using Windows.ApplicationModel.DataTransfer;

namespace past.Core
{
    /// <summary>
    /// Manages the system clipboard and clipboard history.
    /// </summary>
    public interface IClipboardManager
    {
        /// <summary>
        /// Gets the value currently stored on the clipboard, filtered by the given content type.
        /// </summary>
        /// <param name="type">Content type to filter on.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> representing request cancellation.</param>
        /// <returns>
        /// Value currently stored on the clipboard, or null if nothing is currently stored on the clipboard
        /// or does not match the given <paramref name="type"/>.
        /// </returns>
        Task<string?> GetCurrentClipboardValueAsync(ContentType type, CancellationToken? cancellationToken = null);

        /// <summary>
        /// Gets the specified item from the clipboard history, and optionally sets it as the current clipboard value.
        /// </summary>
        /// <param name="identifier">Identifier describing an item in the clipboard history.</param>
        /// <param name="setCurrent">Whether to set the specified item as the current clipboard value.</param>
        /// <param name="type">Content type to filter on.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> representing request cancellation.</param>
        /// <returns>
        /// A <see cref="ValueTuple{IClipboardHistoryItemWrapper, SetHistoryItemAsContentStatus?}"/> containing the item and the status of setting the item as the current clipboard value.
        /// If <paramref name="setCurrent"/> was false, then the status will be null.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="identifier"/> is null.</exception>
        Task<(IClipboardHistoryItemWrapper Item, SetHistoryItemAsContentStatus? SetCurrentStatus)> GetClipboardHistoryItemAsync(ClipboardItemIdentifier identifier, bool setCurrent, ContentType type, CancellationToken? cancellationToken = null);

        /// <summary>
        /// Gets all items in the clipboard history, filtered on <paramref name="type"/> and <paramref name="pinned"/>.
        /// </summary>
        /// <param name="type">Content type to filter on.</param>
        /// <param name="pinned">Whether to only return pinned clipboard items.</param>
        /// <param name="cancellationToken">see cref="CancellationToken"/> representing request cancellation.</param>
        /// <returns>List of all clipboard history items. filtered on on <paramref name="type"/> and <paramref name="pinned"/>.</returns>
        Task<IEnumerable<IClipboardHistoryItemWrapper>> ListClipboardHistoryAsync(ContentType type, bool pinned, CancellationToken? cancellationToken = null);

        /// <inheritdoc cref="WinRtClipboardWrapper.IsHistoryEnabled"/>
        bool IsHistoryEnabled();

        /// <inheritdoc cref="WinRtClipboardWrapper.IsRoamingEnabled"/>
        bool IsRoamingEnabled();
    }
}
