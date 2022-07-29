using Newtonsoft.Json;

namespace past.Core.Models
{
    public class PinnedClipboardMetadataList
    {
        [JsonProperty("items")]
        public IDictionary<string, PinnedClipboardMetadataListItem> Items { get; set; }
    }
}
