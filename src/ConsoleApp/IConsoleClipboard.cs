using past.ConsoleApp.Output;
using past.Core;
using past.Core.Models;
using System;
using System.CommandLine.Invocation;
using System.Threading;
using System.Threading.Tasks;

namespace past.ConsoleApp
{
    /// <summary>
    /// Writes values from the system clipboard and clipboard history to a console.
    /// </summary>
    public interface IConsoleClipboard
    {
        /// <summary>
        /// Writes the current status of the system clipboard history and clipboard roaming settings.
        /// </summary>
        /// <param name="consoleWriter">Writer to write to the console with.</param>
        /// <param name="context">Invocation context to set the exit code.</param>
        /// <exception cref="ArgumentNullException"><paramref name="consoleWriter"/> or <paramref name="context"/> is null.</exception>
        void GetClipboardHistoryStatus(IConsoleWriter consoleWriter, InvocationContext context);

        /// <summary>
        /// Writes the value currently stored on the clipboard to the provided console writer, if compatible with the specified type.
        /// </summary>
        /// <param name="consoleWriter">Writer to write to the console with.</param>
        /// <param name="formatter">Formatter to use for formatting the value.</param>
        /// <param name="type">Content type to filter on.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> representing request cancellation.</param>
        /// <returns>Zero on success, a negative value otherwise.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="consoleWriter"/> or <paramref name="formatter"/> is null.</exception>
        Task<int> GetCurrentClipboardValueAsync(IConsoleWriter consoleWriter, IValueFormatter formatter, ContentType type, CancellationToken cancellationToken);

        /// <summary>
        /// Writes the specified item from the clipboard history to the provided console writer, if compatible with the specified type,
        /// and optionally sets it as the current clipboard value.
        /// </summary>
        /// <param name="consoleWriter">Writer to write to the console with.</param>
        /// <param name="formatter">Formatter to use for formatting the value.</param>
        /// <param name="identifier">Identifier describing an item in the clipboard history.</param>
        /// <param name="type">Content type to filter on.</param>
        /// <param name="setCurrent">Whether to set the specified item as the current clipboard value.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> representing request cancellation.</param>
        /// <returns>Zero on success, a negative value otherwise.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="consoleWriter"/>, <paramref name="formatter"/>, or <paramref name="identifier"/> is null.
        /// </exception>
        Task<int> GetClipboardHistoryItemAsync(IConsoleWriter consoleWriter, IValueFormatter formatter, ClipboardItemIdentifier identifier, ContentType type, bool setCurrent, CancellationToken cancellationToken);

        /// <summary>
        /// Writes all items in the clipboard history to the provided console writer,
        /// filtered on <paramref name="type"/> and <paramref name="pinned"/>.
        /// </summary>
        /// <param name="consoleWriter">Writer to write to the console with.</param>
        /// <param name="formatter">Formatter to use for formatting the value.</param>
        /// <param name="type">Content type to filter on.</param>
        /// <param name="pinned">Whether to only return pinned clipboard items.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> representing request cancellation.</param>
        /// <returns>Zero on success, a negative value otherwise.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="consoleWriter"/> or <paramref name="formatter"/> is null.</exception>
        Task<int> ListClipboardHistoryAsync(IConsoleWriter consoleWriter, IValueFormatter formatter, ContentType type, bool pinned, CancellationToken cancellationToken);

        /// <summary>
        /// Pins the specified item in clipboard history.
        /// </summary>
        /// <param name="consoleWriter">Writer to write to the console with.</param>
        /// <param name="identifier">Identifier describing an item in the clipboard history.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> representing request cancellation.</param>
        /// <returns>Zero on success, a negative value otherwise.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="consoleWriter"/> or <paramref name="identifier"/> is null.
        /// </exception>
        Task<int> PinClipboardItemAsync(IConsoleWriter consoleWriter, ClipboardItemIdentifier identifier, CancellationToken cancellationToken);

        /// <summary>
        /// Unpins the specified pinned item in clipboard history.
        /// </summary>
        /// <param name="consoleWriter">Writer to write to the console with.</param>
        /// <param name="identifier">Identifier describing an item in the clipboard history.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> representing request cancellation.</param>
        /// <returns>Zero on success, a negative value otherwise.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="consoleWriter"/> or <paramref name="identifier"/> is null.
        /// </exception>
        Task<int> UnpinClipboardItemAsync(IConsoleWriter consoleWriter, ClipboardItemIdentifier identifier, CancellationToken cancellationToken);
    }
}
