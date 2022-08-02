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

        /// <summary>
        /// Enables virtual terminal input in the current console.
        /// </summary>
        /// <param name="error">Error message if enabling virtual terminal input fails.</param>
        /// <returns>True if enabling virtual terminal input succeeds, false otherwise.</returns>
        bool TryEnableVirtualTerminalInput([NotNullWhen(false)] out string? error);

        /// <summary>
        /// Clears the console mode.
        /// </summary>
        /// <param name="originalMode">The console mode before it was cleared, if clearing the console mode was successful.</param>
        /// <param name="error">Error message if clearing the console mode fails.</param>
        /// <returns>True if clearing the console mode succeeds, false otherwise.</returns>
        bool TryClearConsoleMode([NotNullWhen(true)] out uint? originalMode, [NotNullWhen(false)] out string? error);
    }
}
