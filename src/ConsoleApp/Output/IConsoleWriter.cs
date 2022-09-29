using past.Core;
using past.Core.Wrappers;
using System.Threading.Tasks;

namespace past.ConsoleApp.Output
{
    /// <summary>
    /// Represents a writer that can write to a connected console.
    /// </summary>
    public interface IConsoleWriter
    {
        /// <summary>
        /// When true, error messages will be suppressed.
        /// </summary>
        bool SuppressErrorOutput { get; }

        /// <summary>
        /// Whether virtual terminal processing was requested to be enabled for the connected console.
        /// </summary>
        bool EnableAnsiProcessing { get; }

        /// <summary>
        /// Controls whether to emit ANSI reset after writing values to the console.
        /// </summary>
        AnsiResetType AnsiResetType { get; }

        /// <summary>
        /// Formats the given value with the provided type, index, formatted, and line ending preference.
        /// Writes the formatted value to the output stream of the connected console.
        /// </summary>
        /// <param name="item">Item to format and write the value of.</param>
        /// <param name="type">Type of the value to get from the item.</param>
        /// <param name="formatter">Optional formatter to use for formatting the value.</param>
        /// <param name="emitLineEnding">Whether to emit a line ending after writing the item value.</param>
        /// <returns>A task representing the operation.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="item"/> is null.</exception>
        Task WriteItemAsync(IClipboardHistoryItemWrapper item, ContentType type, IValueFormatter? formatter = null, bool emitLineEnding = false);

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
