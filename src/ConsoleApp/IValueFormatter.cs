using System;

namespace past.ConsoleApp
{
    /// <summary>
    /// Formats values for output.
    /// </summary>
    public interface IValueFormatter
    {
        /// <summary>
        /// When true, the ASCII NUL character will be used as the line ending in the formatted value.
        /// </summary>
        bool NullLineEnding { get; }

        /// <summary>
        /// When true, the provided index will be included in the formatted value.
        /// </summary>
        bool IncludeIndex { get; }

        /// <summary>
        /// When true, the provided ID will be included in the formatted value.
        /// </summary>
        bool IncludeId { get; }

        /// <summary>
        /// When true, the provided timestamp will be included in the formatted value.
        /// </summary>
        bool IncludeTimestamp { get; }

        /// <summary>
        /// Formats the given value to append ANSI reset and the current environment's line ending if specified.
        /// </summary>
        /// <param name="value">Value to format.</param>
        /// <param name="emitAnsiReset">Whether to append ANSI reset.</param>
        /// <param name="emitLineEnding">Whether to append the current environment's line terminator.</param>
        /// <returns>The formatted value.</returns>
        string Format(string value, bool emitAnsiReset, bool emitLineEnding = false);

        /// <summary>
        /// Formats the given value with the provided index, ID, and timestamp.
        /// Appends ANSI reset and the current environment's line ending if specified.
        /// </summary>
        /// <param name="value">Value to format.</param>
        /// <param name="index">The index to prepend, or null to omit the index.</param>
        /// <param name="id">The ID to include after the index.</param>
        /// <param name="timestamp">The timestamp to include after the ID.</param>
        /// <param name="emitAnsiReset">Whether to append ANSI reset.</param>
        /// <param name="emitLineEnding">Whether to append the current environment's line terminator.</param>
        /// <returns>The formatted value.</returns>
        string Format(string value, int? index, string id, DateTimeOffset timestamp, bool emitAnsiReset, bool emitLineEnding = false);
    }
}
