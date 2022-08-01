using System.Diagnostics.CodeAnalysis;

namespace past.ConsoleApp
{
    /// <summary>
    /// Utilities for controlling the current console.
    /// </summary>
    public interface IConsoleUtilities
    {
        /// <summary>
        /// Enables virtual terminal processing in the current console.
        /// </summary>
        /// <param name="error">Error message if enabling virtual terminal processing fails.</param>
        /// <returns>True if enabling virtual terminal processing succeeds, false otherwise.</returns>
        bool TryEnableVirtualTerminalProcessing([NotNullWhen(false)] out string? error);
    }
}
