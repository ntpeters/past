using System;

namespace past.ConsoleApp
{
    public interface IValueFormatter
    {
        string Format(string value, int? index, string id, DateTimeOffset timestamp, bool emitAnsiReset);
    }
}
