using System;

namespace past.ConsoleApp.Wrappers
{
    /// <inheritdoc cref="INativeMethodsWrapper"/>
    internal class NativeMethodsWrapper : INativeMethodsWrapper
    {
        public bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode)
            => NativeMethods.GetConsoleMode(hConsoleHandle, out lpMode);

        public bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode)
            => NativeMethods.SetConsoleMode(hConsoleHandle, dwMode);

        public IntPtr GetStdHandle(int nStdHandle)
            => NativeMethods.GetStdHandle(nStdHandle);

        public uint GetLastError()
            => NativeMethods.GetLastError();

        public uint FormatMessage(uint dwFlags, IntPtr lpSource, uint dwMessageId, uint dwLanguageId, out string lpBuffer, uint nSize, IntPtr arguments)
            => NativeMethods.FormatMessage(dwFlags, lpSource, dwMessageId, dwLanguageId, out lpBuffer, nSize, arguments);
    }
}
