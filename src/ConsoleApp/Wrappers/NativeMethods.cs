using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace past.ConsoleApp.Wrappers
{
    /// <summary>
    /// Imported native methods for access via platform invoke.
    /// </summary>
    [ExcludeFromCodeCoverage(Justification = "Native methods are not tested.")]
    internal static class NativeMethods
    {
        /// <summary>
        /// Retrieves the current input mode of a console's input buffer or the current output mode of a console screen buffer.
        /// </summary>
        /// <remarks>
        /// For more info, see: <see href="https://docs.microsoft.com/en-us/windows/console/getconsolemode"/>.
        /// </remarks>
        /// <param name="hConsoleHandle">
        /// A handle to the console input buffer or a console screen buffer.
        /// The handle must have the GENERIC_READ access right.
        /// </param>
        /// <param name="lpMode">A pointer to a variable that receives the current mode of the specified buffer.</param>
        /// <returns>
        /// If the function succeeds, the return value is nonzero.
        /// If the function fails, the return value is zero.
        /// To get extended error information, call <see cref="GetLastError"/>.
        /// </returns>
        [DllImport("kernel32.dll")]
        internal static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);


        /// <summary>
        /// Sets the input mode of a console's input buffer or the output mode of a console screen buffer.
        /// </summary>
        /// <remarks>
        /// For more info, see: <see href="https://docs.microsoft.com/en-us/windows/console/setconsolemode"/>.
        /// </remarks>
        /// <param name="hConsoleHandle">
        /// A handle to the console input buffer or a console screen buffer.
        /// The handle must have the GENERIC_READ access right.
        /// </param>
        /// <param name="dwMode">The input or output mode to be set.</param>
        /// <returns>
        /// If the function succeeds, the return value is nonzero.
        /// If the function fails, the return value is zero.
        /// To get extended error information, call <see cref="GetLastError"/>.
        /// </returns>
        [DllImport("kernel32.dll")]
        internal static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        /// <summary>
        /// Retrieves a handle to the specified standard device (standard input, standard output, or standard error).
        /// </summary>
        /// <remarks>
        /// For more info, see: <see href="https://docs.microsoft.com/en-us/windows/console/getstdhandle"/>.
        /// </remarks>
        /// <param name="nStdHandle">
        /// The standard device.
        /// This parameter can be one of the following values:
        /// <list type="bullet">
        /// <item><see cref="NativeConstants.STD_INPUT_HANDLE"/></item>
        /// <item><see cref="NativeConstants.STD_OUTPUT_HANDLE"/></item>
        /// <item><see cref="NativeConstants.STD_ERROR_HANDLE"/></item>
        /// </list>
        /// </param>
        /// <returns>
        /// If the function succeeds, the return value is a handle to the specified device,
        /// or a redirected handle set by a previous call to SetStdHandle.
        /// The handle has GENERIC_READ and GENERIC_WRITE access rights, unless the application
        /// has used SetStdHandle to set a standard handle with lesser access.
        /// <br/>
        /// If the function fails, the return value is <see cref="NativeConstants.INVALID_HANDLE_VALUE"/>.
        /// To get extended error information, call <see cref="GetLastError"/>.
        /// <br/>
        /// If an application does not have associated standard handles, such as a service running on an interactive desktop,
        /// and has not redirected them, the return value is NULL.
        /// </returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr GetStdHandle(int nStdHandle);

        /// <summary>
        /// Retrieves the calling thread's last-error code value.
        /// The last-error code is maintained on a per-thread basis.
        /// Multiple threads do not overwrite each other's last-error code.
        /// </summary>
        /// <remarks>
        /// For more info, see: <see href="https://docs.microsoft.com/en-us/windows/win32/api/errhandlingapi/nf-errhandlingapi-getlasterror"/>.
        /// </remarks>
        /// <returns>
        /// The return value is the calling thread's last-error code.
        /// </returns>
        [DllImport("kernel32.dll")]
        internal static extern uint GetLastError();

        /// <summary>
        /// Formats a message string.
        /// <br/>
        /// The function requires a message definition as input.
        /// The message definition can come from a buffer passed into the function.
        /// It can come from a message table resource in an already-loaded module.
        /// Or the caller can ask the function to search the system's message table resource(s) for the message definition.
        /// <br/>
        /// The function finds the message definition in a message table resource based on a message identifier and a language identifier.
        /// The function copies the formatted message text to an output buffer, processing any embedded insert sequences if requested.
        /// </summary>
        /// <remarks>
        /// For more info, see: <see href="https://docs.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-formatmessage"/>
        /// </remarks>
        /// <param name="dwFlags">
        /// The formatting options, and how to interpret the <paramref name="lpSource"/> parameter.
        /// <br/>
        /// The low-order byte of <paramref name="dwFlags"/> specifies how the function handles line breaks in the output buffer.
        /// The low-order byte can also specify the maximum width of a formatted output line.
        /// </param>
        /// <param name="lpSource">
        /// The location of the message definition.
        /// The type of this parameter depends upon the settings in the <paramref name="dwFlags"/> parameter.
        /// </param>
        /// <param name="dwMessageId">
        /// The message identifier for the requested message.
        /// This parameter is ignored if <paramref name="dwFlags"/> includes FORMAT_MESSAGE_FROM_STRING.
        /// </param>
        /// <param name="dwLanguageId">
        /// The language identifier for the requested message.
        /// This parameter is ignored if <paramref name="dwFlags"/> includes FORMAT_MESSAGE_FROM_STRING.
        /// </param>
        /// <param name="lpBuffer">
        /// A pointer to a buffer that receives the null-terminated string that specifies the formatted message.
        /// <br/>
        /// If <paramref name="dwFlags"/> includes <see cref="NativeConstants.FORMAT_MESSAGE_ALLOCATE_BUFFER"/>, the function allocates a buffer,
        /// and places the pointer to the buffer at the address specified in <paramref name="lpBuffer"/>.
        /// <br/>
        /// This buffer cannot be larger than 64K bytes.
        /// </param>
        /// <param name="nSize">
        /// If the <see cref="NativeConstants.FORMAT_MESSAGE_ALLOCATE_BUFFER"/> flag is not set,
        /// this parameter specifies the size of the output buffer, in TCHARs.
        /// If <see cref="NativeConstants.FORMAT_MESSAGE_ALLOCATE_BUFFER"/> is set,
        /// this parameter specifies the minimum number of TCHARs to allocate for an output buffer.
        /// <br/>
        /// The output buffer cannot be larger than 64K bytes.
        /// </param>
        /// <param name="arguments">An array of values that are used as insert values in the formatted message.</param>
        /// <returns>
        /// If the function succeeds, the return value is the number of TCHARs stored in the output buffer, excluding the terminating null character.
        /// <br/>
        /// If the function fails, the return value is zero. To get extended error information, call <see cref="GetLastError"/>.
        /// </returns>
        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern uint FormatMessage(uint dwFlags, IntPtr lpSource, uint dwMessageId, uint dwLanguageId, out string lpBuffer, uint nSize, IntPtr arguments);
    }
}
