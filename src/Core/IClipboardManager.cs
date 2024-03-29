using past.Core.Models;
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
        /// <exception cref="TimeoutException">Getting the current clipboard value times out.</exception>
        Task<string?> GetCurrentClipboardValueAsync(ContentType type, CancellationToken? cancellationToken = null);

        /// <summary>
        /// Gets the specified item from the clipboard history.
        /// </summary>
        /// <param name="identifier">Identifier describing an item in the clipboard history.</param>
        /// <param name="type">Content type to filter on.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> representing request cancellation.</param>
        /// <returns>The specified item, if found.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="identifier"/> is null.</exception>
        /// <exception cref="PastException">Failure to retrieve the clipboard history, specified item isn't found, or item doesn't match the specified content type.</exception>
        Task<IClipboardHistoryItemWrapper> GetClipboardHistoryItemAsync(ClipboardItemIdentifier identifier, ContentType type, CancellationToken? cancellationToken = null);

        /// <summary>
        /// Gets all items in the clipboard history, filtered on <paramref name="type"/> and <paramref name="pinned"/>.
        /// </summary>
        /// <param name="type">Content type to filter on.</param>
        /// <param name="pinned">Whether to only return pinned clipboard items.</param>
        /// <param name="cancellationToken">see cref="CancellationToken"/> representing request cancellation.</param>
        /// <returns>List of all clipboard history items. filtered on on <paramref name="type"/> and <paramref name="pinned"/>.</returns>
        /// <exception cref="PastException">Failure to retrieve the clipboard history, failure getting pinned items, or no items  match the specified content type.</exception>
        Task<IEnumerable<IClipboardHistoryItemWrapper>> GetClipboardHistoryAsync(ContentType type, bool pinned, CancellationToken? cancellationToken = null);

        /// <summary>
        /// Pins the specified item in the clipboard history.
        /// </summary>
        /// <param name="identifier">Identifier describing an item in the clipboard history.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> representing request cancellation.</param>
        /// <returns>A task representing the operation.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="identifier"/> is null.</exception>
        /// <exception cref="PastException">
        /// Failure to retrieve the clipboard history, specified item isn't found, the item is already pinned, or there's an error accessing the files on disk.
        /// </exception>
        Task PinClipboardItemAsync(ClipboardItemIdentifier identifier, CancellationToken? cancellationToken = null);

        /// <summary>
        /// Unpins the specified pinned item in the clipboard history.
        /// </summary>
        /// <param name="identifier">Identifier describing an item in the clipboard history.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> representing request cancellation.</param>
        /// <returns>A task representing the operation.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="identifier"/> is null.</exception>
        /// <exception cref="PastException">
        /// Failure to retrieve the clipboard history, specified item isn't found, the item is not pinned, or there's an error accessing the files on disk.
        /// </exception>
        Task UnpinClipboardItemAsync(ClipboardItemIdentifier identifier, CancellationToken? cancellationToken = null);

        /// <inheritdoc cref="WinRtClipboardWrapper.SetHistoryItemAsContent(IClipboardHistoryItemWrapper)"/>
        SetHistoryItemAsContentStatus SetHistoryItemAsContent(IClipboardHistoryItemWrapper item);

        /// <inheritdoc cref="WinRtClipboardWrapper.IsHistoryEnabled"/>
        bool IsHistoryEnabled();

        /// <inheritdoc cref="WinRtClipboardWrapper.IsRoamingEnabled"/>
        bool IsRoamingEnabled();
    }
}
