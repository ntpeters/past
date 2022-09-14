using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Json;

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
