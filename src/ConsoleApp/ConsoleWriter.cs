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
    public class ConsoleWriter : IConsoleWriter
    {
        private readonly IConsole _console;
        private readonly bool _suppressErrorOutput;
        private readonly bool _enableAnsiProcessing;
        private readonly AnsiResetType _ansiResetType;

        public ConsoleWriter(IConsole console, bool suppressErrorOutput, bool enableAnsiProcessing, AnsiResetType ansiResetType)
        {
            _console = console ?? throw new ArgumentNullException(nameof(console));
            _suppressErrorOutput = suppressErrorOutput;
            _enableAnsiProcessing = enableAnsiProcessing;
            _ansiResetType = ansiResetType;
        }

        public async Task WriteItemAsync(IClipboardHistoryItemWrapper item, ContentType type, int? index = null, IValueFormatter? formatter = null, bool emitLineEnding = false)
        {
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

            WriteInternal(value);
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

            WriteInternal(value);
        }

        public void WriteLine(string text) => _console.WriteLine(text);

        public void WriteErrorLine(string text) => _console.WriteErrorLine(text, _suppressErrorOutput);

        private void WriteInternal(string value)
        {
            if (_enableAnsiProcessing && !_console.IsOutputRedirected && !ConsoleHelpers.TryEnableVirtualTerminalProcessing(out var error))
            {
                _console.WriteErrorLine($"Failed to enable virtual terminal processing. [{error}]", suppressOutput: _suppressErrorOutput);
            }

            _console.Write(value);
        }

        private async Task<string?> GetClipboardItemValueAsync(DataPackageView content, ContentType type = ContentType.Text)
        {
            if (type.HasFlag(ContentType.Text) && content.Contains(StandardDataFormats.Text))
            {
                return await content.GetTextAsync();
            }
            else if (type == ContentType.All)
            {
                var message = new StringBuilder();
                if (_enableAnsiProcessing)
                {
                    message.Append(Ansi.Color.Foreground.Red.EscapeSequence);
                }

                message.Append($"[Unsupported Format: {string.Join(',', content.AvailableFormats)}]");
                return message.ToString();
            }

            return null;
        }

        private bool ShouldEmitAnsiReset(string value)
        {
            switch (_ansiResetType)
            {
                case AnsiResetType.Auto:
                    bool shouldEmitAnsiReset = _enableAnsiProcessing;
                    if (!_enableAnsiProcessing)
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
    }
}
