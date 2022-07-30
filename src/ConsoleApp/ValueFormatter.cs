using System;
using System.CommandLine.Rendering;
using System.Text;

namespace past.ConsoleApp
{
    public class ValueFormatter : IValueFormatter
    {
        private readonly bool _nullLineEnding;
        private readonly bool _includeIndex;
        private readonly bool _includeId;
        private readonly bool _includeTimestamp;

        public ValueFormatter(bool nullLineEnding, bool includeIndex, bool includeId, bool includeTimestamp)
        {
            _nullLineEnding = nullLineEnding;
            _includeIndex = includeIndex;
            _includeId = includeId;
            _includeTimestamp = includeTimestamp;
        }

        public string Format(string value, int? index, string id, DateTimeOffset timestamp, bool emitAnsiReset )
        {
            var outputValue = new StringBuilder();
            if (_includeIndex && index != null)
            {
                outputValue.Append($"{index}:");
            }

            if (_includeId)
            {
                outputValue.Append($"{id}:");
            }

            if (_includeTimestamp)
            {
                outputValue.Append($"{timestamp}:");
            }

            outputValue.Append(value);

            if (emitAnsiReset)
            {
                outputValue.Append(Ansi.Text.AttributesOff.EscapeSequence);
            }

            if (_nullLineEnding)
            {
                outputValue.Append('\0');
            }
            else
            {
                outputValue.Append('\n');
            }

            return outputValue.ToString();
        }
    }
}
