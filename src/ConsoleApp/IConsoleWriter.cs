using past.Core;
using past.Core.Wrappers;
using System.Threading.Tasks;

namespace past.ConsoleApp
{
    public interface IConsoleWriter
    {
        Task WriteItemAsync(IClipboardHistoryItemWrapper item, ContentType type, int? index = null, IValueFormatter? formatter = null);
        void WriteValue(string? value);
        void WriteLine(string text);
        void WriteErrorLine(string text);
    }
}
