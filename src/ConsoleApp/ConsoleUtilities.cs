using past.ConsoleApp.Wrappers;
using System;
using System.CommandLine.Rendering;
using System.Diagnostics.CodeAnalysis;

namespace past.ConsoleApp
{
    /// <inheritdoc cref="IConsoleUtilities"/>
    public class ConsoleUtilities : IConsoleUtilities
    {
        #region Private Fields
        private readonly INativeMethodsWrapper _nativeMethods;
        #endregion Private Fields

        #region Constructors
        /// <summary>
        /// Creates a new <see cref="ConsoleUtilities"/> using the system native methods for interacting with the current console.
        /// </summary>
        public ConsoleUtilities()
            : this(new NativeMethodsWrapper())
        {
        }

        /// <summary>
        /// Creates a new <see cref="ConsoleUtilities"/> using the provided native methods for interacting with the current console.
        /// </summary>
        /// <param name="nativeMethods">Native methods to use for interacting with the console.</param>
        /// <exception cref="ArgumentNullException"><paramref name="nativeMethods"/> is null.</exception>
        public ConsoleUtilities(INativeMethodsWrapper nativeMethods)
        {
            _nativeMethods = nativeMethods ?? throw new ArgumentNullException(nameof(nativeMethods));
        }
        #endregion Constructors

        #region Public Methods
        public bool TryEnableVirtualTerminalProcessing([NotNullWhen(false)] out string? error)
        {
            var iStdOut = _nativeMethods.GetStdHandle(NativeConstants.STD_OUTPUT_HANDLE);
            if (iStdOut == NativeConstants.INVALID_HANDLE_VALUE)
            {
                error = GetLastErrorMessage();
                return false;
            }

            if (!_nativeMethods.GetConsoleMode(iStdOut, out uint outConsoleMode))
            {
                error = GetLastErrorMessage();
                return false;
            }

            outConsoleMode |= NativeConstants.ENABLE_VIRTUAL_TERMINAL_PROCESSING | NativeConstants.DISABLE_NEWLINE_AUTO_RETURN;
            if (!_nativeMethods.SetConsoleMode(iStdOut, outConsoleMode))
            {
                error = GetLastErrorMessage();
                return false;
            }

            error = null;
            return true;
        }

        public bool TryEnableVirtualTerminalInput([NotNullWhen(false)] out string? error)
        {
            var iStdIn = _nativeMethods.GetStdHandle(NativeConstants.STD_INPUT_HANDLE);
            if (iStdIn == NativeConstants.INVALID_HANDLE_VALUE)
            {
                error = GetLastErrorMessage();
                return false;
            }

            if (!_nativeMethods.GetConsoleMode(iStdIn, out uint inConsoleMode))
            {
                error = GetLastErrorMessage();
                return false;
            }

            inConsoleMode |= NativeConstants.ENABLE_VIRTUAL_TERMINAL_INPUT;
            if (!_nativeMethods.SetConsoleMode(iStdIn, inConsoleMode))
            {
                error = GetLastErrorMessage();
                return false;
            }

            error = null;
            return true;
        }

        public bool TryClearConsoleMode([NotNullWhen(true)] out uint? originalMode, [NotNullWhen(false)] out string? error)
        {
            var iStdOut = _nativeMethods.GetStdHandle(NativeConstants.STD_OUTPUT_HANDLE);
            if (iStdOut == NativeConstants.INVALID_HANDLE_VALUE)
            {
                error = GetLastErrorMessage();
                originalMode = null;
                return false;
            }

            if (!_nativeMethods.GetConsoleMode(iStdOut, out uint outConsoleMode))
            {
                error = GetLastErrorMessage();
                originalMode = null;
                return false;
            }

            if (!_nativeMethods.SetConsoleMode(iStdOut, NativeConstants.CLEAR_CONSOLE_MODE))
            {
                error = GetLastErrorMessage();
                originalMode = null;
                return false;
            }

            originalMode = outConsoleMode;
            error = null;
            return true;
        }
        #endregion Public Methods

        #region Private Methods
        /// <summary>
        /// Gets the system error message associated with the specified system error code.
        /// </summary>
        /// <param name="errorCode">System error code.</param>
        /// <returns>Message associated with the <paramref name="errorCode"/>.</returns>
        private string GetSystemErrorMessage(uint errorCode)
        {
            if (_nativeMethods.FormatMessage(
                NativeConstants.FORMAT_MESSAGE_ALLOCATE_BUFFER | NativeConstants.FORMAT_MESSAGE_FROM_SYSTEM | NativeConstants.FORMAT_MESSAGE_IGNORE_INSERTS,
                IntPtr.Zero,
                errorCode,
                0,
                out var message,
                0,
                IntPtr.Zero) == 0)
            {
                return string.Empty;
            }

            // FormatMessage always appends a newline
            return message.TrimEnd('\n');
        }

        /// <summary>
        /// Gets the system error message associated with the calling thread's last error code value.
        /// </summary>
        /// <returns>Message associated with the the calling thread's last error code value.</returns>
        private string GetLastErrorMessage()
        {
            var errorCode = _nativeMethods.GetLastError();
            var errorMessage = GetSystemErrorMessage(errorCode);
            return $"Error {errorCode}: {errorMessage}";
        }
        #endregion Private Methods

    }
}
