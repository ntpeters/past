using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Diagnostics.CodeAnalysis;
using Windows.ApplicationModel.DataTransfer;

namespace past.Core.Models
{
    /// <summary>
    /// Metadata describing a pinned clipboard history item.
    /// </summary>
    public class PinnedItemMetadata
    {
        /// <summary>
        /// Metadata for a pinned item with contents of the format <see cref="StandardDataFormats.Text"/>.
        /// </summary>
        public static PinnedItemMetadata Text
        {
            get
            {
                var metadata = new PinnedItemMetadata();
                metadata.FormatMetadata[StandardDataFormats.Text] = new PinnedItemFormatMetadata
                {
                    DataType = "String"
                };
                return metadata;
            }
        }

        /// <summary>
        /// Metadata describing all of the data formats the pinned item contains.
        /// </summary>
        [JsonProperty("formatMetadata")]
        public IDictionary<string, PinnedItemFormatMetadata> FormatMetadata { get; [ExcludeFromCodeCoverage] set; } = new Dictionary<string, PinnedItemFormatMetadata>();

        /// <summary>
        /// ID of the source app the clipboard item was copied from.
        /// </summary>
        /// <remarks>
        /// This does not seem to be usually set by the system.
        /// </remarks>
        [JsonProperty("sourceAppId")]
        public string SourceAppId { get; [ExcludeFromCodeCoverage] set; } = string.Empty;

        /// <summary>
        /// Properties associated with the item.
        /// </summary>
        /// <remarks>
        /// It is unclear whether this is actually used, or what the value type is.
        /// </remarks>
        [JsonProperty("property")]
        public IDictionary<string, object> Property { get; [ExcludeFromCodeCoverage] set; } = new Dictionary<string, object>();
    }
}
