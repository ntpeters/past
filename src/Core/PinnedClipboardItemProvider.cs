using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Json;

namespace past.Core
{
    public class PinnedClipboardItemProvider : IPinnedClipboardItemProvider
    {
        private readonly string _pinnedClipboardPath;

        #region Constructors
        // Pinned item IDs can be read from: %LOCALAPPDATA%/Microsoft/Windows/Clipboard/Pinned/{GUID}/metadata.json
        public PinnedClipboardItemProvider()
            : this(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft/Windows/Clipboard/Pinned"))
        {
        }

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
            var pinnedClipboardItemIds = pinnedClipboardItemMetadata.RootElement.GetProperty("items").EnumerateObject().Select(property => property.Name);
            if (pinnedClipboardItemIds == null || !pinnedClipboardItemIds.Any())
            {
                errorMessage = "No pinned items in clipboard history";
                return false;
            }

            pinnedItemIds = pinnedClipboardItemIds.ToHashSet();
            return true;
        }
        #endregion Public Methods

        #region Helpers
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
