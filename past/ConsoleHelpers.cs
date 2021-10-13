using System;
using System.CommandLine.Rendering;
using System.Runtime.InteropServices;

namespace past
{
    public class ConsoleHelpers
    {
        public const char ANSI_ESCAPE = '\u001b';
        public static readonly string ANSI_RESET = $"{ANSI_ESCAPE}[0m";

        private static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);
        private const int STD_INPUT_HANDLE = -10;
        private const int STD_OUTPUT_HANDLE = -11;

        private const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;
        private const uint DISABLE_NEWLINE_AUTO_RETURN = 0x0008;
        private const uint ENABLE_VIRTUAL_TERMINAL_INPUT = 0x0200;

        private const uint FORMAT_MESSAGE_ALLOCATE_BUFFER = 0x00000100;
        private const uint FORMAT_MESSAGE_IGNORE_INSERTS = 0x00000200;
        private const uint FORMAT_MESSAGE_FROM_SYSTEM = 0x00001000;

        [DllImport("kernel32.dll")]
        private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [DllImport("kernel32.dll")]
        private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll")]
        private static extern uint GetLastError();

        [DllImport("Kernel32.dll", SetLastError = true)]
        private static extern uint FormatMessage(uint dwFlags, IntPtr lpSource, uint dwMessageId, uint dwLanguageId, out string lpBuffer, uint nSize, IntPtr Arguments);

        

        public static string GetSystemErrorMessage(uint errorCode)
        {
            if (FormatMessage(
                FORMAT_MESSAGE_ALLOCATE_BUFFER | FORMAT_MESSAGE_FROM_SYSTEM | FORMAT_MESSAGE_IGNORE_INSERTS,
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

        public static string GetLastErrorMessage()
        {
            var errorCode = GetLastError();
            var errorMessage = GetSystemErrorMessage(errorCode);
            return $"Error {errorCode}: {errorMessage}";
        }

        public static bool TryEnableVirtualTerminalInput(out string error)
        {
            var iStdIn = GetStdHandle(STD_INPUT_HANDLE);
            if (iStdIn == INVALID_HANDLE_VALUE)
            {
                error = GetLastErrorMessage();
                return false;
            }

            if (!GetConsoleMode(iStdIn, out uint inConsoleMode))
            {
                error = GetLastErrorMessage();
                return false;
            }

            inConsoleMode |= ENABLE_VIRTUAL_TERMINAL_INPUT;
            if (!SetConsoleMode(iStdIn, inConsoleMode))
            {
                error = GetLastErrorMessage();
                return false;
            }

            error = string.Empty;
            return true;
        }

        public static bool TryEnableVirtualTerminalProcessing(out string error, bool useCommandLineInteropApi = true)
        {
            if (useCommandLineInteropApi)
            {
                var vtMode = VirtualTerminalMode.TryEnable();
                if (vtMode == null)
                {
                    error = "VTMode is null";
                    return false;
                }

                if (!vtMode.IsEnabled)
                {
                    if (vtMode.Error != null)
                    {
                        error = $"VTMode Error {vtMode.Error}: {GetSystemErrorMessage((uint)vtMode.Error)}";
                    }
                    else
                    {
                        error = "VTMode error is null";
                    }
                    return false;
                }
            }
            else
            {
                var iStdOut = GetStdHandle(STD_OUTPUT_HANDLE);
                if (iStdOut == INVALID_HANDLE_VALUE)
                {
                    error = GetLastErrorMessage();
                    return false;
                }

                if (!GetConsoleMode(iStdOut, out uint outConsoleMode))
                {
                    error = GetLastErrorMessage();
                    return false;
                }

                outConsoleMode |= ENABLE_VIRTUAL_TERMINAL_PROCESSING | DISABLE_NEWLINE_AUTO_RETURN;
                if (!SetConsoleMode(iStdOut, outConsoleMode))
                {
                    error = GetLastErrorMessage();
                    return false;
                }
            }

            error = string.Empty;
            return true;
        }
    }
}
