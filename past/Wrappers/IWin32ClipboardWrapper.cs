using System.Windows;

namespace past.Wrappers
{
    /// <inheritdoc cref="Clipboard"/>
    /// <remarks>
    /// This is a thin wrapper around <see cref="Clipboard"/> to support mocking.
    /// Only the methods used by this project are exposed through this wrapper.
    /// <br/><br/>
    /// NOTE: All methods exposed by this wrapper, as with the underlaying APIs they call, must be called from a thread running in Single Threaded Apartment (STA) mode.
    /// </remarks>
    public interface IWin32ClipboardWrapper
    {
        /// <inheritdoc cref="Clipboard.ContainsFileDropList"/>
        public bool ContainsFileDropList();
        /// <inheritdoc cref="Clipboard.ContainsImage"/>
        public bool ContainsImage();
        /// <inheritdoc cref="Clipboard.ContainsText(TextDataFormat)"/>
        public bool ContainsText(TextDataFormat format);
        /// <inheritdoc cref="Clipboard.GetDataObject"/>
        public IDataObject GetDataObject();
        /// <inheritdoc cref="Clipboard.GetText(TextDataFormat)"/>
        public string GetText(TextDataFormat format);
    }
}
