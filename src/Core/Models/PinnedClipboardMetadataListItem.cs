using Newtonsoft.Json;

namespace past.Core.Models
{
    /// <summary>
    /// Metadata of a pinned clipboard history item.
    /// </summary>
    public class PinnedClipboardMetadataListItem
    {
        /// <summary>
        /// Format of the timestamp for pinned clipboard history items for serialization.
        /// </summary>
        public static readonly string TimestampFormatString = "yyyy-MM-ddTHH:mm:ssZ";

        /// <summary>
        /// Timestamp of when the item was added to the clipboard history.
        /// </summary>
        [JsonProperty("timestamp")]
        public DateTimeOffset Timestamp { get; set; }

        /// <summary>
        /// The source of the associated clipboard history item content.
        /// </summary>
        /// <remarks>
        /// This value has only been observed as being 'Local'.
        /// </remarks>
        [JsonProperty("source")]
        public string Source { get; set; }
    }
}
