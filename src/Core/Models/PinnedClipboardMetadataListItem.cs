using Newtonsoft.Json;

namespace past.Core.Models
{
    public class PinnedClipboardMetadataListItem
    {
        public static readonly string TimestampFormatString = "yyyy-MM-ddTHH:mm:ssZ";

        [JsonProperty("timestamp")]
        public DateTimeOffset Timestamp { get; set; }
        [JsonProperty("source")]
        public string Source { get; set; }
    }
}
