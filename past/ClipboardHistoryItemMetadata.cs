using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using Windows.ApplicationModel.DataTransfer;

namespace past
{
    public class ClipboardHistoryItemMetadata
    {
        public enum PinnedStatus
        {
            Unknown,
            Pinned,
            Unpinned
        }

        public Guid Id { get; set; }
        public int Index { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public PinnedStatus PinStatus { get; set; }
        public IReadOnlyList<string> AvailableFormats { get; set; }

        public ClipboardHistoryItemMetadata(ClipboardHistoryItem item, int index)
        {
            Id = Guid.Parse(item.Id);
            Index = index;
            Timestamp = item.Timestamp;
            AvailableFormats = item.Content.AvailableFormats;
            PinStatus = PinnedStatus.Unknown;
        }
    }
}
