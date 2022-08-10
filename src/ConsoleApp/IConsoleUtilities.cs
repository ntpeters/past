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
        /// <remarks>
        /// The original mode can be restored by calling <see cref="TryRestoreConsoleMode(out string?)"/>.
        /// </remarks>
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
        /// <remarks>
        /// The original mode can be restored by calling <see cref="TryRestoreConsoleMode(out string?)"/>.
        /// </remarks>
        /// <param name="error">Error message if clearing the console mode fails.</param>
        /// <returns>True if clearing the console mode succeeds, false otherwise.</returns>
        bool TryClearConsoleMode([NotNullWhen(false)] out string? error);

        /// <summary>
        /// Restores the original console mode before calling <see cref="TryEnableVirtualTerminalInput(out string?)"/>
        /// or <see cref="TryClearConsoleMode(out string?)"/>.
        /// </summary>
        /// <remarks>
        /// If the console mode was not altered by either <see cref="TryEnableVirtualTerminalInput(out string?)"/>
        /// or <see cref="TryClearConsoleMode(out string?)"/>, then this method will do nothing and return true.
        /// </remarks>
        /// <param name="error">Error message if restoring the console mode fails.</param>
        /// <returns>True if restoring the console mode succeeds, false otherwise.</returns>
        bool TryRestoreConsoleMode([NotNullWhen(false)] out string? error);
    }
}
