using System;

namespace past.ConsoleApp.Wrappers
{
    /// <summary>
    /// Constants used by imported native methods.
    /// </summary>
    public static class NativeConstants
    {
        /// <summary>
        /// The ANSI escape character.
        /// </summary>
        public const char ANSI_ESCAPE = '\u001b';

        /// <summary>
        /// Returns all attributes for the format of the screen and text to the default state prior to modification.
        /// </summary>
        /// <remarks>
        /// For more info, see: <see href="https://docs.microsoft.com/en-us/windows/console/console-virtual-terminal-sequences#text-formatting"/>
        /// </remarks>
        public static readonly string ANSI_RESET = $"{ANSI_ESCAPE}[0m";

        /// <summary>
        /// Represents an invalid handle.
        /// </summary>
        public static readonly IntPtr INVALID_HANDLE_VALUE = new(-1);

        /// <summary>
        /// The standard input device. Initially, this is the console input buffer, CONIN$.
        /// </summary>
        public const int STD_INPUT_HANDLE = -10;

        /// <summary>
        /// The standard output device. Initially, this is the active console screen buffer, CONOUT$.
        /// </summary>
        public const int STD_OUTPUT_HANDLE = -11;

        /// <summary>
        /// The standard error device. Initially, this is the active console screen buffer, CONOUT$.
        /// </summary>
        public const int STD_ERROR_HANDLE = -12;

        /// <summary>
        /// When this value is provided as the mode when setting the console mode, the console mode is cleared.
        /// </summary>
        public const int CLEAR_CONSOLE_MODE = 0;

        /// <summary>
        /// Characters written or echoed to the console are parsed for ASCII control sequences, and the correct action is performed.
        /// Backspace, tab, bell, carriage return, and line feed characters are processed.
        /// <br/>
        /// It should be enabled when using control sequences or when ENABLE_VIRTUAL_TERMINAL_PROCESSING is set.
        /// </summary>
        /// <remarks>
        /// For more info, see: <see href="https://docs.microsoft.com/en-us/windows/console/setconsolemode#parameters"/>
        /// </remarks>
        public const uint ENABLE_PROCESSED_OUTPUT = 0x0001;

        /// <summary>
        /// When writing writing or echoing to the console, the cursor moves to the beginning of the
        /// next row when it reaches the end of the current row.
        /// <br/>
        /// This causes the rows displayed in the console window to scroll up automatically when the
        /// cursor advances beyond the last row in the window.
        /// It also causes the contents of the console screen buffer to scroll up (discarding the top row of the console screen buffer)
        /// when the cursor advances beyond the last row in the console screen buffer.
        /// <br/>
        /// If this mode is disabled, the last character in the row is overwritten with any subsequent characters.
        /// </summary>
        /// <remarks>
        /// For more info, see: <see href="https://docs.microsoft.com/en-us/windows/console/setconsolemode#parameters"/>
        /// </remarks>
        public const uint ENABLE_WRAP_AT_EOL_OUTPUT = 0x0002;

        /// <summary>
        /// When writing to the console, characters are parsed for VT100 and similar control character sequences that
        /// control cursor movement, color/font mode, and other operations that can also be performed via the existing
        /// Console APIs.
        /// <br/>
        /// Ensure <see cref="ENABLE_PROCESSED_OUTPUT"/> is set when using this flag.
        /// </summary>
        /// <remarks>
        /// For more info, see: <see href="https://docs.microsoft.com/en-us/windows/console/setconsolemode#parameters"/>
        /// </remarks>
        public const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;

        /// <summary>
        /// When writing to the console, this adds an additional state to end-of-line wrapping that can delay the
        /// cursor move and buffer scroll operations.
        /// <br/><br/>
        /// Normally when ENABLE_WRAP_AT_EOL_OUTPUT is set and text reaches the end of the line, the cursor will
        /// immediately move to the next line and the contents of the buffer will scroll up by one line.
        /// In contrast with this flag set, the cursor does not move to the next line, and the scroll operation
        /// is not performed. The written character will be printed in the final position on the line and the
        /// cursor will remain above this character as if ENABLE_WRAP_AT_EOL_OUTPUT was off, but the next printable
        /// character will be printed as if ENABLE_WRAP_AT_EOL_OUTPUT is on.No overwrite will occur.
        /// Specifically, the cursor quickly advances down to the following line, a scroll is performed if necessary,
        /// the character is printed, and the cursor advances one more position.
        /// <br/><br/>
        /// The typical usage of this flag is intended in conjunction with setting <see cref="ENABLE_VIRTUAL_TERMINAL_PROCESSING"/>
        /// to better emulate a terminal emulator where writing the final character on the screen (in the bottom right corner)
        /// without triggering an immediate scroll is the desired behavior.
        /// </summary>
        /// <remarks>
        /// For more info, see: <see href="https://docs.microsoft.com/en-us/windows/console/setconsolemode#parameters"/>
        /// </remarks>
        public const uint DISABLE_NEWLINE_AUTO_RETURN = 0x0008;

        /// <summary>
        /// Setting this flag directs the Virtual Terminal processing engine to convert user input received by the console
        /// window into Console Virtual Terminal Sequences that can be retrieved by a supporting application when reading
        /// the console input..
        /// <br/>
        /// The typical usage of this flag is intended in conjunction with <see cref="ENABLE_VIRTUAL_TERMINAL_PROCESSING"/>
        /// on the output handle to connect to an application that communicates exclusively via virtual terminal sequences.
        /// </summary>
        /// <remarks>
        /// For more info, see: <see href="https://docs.microsoft.com/en-us/windows/console/setconsolemode#parameters"/>
        /// </remarks>
        public const uint ENABLE_VIRTUAL_TERMINAL_INPUT = 0x0200;

        /// <summary>
        /// The function allocates a buffer large enough to hold the formatted message, and places a pointer to the
        /// allocated buffer at the address specified by lpBuffer.
        /// <br/>
        /// If the length of the formatted message exceeds 128K bytes, then
        /// <see cref="NativeMethods.FormatMessage(uint, IntPtr, uint, uint, out string, uint, IntPtr)"/>
        /// will fail and a subsequent call to GetLastError will return ERROR_MORE_DATA.
        /// </summary>
        /// <remarks>
        /// For more info, see: <see href="https://docs.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-formatmessage"/>
        /// </remarks>
        public const uint FORMAT_MESSAGE_ALLOCATE_BUFFER = 0x00000100;

        /// <summary>
        /// Insert sequences in the message definition such as %1 are to be ignored and passed
        /// through to the output buffer unchanged.
        ///<br/>
        /// This flag is useful for fetching a message for later formatting.
        /// <br/>
        /// If this flag is set, the Arguments parameter is ignored.
        /// </summary>
        /// <remarks>
        /// For more info, see: <see href="https://docs.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-formatmessage"/>
        /// </remarks>
        public const uint FORMAT_MESSAGE_IGNORE_INSERTS = 0x00000200;

        /// <summary>
        /// The function should search the system message-table resource(s) for the requested message.
        /// <br/>
        /// If this flag is specified with FORMAT_MESSAGE_FROM_HMODULE, the function searches the system
        /// message table if the message is not found in the module specified by lpSource.
        /// <br/>
        /// This flag cannot be used with FORMAT_MESSAGE_FROM_STRING.
        /// <br/>
        /// If this flag is specified, an application can pass the result of the <see cref="NativeMethods.GetLastError"/>
        /// function to retrieve the message text for a system-defined error.
        /// </summary>
        /// <remarks>
        /// For more info, see: <see href="https://docs.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-formatmessage"/>
        /// </remarks>
        public const uint FORMAT_MESSAGE_FROM_SYSTEM = 0x00001000;
    }
}
