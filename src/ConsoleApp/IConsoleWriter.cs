using past.Core;
using past.Core.Wrappers;
using System.Threading.Tasks;

namespace past.ConsoleApp
{
    public interface IConsoleWriter
    {
        Task WriteItemAsync(IClipboardHistoryItemWrapper item, ContentType type, int? index = null, IValueFormatter? formatter = null, bool emitLineEnding = false);
        void WriteValue(string? value, IValueFormatter? formatter = null);
        void WriteLine(string text);
        void WriteErrorLine(string text);
    }
}
