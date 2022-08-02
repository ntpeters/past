using past.ConsoleApp.Extensions;
using past.Core;
using past.Core.Wrappers;
using System;
using System.CommandLine;
using System.CommandLine.Rendering;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;

namespace past.ConsoleApp
{
    /// <inheritdoc cref="IConsoleWriter"/>
    public class ConsoleWriter : IConsoleWriter
    {
        #region Public Properties
        public  bool SuppressErrorOutput { get; }
        public  bool EnableAnsiProcessing { get; }
        public  AnsiResetType AnsiResetType { get; }
        #endregion Public Properties

        #region Private Fields
        private readonly IConsole _console;
        private readonly IConsoleUtilities _consoleUtilities;
        #endregion Private Fields

        #region Constructors
        /// <summary>
        /// Creates a new <see cref="ConsoleWriter"/> connected to the given console, with the provided options.
        /// </summary>
        /// <param name="console">Console to write output to.</param>
        /// <param name="consoleUtilities">Used to set the console mode.</param>
        /// <param name="suppressErrorOutput">Whether to silence error output.</param>
        /// <param name="enableAnsiProcessing">Whether to enable virtual terminal processing.</param>
        /// <param name="ansiResetType">Controls how to determine whether ANSI reset should be emitted.</param>
        /// <exception cref="ArgumentNullException"><paramref name="console"/> or <paramref name="consoleUtilities"/> is null.</exception>
        public ConsoleWriter(IConsole console, IConsoleUtilities consoleUtilities, bool suppressErrorOutput, bool enableAnsiProcessing, AnsiResetType ansiResetType)
        {
            _console = console ?? throw new ArgumentNullException(nameof(console));
            _consoleUtilities = consoleUtilities ?? throw new ArgumentNullException(nameof(consoleUtilities));
            SuppressErrorOutput = suppressErrorOutput;
            EnableAnsiProcessing = enableAnsiProcessing;
            AnsiResetType = ansiResetType;
        }
        #endregion Constructors

        #region Public Methods
        public async Task WriteItemAsync(IClipboardHistoryItemWrapper item, ContentType type, int? index = null, IValueFormatter? formatter = null, bool emitLineEnding = false)
        {
            _ = item ?? throw new ArgumentNullException(nameof(item));

            var value = await GetClipboardItemValueAsync(item.Content, type);
            if (value == null)
            {
                return;
            }

            if (formatter != null)
            {
                var emitAnsiReset = ShouldEmitAnsiReset(value);
                value = formatter.Format(value, index, item.Id, item.Timestamp, emitAnsiReset, emitLineEnding);
            }

            WriteValueInternal(value);
        }

        public void WriteValue(string? value, IValueFormatter? formatter = null)
        {
            if (value == null)
            {
                return;
            }

            if (formatter != null)
            {
                var emitAnsiReset = ShouldEmitAnsiReset(value);
                value = formatter.Format(value, emitAnsiReset);
            }

            WriteValueInternal(value);
        }

        public void WriteLine(string text) => _console.WriteLine(text);

        public void WriteErrorLine(string text) => _console.WriteErrorLine(text, SuppressErrorOutput);
        #endregion Public Methods

        #region Private Methods
        /// <summary>
        /// Writes the given value to the output stream of the connected console.
        /// Attempts to enable virtual terminal processing if option was enabled and output was not redirected.
        /// </summary>
        /// <param name="value">Value to write.</param>
        private void WriteValueInternal(string value)
        {
            if (EnableAnsiProcessing && !_console.IsOutputRedirected && !_consoleUtilities.TryEnableVirtualTerminalProcessing(out var error))
            {
                _console.WriteErrorLine($"Failed to enable virtual terminal processing. [{error}]", suppressOutput: SuppressErrorOutput);
            }

            _console.Write(value);
        }

        /// <summary>
        /// Gets the value stored in the given data package that is compatible with the specified type.
        /// </summary>
        /// <param name="content">Data package to get the value from.</param>
        /// <param name="type">Type of value to get.</param>
        /// <returns>The value, or null if the requested type is not supported.</returns>
        private async Task<string?> GetClipboardItemValueAsync(DataPackageView content, ContentType type = ContentType.Text)
        {
            if (type.HasFlag(ContentType.Text) && content.Contains(StandardDataFormats.Text))
            {
                return await content.GetTextAsync();
            }
            else if (type == ContentType.All)
            {
                var message = new StringBuilder();
                if (EnableAnsiProcessing)
                {
                    message.Append(Ansi.Color.Foreground.Red.EscapeSequence);
                }

                message.Append($"[Unsupported Format: {string.Join(',', content.AvailableFormats)}]");
                return message.ToString();
            }

            return null;
        }

        /// <summary>
        /// Determines whether ANSI reset should be emitted for the given value and current invocation, in the current environment.
        /// </summary>
        /// <remarks>
        /// When <see cref="AnsiResetType"/> is <see cref="AnsiResetType.Auto"/>, then this method will return true
        /// if all of the following conditions are also true:
        /// <list type="bullet">
        /// <item>ANSI processing is enabled</item>
        /// <item>Console output is not redirected</item>
        /// <item>COLORTERM is '24bit' or 'truecolor'</item>
        /// <item>The <paramref name="value"/> contains ANSI escape sequences</item>
        /// </list>
        /// </remarks>
        /// <param name="value">Value to include in the evaluation.</param>
        /// <returns>True if ANSI reset should be emitted, false otherwise.</returns>
        private bool ShouldEmitAnsiReset(string value)
        {
            switch (AnsiResetType)
            {
                case AnsiResetType.Auto:
                    bool shouldEmitAnsiReset = EnableAnsiProcessing;
                    if (!EnableAnsiProcessing)
                    {
                        if (_console.IsOutputRedirected)
                        {
                            // Don't emit ANSI reset if output was redirected and ANSI processing was not enabled
                            shouldEmitAnsiReset = false;
                        }
                        else
                        {
                            // Emit ANSI reset if the current terminal probably supports ANSI sequences based on TERM and COLORTERM, even if ANSI processing was not enabled
                            var termValue = Environment.GetEnvironmentVariable("TERM");
                            var colorTermValue = Environment.GetEnvironmentVariable("COLORTERM");
                            shouldEmitAnsiReset = termValue == "xterm-256color" || colorTermValue == "24bit" || colorTermValue == "truecolor";
                        }
                    }

                    if (shouldEmitAnsiReset && Regex.IsMatch(value, "\\e\\[[0-9;]*m"))
                    {
                        // Emit ANSI reset if the value contains ANSI escape sequences and either ANSI processing was enabled or the current terminal probably supports them based on TERM and COLORTERM
                        shouldEmitAnsiReset = true;
                    }

                    return shouldEmitAnsiReset;
                case AnsiResetType.On:
                    return true;
                case AnsiResetType.Off: // Fallthrough
                default: return false;
            }
        }
        #endregion Private Methods
    }
}
