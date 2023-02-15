using Newtonsoft.Json;
using past.Core.Models;
using past.Core.Wrappers;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Text.Json;
using Windows.ApplicationModel.DataTransfer;

namespace past.Core.Providers
{
    /// <inheritdoc cref="IPinnedClipboardItemProvider"/>
    public class PinnedClipboardItemProvider : IPinnedClipboardItemProvider
    {
        private readonly string _pinnedClipboardPath;

        #region Constructors
        /// <summary>
        /// Creates a new <see cref="PinnedClipboardItemProvider"/> with the system pinned clipboard metadata path.
        /// </summary>
        public PinnedClipboardItemProvider()
            // Pinned item IDs can be read from: %LOCALAPPDATA%/Microsoft/Windows/Clipboard/Pinned/{GUID}/metadata.json
            : this(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft/Windows/Clipboard/Pinned"))
        {
        }

        /// <summary>
        /// Creates a new <see cref="PinnedClipboardItemProvider"/> with the provided pinned clipboard metadata path.
        /// </summary>
        /// <remarks>
        /// This constructor is provided to support testing.
        /// </remarks>
        /// <param name="pinnedClipboardPath">Path to pinned clipboard metadata.</param>
        /// <exception cref="ArgumentNullException"><paramref name="pinnedClipboardPath"/> is null.</exception>
        public PinnedClipboardItemProvider(string pinnedClipboardPath)
        {
            _pinnedClipboardPath = pinnedClipboardPath ?? throw new ArgumentNullException(nameof(pinnedClipboardPath));
        }
        #endregion Constructors

        #region Public Methods
        public bool TryGetPinnedClipboardHistoryItemIds([NotNullWhen(true)] out HashSet<string>? pinnedItemIds, [NotNullWhen(false)] out string? errorMessage)
        {
            pinnedItemIds = null;
            errorMessage = null;
            if (!TryGetPinnedItemsMetadataPath(out var pinnedItemsMetadataPath, out errorMessage))
            {
                return false;
            }

            var pinnedClipboardItemMetadataJson = File.ReadAllText(pinnedItemsMetadataPath);
            var pinnedClipboardItemMetadata = JsonDocument.Parse(pinnedClipboardItemMetadataJson);
            var pinnedClipboardItems = pinnedClipboardItemMetadata.RootElement.GetProperty("items");
            if (pinnedClipboardItems.ValueKind == JsonValueKind.Null)
            {
                errorMessage = "No pinned items in clipboard history";
                return false;
            }

            var pinnedClipboardItemIds = pinnedClipboardItems.EnumerateObject().Select(property => property.Name);
            pinnedItemIds = pinnedClipboardItemIds.ToHashSet();
            return true;
        }

        public bool TryAddPinnedClipboardHistoryItem(IClipboardHistoryItemWrapper item, [NotNullWhen(false)] out string? errorMessage)
        {
            if (!TryGetPinnedItemsMetadataPath(out var pinnedItemsMetadataPath, out errorMessage))
            {
                return false;
            }

            var pinnedClipboardItemMetadataJson = File.ReadAllText(pinnedItemsMetadataPath, Encoding.Unicode);
            var pinnedItemsMetadata = JsonConvert.DeserializeObject<PinnedClipboardMetadataList>(pinnedClipboardItemMetadataJson);
            if (pinnedItemsMetadata?.Items == null)
            {
                errorMessage = "Failed to read pinned clipboard history items";
                return false;
            }

            if (pinnedItemsMetadata.Items.ContainsKey(item.Id))
            {
                errorMessage = $"Clipboard history item with ID {item.Id} is already pinned.";
                return false;
            }

            // Add new item to pin metadata
            pinnedItemsMetadata.Items[item.Id] = new PinnedClipboardMetadataListItem
            {
                Timestamp = item.Timestamp,
                Source = "Local"
            };

            // Write the updated manifest back to disk
            var newPinnedClipboardItemMetadataJson = JsonConvert.SerializeObject(pinnedItemsMetadata);
            File.WriteAllText(pinnedItemsMetadataPath, newPinnedClipboardItemMetadataJson, Encoding.Unicode);

            // Create directory for item (name is the item GUID)
            var pinnedItemPath = Path.Combine(Path.GetDirectoryName(pinnedItemsMetadataPath)!, item.Id);
            Directory.CreateDirectory(pinnedItemPath);

            // Create metadata file for item
            var pinnedItemMetadataPath = Path.Combine(pinnedItemPath, "metadata.json");
            var pinnedItemMetadata = JsonConvert.SerializeObject(PinnedItemMetadata.Text);
            File.WriteAllText(pinnedItemMetadataPath, pinnedItemMetadata, Encoding.Unicode);

            // Create content files for item (each file name is the base64 encoded name of item data type)
            if (item.Content.Contains(StandardDataFormats.Text))
            {
                var value = item.Content.GetTextAsync().GetResults();
                string encodedFormatId = Convert.ToBase64String(Encoding.ASCII.GetBytes(StandardDataFormats.Text));
                string itemContentPath = Path.Combine(pinnedItemPath, encodedFormatId);
                File.WriteAllText(itemContentPath, value, Encoding.Unicode);
            }
            else
            {
                errorMessage = $"Unsupported Format: {string.Join(',', item.Content.AvailableFormats)}";
                return false;
            }

            return true;
        }

        public bool TryRemovePinnedClipboardHistoryItem(IClipboardHistoryItemWrapper item, [NotNullWhen(false)] out string? errorMessage)
        {
            if (!TryGetPinnedItemsMetadataPath(out var pinnedItemsMetadataPath, out errorMessage))
            {
                return false;
            }

            var pinnedClipboardItemMetadataJson = File.ReadAllText(pinnedItemsMetadataPath, Encoding.Unicode);
            var pinnedItemsMetadata = JsonConvert.DeserializeObject<PinnedClipboardMetadataList>(pinnedClipboardItemMetadataJson);
            if (pinnedItemsMetadata?.Items == null)
            {
                errorMessage = $"Failed to read pinned clipboard history items";
                return false;
            }

            // Remove item from pin metadata
            if (!pinnedItemsMetadata.Items.Remove(item.Id))
            {
                errorMessage = $"Clipboard history item with ID {item.Id} is not pinned.";
                return false;
            }

            // Write the updated manifest back to disk
            var newPinnedClipboardItemMetadataJson = JsonConvert.SerializeObject(pinnedItemsMetadata);
            File.WriteAllText(pinnedItemsMetadataPath, newPinnedClipboardItemMetadataJson, Encoding.Unicode);

            // Remove the item directory
            var pinnedItemPath = Path.Combine(Path.GetDirectoryName(pinnedItemsMetadataPath)!, item.Id);
            if (!Directory.Exists(pinnedItemPath))
            {
                errorMessage = $"Pinned item {item.Id} not found on disk.";
                return false;
            }

            Directory.Delete(pinnedItemPath, true);

            return true;
        }
        #endregion Public Methods

        #region Helpers
        /// <summary>
        /// Gets the path to where pinned item metadata is stored.
        /// </summary>
        /// <param name="pinnedItemsMetadataPath">Path to the pinned items metadata if found, null otherwise.</param>
        /// <param name="errorMessage">Error message if getting the pinned items metadata path fails, null otherwise.</param>
        /// <returns>True if getting the pinned items metadata path succeeds, false otherwise.</returns>
        private bool TryGetPinnedItemsMetadataPath([NotNullWhen(true)] out string? pinnedItemsMetadataPath, [NotNullWhen(false)] out string? errorMessage)
        {
            pinnedItemsMetadataPath = null;
            errorMessage = null;
            if (!Directory.Exists(_pinnedClipboardPath))
            {
                errorMessage = $"Failed to retrieve pinned clipboard history items: Clipboard app data directory not found ({_pinnedClipboardPath})";
                return false;
            }

            foreach (var directoryPath in Directory.EnumerateDirectories(_pinnedClipboardPath))
            {
                var metadataPath = Path.Combine(directoryPath, "metadata.json");
                if (File.Exists(metadataPath))
                {
                    pinnedItemsMetadataPath = metadataPath;
                    return true;
                }
            }

            errorMessage = "Failed to retrieve pinned clipboard history items: Metadata file not found";
            return false;
        }
        #endregion Helpers
    }
}
