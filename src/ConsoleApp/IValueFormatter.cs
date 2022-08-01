using System;

namespace past.ConsoleApp
{
    public interface IValueFormatter
    {
        string Format(string value, bool emitAnsiReset, bool emitLineEnding = false);

        string Format(string value, int? index, string id, DateTimeOffset timestamp, bool emitAnsiReset, bool emitLineEnding = false);
    }
}
