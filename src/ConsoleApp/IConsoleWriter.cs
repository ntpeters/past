using past.Core;
using past.Core.Wrappers;
using System.Threading.Tasks;

namespace past.ConsoleApp
{
    /// <summary>
    /// Represents a writer that can write to a connected console.
    /// </summary>
    public interface IConsoleWriter
    {
        /// <summary>
        /// Formats the given value with the provided type, index, formatted, and line ending preference.
        /// Writes the formatted value to the output stream of the connected console.
        /// </summary>
        /// <param name="item">Item to format and write the value of.</param>
        /// <param name="type">Type of the value to get from the item.</param>
        /// <param name="index">Optional index to format the value with.</param>
        /// <param name="formatter">Optional formatter to use for formatting the value.</param>
        /// <param name="emitLineEnding">Whether to emit a line ending after writing the item value.</param>
        /// <returns>A task representing the operation.</returns>
        Task WriteItemAsync(IClipboardHistoryItemWrapper item, ContentType type, int? index = null, IValueFormatter? formatter = null, bool emitLineEnding = false);

        /// <summary>
        /// Formats the given value with the provided formatter then writes it to the output stream of the connected console.
        /// </summary>
        /// <param name="value">Value to format and write.</param>
        /// <param name="formatter">Optional formatter to use for formatting the value.</param>
        void WriteValue(string? value, IValueFormatter? formatter = null);

        /// <summary>
        /// Writes the given text to the output stream of the connected console, followed by the current environment's line ending.
        /// </summary>
        /// <param name="text">Text to write.</param>
        void WriteLine(string text);

        /// <summary>
        /// Writes the given text to the error stream of the connected console, followed by the current environment's line ending.
        /// </summary>
        /// <param name="text">Text to write.</param>
        void WriteErrorLine(string text);
    }
}
