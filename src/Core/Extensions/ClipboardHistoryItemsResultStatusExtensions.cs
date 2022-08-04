using Windows.ApplicationModel.DataTransfer;

namespace past.Core.Extensions
{
    /// <summary>
    /// Extensions for <see cref="ClipboardHistoryItemsResultStatus"/>.
    /// </summary>
    public static class ClipboardHistoryItemsResultStatusExtensions
    {
        /// <summary>
        /// Maps a <see cref="ClipboardHistoryItemsResultStatus"/> to an <see cref="ErrorCode"/>.
        /// </summary>
        /// <param name="resultStatus">The <see cref="ClipboardHistoryItemsResultStatus"/> to convert.</param>
        /// <returns>The <see cref="ErrorCode"/> representing the <see cref="ClipboardHistoryItemsResultStatus"/>.</returns>
        public static ErrorCode ToErrorCode(this ClipboardHistoryItemsResultStatus resultStatus) =>
            resultStatus switch
            {
                ClipboardHistoryItemsResultStatus.AccessDenied => ErrorCode.AccessDenied,
                ClipboardHistoryItemsResultStatus.ClipboardHistoryDisabled => ErrorCode.ClipboardHistoryDisabled,
                ClipboardHistoryItemsResultStatus.Success => ErrorCode.Success,
                _ => ErrorCode.Success
            };
    }
}
