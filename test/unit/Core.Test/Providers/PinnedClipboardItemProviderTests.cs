using Newtonsoft.Json;
using past.Core.Models;
using past.Core.Providers;
using System.Text;

namespace past.Core.Test.Providers
{
    public class PinnedClipboardItemProviderTests
    {
        #region Setup
        [TearDown]
        public void Teardown()
        {
            // Cleanup test pin directory after each test if one was created
            var basePinDir = GetTestPinnedItemBaseDirectory();
            if (Directory.Exists(basePinDir))
            {
                Directory.Delete(basePinDir, true);
            }
        }
        #endregion Setup

        #region Constructors
        [Test]
        public void Constructor_Parameterless_Success()
        {
            Assert.DoesNotThrow(() => new PinnedClipboardItemProvider());
        }

        [Test]
        public void Constructor_NonEmptyValue_Success()
        {
            Assert.DoesNotThrow(() => new PinnedClipboardItemProvider(GetTestPinnedItemBaseDirectory()));
        }

        [Test]
        public void Constructor_NullValue_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new PinnedClipboardItemProvider(null!));
        }
        #endregion Constructors

        #region TryGetPinnedClipboardHistoryItemIds
        [Test]
        public void TryGetPinnedClipboardHistoryItemIds_PinnedItemsExist_ReturnsExpectedItemIds()
        {
            // Arrange
            var expectedItemIds = new HashSet<string>();
            var pinnedMetadataList = new PinnedClipboardMetadataList();
            pinnedMetadataList.Items = new Dictionary<string, PinnedClipboardMetadataListItem>();
            for (int i = 0; i < 3; i++)
            {
                var pinnedMetadataListItem = new PinnedClipboardMetadataListItem
                {
                    Timestamp = DateTimeOffset.Now,
                    Source = "Local"
                };

                var itemId = GetNewFormattedGuidString();
                expectedItemIds.Add(itemId);
                pinnedMetadataList.Items.Add(itemId, pinnedMetadataListItem);
            }

            CreateTestPinnedItemMetadataFile(pinnedMetadataList);

            var pinnedItemProvider = new PinnedClipboardItemProvider(GetTestPinnedItemBaseDirectory());

            // Act
            var result = pinnedItemProvider.TryGetPinnedClipboardHistoryItemIds(out var actualItemIds, out var errorMessage);

            // Assert
            Assert.That(result, Is.True);
            Assert.That(actualItemIds, Is.EqualTo(expectedItemIds));
            Assert.That(errorMessage, Is.Null);
        }

        [Test]
        public void TryGetPinnedClipboardHistoryItemIds_EmptyPinnedItemJsonList_ReturnsTrueAndEmptySet()
        {
            // Arrange
            var pinnedMetadataList = new PinnedClipboardMetadataList();
            pinnedMetadataList.Items = new Dictionary<string, PinnedClipboardMetadataListItem>();
            CreateTestPinnedItemMetadataFile(pinnedMetadataList);

            var pinnedItemProvider = new PinnedClipboardItemProvider(GetTestPinnedItemBaseDirectory());

            // Act
            var result = pinnedItemProvider.TryGetPinnedClipboardHistoryItemIds(out var itemIds, out var actualErrorMessage);

            // Assert
            Assert.That(result, Is.True);
            Assert.That(itemIds, Is.Empty);
            Assert.That(actualErrorMessage, Is.Null);
        }

        [Test]
        public void TryGetPinnedClipboardHistoryItemIds_NullPinnedItemJsonList_ReturnsFalseAndExpectedError()
        {
            // Arrange
            var expectedErrorMessage = "No pinned items in clipboard history";
            var pinnedMetadataList = new PinnedClipboardMetadataList();
            pinnedMetadataList.Items = null!;
            CreateTestPinnedItemMetadataFile(pinnedMetadataList);

            var pinnedItemProvider = new PinnedClipboardItemProvider(GetTestPinnedItemBaseDirectory());

            // Act
            var result = pinnedItemProvider.TryGetPinnedClipboardHistoryItemIds(out var itemIds, out var actualErrorMessage);

            // Assert
            Assert.That(result, Is.False);
            Assert.That(itemIds, Is.Null);
            Assert.That(actualErrorMessage, Is.EqualTo(expectedErrorMessage));
        }

        [Test]
        public void TryGetPinnedClipboardHistoryItemIds_BasePinDirectoryNotExist_ReturnsFalseAndExpectedError()
        {
            // Arrange
            var basePinDir = GetTestPinnedItemBaseDirectory();
            var expectedErrorMessage = $"Failed to retrieve pinned clipboard history items: Clipboard app data directory not found ({basePinDir})";
            var pinnedItemProvider = new PinnedClipboardItemProvider(basePinDir);

            // Act
            var result = pinnedItemProvider.TryGetPinnedClipboardHistoryItemIds(out var itemIds, out var actualErrorMessage);

            // Assert
            Assert.That(result, Is.False);
            Assert.That(itemIds, Is.Null);
            Assert.That(actualErrorMessage, Is.EqualTo(expectedErrorMessage));
        }

        [Test]
        public void TryGetPinnedClipboardHistoryItemIds_PinGuidDirectoryNotExist_ReturnsFalseAndExpectedError()
        {
            // Arrange
            var expectedErrorMessage = "Failed to retrieve pinned clipboard history items: Metadata file not found";
            var basePinDir = GetTestPinnedItemBaseDirectory();
            Directory.CreateDirectory(basePinDir);
            var pinnedItemProvider = new PinnedClipboardItemProvider(basePinDir);

            // Act
            var result = pinnedItemProvider.TryGetPinnedClipboardHistoryItemIds(out var itemIds, out var actualErrorMessage);

            // Assert
            Assert.That(result, Is.False);
            Assert.That(itemIds, Is.Null);
            Assert.That(actualErrorMessage, Is.EqualTo(expectedErrorMessage));
        }

        [Test]
        public void TryGetPinnedClipboardHistoryItemIds_PinMetadataFileNotExist_ReturnsFalseAndExpectedError()
        {
            // Arrange
            var expectedErrorMessage = "Failed to retrieve pinned clipboard history items: Metadata file not found";
            var basePinDir = GetTestPinnedItemBaseDirectory();
            var pinGuidDir = GetNewTestPinnedGuidDirectory();
            Directory.CreateDirectory(pinGuidDir);
            var pinnedItemProvider = new PinnedClipboardItemProvider(basePinDir);

            // Act
            var result = pinnedItemProvider.TryGetPinnedClipboardHistoryItemIds(out var itemIds, out var actualErrorMessage);

            // Assert
            Assert.That(result, Is.False);
            Assert.That(itemIds, Is.Null);
            Assert.That(actualErrorMessage, Is.EqualTo(expectedErrorMessage));
        }
        #endregion TryGetPinnedClipboardHistoryItemIds

        #region Helpers
        private string GetNewFormattedGuidString() => $"{{{Guid.NewGuid().ToString().ToUpper()}}}";

        private string GetTestPinnedItemBaseDirectory()
        {
            var testDir = TestContext.CurrentContext.TestDirectory;
            Assert.That(Directory.Exists(testDir), Is.True);
            return Path.Combine(testDir, "Pinned");
        }

        private string GetNewTestPinnedGuidDirectory()
        {
            var basePinDir = GetTestPinnedItemBaseDirectory();
            var pinDirId = GetNewFormattedGuidString();
            return Path.Combine(basePinDir, pinDirId);
        }

        private string GetNewTestPinnedMetadataFilePath(string? basePath = null)
        {
            basePath ??= GetNewTestPinnedGuidDirectory();
            return Path.Combine(basePath, "metadata.json");
        }

        private void CreateTestPinnedItemMetadataFile(PinnedClipboardMetadataList pinnedMetadataList)
        {
            var newPinDir = GetNewTestPinnedGuidDirectory();
            Directory.CreateDirectory(newPinDir);

            var pinnedMetadataFilePath = GetNewTestPinnedMetadataFilePath(newPinDir);
            var pinnedMetadataListJson = JsonConvert.SerializeObject(pinnedMetadataList, new JsonSerializerSettings
            {
                DateFormatString = PinnedClipboardMetadataListItem.TimestampFormatString
            });
            File.WriteAllText(pinnedMetadataFilePath, pinnedMetadataListJson, Encoding.Unicode);
        }
        #endregion Helpers
    }
}
