using past.ConsoleApp.Extensions;
using past.ConsoleApp.Wrappers;
using past.Core;
using past.Core.Wrappers;
using System;
using System.CommandLine;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;

namespace past.ConsoleApp.Output
{
    /// <inheritdoc cref="IConsoleWriter"/>
    public class ConsoleWriter : IConsoleWriter
    {
        #region Public Properties
        public bool SuppressErrorOutput { get; }
        public bool EnableAnsiProcessing { get; }
        public AnsiResetType AnsiResetType { get; }
        #endregion Public Properties

        #region Private Fields
        private readonly IConsole _console;
        private readonly IEnvironmentWrapper _environment;
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
        public ConsoleWriter(IConsole console, IEnvironmentWrapper environment, bool suppressErrorOutput, bool enableAnsiProcessing, AnsiResetType ansiResetType)
        {
            _console = console ?? throw new ArgumentNullException(nameof(console));
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
            SuppressErrorOutput = suppressErrorOutput;
            EnableAnsiProcessing = enableAnsiProcessing;
            AnsiResetType = ansiResetType;
        }
        #endregion Constructors

        #region Public Methods
        public async Task WriteItemAsync(IClipboardHistoryItemWrapper item, ContentType type, IValueFormatter? formatter = null, bool emitLineEnding = false)
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
                value = formatter.Format(value, item.Index, item.Id, item.Timestamp, emitAnsiReset, emitLineEnding);
            }

            _console.Write(value);
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

            _console.Write(value);
        }

        public void WriteLine(string text) => _console.WriteLine(text);

        public void WriteErrorLine(string text) => _console.WriteErrorLine(text, SuppressErrorOutput);
        #endregion Public Methods

        #region Private Methods
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
            else if (type.HasFlag(ContentType.Image) && content.Contains(StandardDataFormats.Bitmap))
            {
                var message = new StringBuilder();
                if (EnableAnsiProcessing)
                {
                    message.Append(NativeConstants.ANSI_RED);
                }

                message.Append("[Unsupported Format: Image support coming soon]");
                return message.ToString();
            }
            else if (type == ContentType.All)
            {
                var message = new StringBuilder();
                if (EnableAnsiProcessing)
                {
                    message.Append(NativeConstants.ANSI_RED);
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
                    // Don't emit ANSI reset if the value doesn't contain ANSI escape sequences
                    if (!Regex.IsMatch(value, "\\e\\[[0-9;]*m"))
                    {
                        return false;
                    }

                    // Emit ANSI reset if the value contains ANSI escape sequences and ANSI processing was enabled
                    if (EnableAnsiProcessing)
                    {
                        return true;
                    }

                    // Don't emit ANSI reset if output was redirected and ANSI processing was not enabled
                    if (_console.IsOutputRedirected)
                    {
                        return false;
                    }

                    // Emit ANSI reset if  the value contains ANSI escape sequences and the current terminal probably
                    // supports ANSI sequences based on COLORTERM, even if ANSI processing was not enabled
                    var colorTermValue = _environment.GetEnvironmentVariable("COLORTERM");
                    return colorTermValue == "24bit" || colorTermValue == "truecolor";
                case AnsiResetType.On:
                    return true;
                case AnsiResetType.Off: // Fallthrough
                default: return false;
            }
        }
        #endregion Private Methods
    }
}
