using System;

namespace past.ConsoleApp.Wrappers
{
    /// <summary>
    /// Exposes native methods for interacting with the console.
    /// </summary>
    /// <remarks>
    /// This is a thin wrapper around native methods to support mocking.
    /// </remarks>
    public interface INativeMethodsWrapper
    {
        /// <inheritdoc cref="NativeMethods.GetConsoleMode(IntPtr, out uint)"/>
        bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        /// <inheritdoc cref="NativeMethods.SetConsoleMode(IntPtr, uint)"/>
        bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        /// <inheritdoc cref="NativeMethods.GetStdHandle(int)"/>
        IntPtr GetStdHandle(int nStdHandle);

        /// <inheritdoc cref="NativeMethods.GetLastError"/>
        uint GetLastError();

        /// <inheritdoc cref="NativeMethods.FormatMessage(uint, IntPtr, uint, uint, out string, uint, IntPtr)"/>
        uint FormatMessage(uint dwFlags, IntPtr lpSource, uint dwMessageId, uint dwLanguageId, out string lpBuffer, uint nSize, IntPtr arguments);
    }
}
