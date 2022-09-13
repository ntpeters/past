using System.Diagnostics.CodeAnalysis;
using System.Windows;
using Win32Clipboard = System.Windows.Clipboard;

namespace past.Core.Wrappers
{
    /// <inheritdoc cref="IWin32ClipboardWrapper"/>
    [ExcludeFromCodeCoverage(Justification = "Wrappers are not intended to be tested, as they exist solely to enable dependency injection of non-mockable APIs.")]
    public class Win32ClipboardWrapper : IWin32ClipboardWrapper
    {
        public bool ContainsFileDropList() => Win32Clipboard.ContainsFileDropList();

        public bool ContainsImage() => Win32Clipboard.ContainsImage();

        public bool ContainsText(TextDataFormat format) => Win32Clipboard.ContainsText(format);

        public IDataObject GetDataObject() => Win32Clipboard.GetDataObject();

        public string GetText(TextDataFormat format) => Win32Clipboard.GetText(format);
    }
}
