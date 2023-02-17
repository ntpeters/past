using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;

namespace past.Core.Models
{
    /// <summary>
    /// Metadata describing the format of a pinned clipboard history item.
    /// </summary>
    public class PinnedItemFormatMetadata
    {
        /// <summary>
        /// The underlaying data type of the pinned item.
        /// </summary>
        [JsonProperty("dataType")]
        public string DataType { get; set; } = string.Empty;

        /// <summary>
        /// The collection type of the pinned item.
        /// </summary>
        [JsonProperty("collectionType")]
        public string CollectionType { get; [ExcludeFromCodeCoverage] set; } = "None";

        /// <summary>
        /// Whether the pinned item content is encrypted on disk.
        /// </summary>
        [JsonProperty("isEncrypted")]
        public bool IsEncrypted { get; [ExcludeFromCodeCoverage] set; } = false;
    }
}
