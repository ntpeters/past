using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace past
{
    public class ClipboardHistoryPinnedMetadata
    {
        public class ClipboardHistoryPinnedMetadataItem
        {
            [JsonProperty("timestamp")]
            public DateTimeOffset Timestamp { get; set; }
            [JsonProperty("source")]
            public string Source { get; set; }
        }

        [JsonProperty("items")]
        public IDictionary<string, ClipboardHistoryPinnedMetadataItem> Items { get; set; }
    }
}
