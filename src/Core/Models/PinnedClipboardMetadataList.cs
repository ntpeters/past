using Newtonsoft.Json;

namespace past.Core.Models
{
    /// <summary>
    /// List of pinned clipboard history item metadata.
    /// </summary>
    public class PinnedClipboardMetadataList
    {
        /// <summary>
        /// Collection of pinned clipboard history metadata items, where the key is the ID
        /// of the associated clipboard history item.
        /// </summary>
        [JsonProperty("items")]
        public IDictionary<string, PinnedClipboardMetadataListItem> Items { get; set; } = new Dictionary<string, PinnedClipboardMetadataListItem>();
    }
}
