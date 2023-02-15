using Moq;
using Newtonsoft.Json;
using past.Core.Models;
using past.Core.Providers;
using past.Core.Wrappers;
using System.Text;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage.Streams;
using Windows.Storage;

namespace past.Core.Test.Providers
{
    public class PinnedClipboardItemProviderTests
    {
        #region Setup
        private string _pinnedItemsDirectory;

        [SetUp] public void SetUp()
        {
            _pinnedItemsDirectory = GetNewTestPinnedGuidDirectory();

            var basePinDir = GetTestPinnedItemBaseDirectory();
            if (Directory.Exists(basePinDir))
            {
                Assert.Fail("Test pinned item base directory should not exist prior to test execution.");
            }
        }

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

        #region TryAddPinnedClipboardHistoryItem
        [Test]
        public void TryAddPinnedClipboardHistoryItem_Success()
        {
            // Arrange
            var expectedItemIds = new HashSet<string>();
            var pinnedMetadataList = new PinnedClipboardMetadataList();
            pinnedMetadataList.Items = new Dictionary<string, PinnedClipboardMetadataListItem>();

            CreateTestPinnedItemMetadataFile(pinnedMetadataList);

            var pinnedItemProvider = new PinnedClipboardItemProvider(GetTestPinnedItemBaseDirectory());

            var mockClipboardItem = new Mock<IClipboardHistoryItemWrapper>(MockBehavior.Strict);

            var expectedItemId = GetNewFormattedGuidString();
            mockClipboardItem
                .SetupGet(mock => mock.Id)
                .Returns(expectedItemId);

            mockClipboardItem
                .SetupGet(mock => mock.Timestamp)
                .Returns(DateTimeOffset.Now);

            var expectedContent = "Item 0";

            var dataPackage = new DataPackage();
            dataPackage.SetText(expectedContent);
            mockClipboardItem
                .SetupGet(mock => mock.Content)
                .Returns(dataPackage.GetView());

            var expectedItemMetadata = "{\"formatMetadata\":{\"Text\":{\"isEncrypted\":false,\"dataType\":\"String\",\"collectionType\":\"None\"}},\"sourceAppId\":\"\",\"property\":{}}";

            // Act
            var result = pinnedItemProvider.TryAddPinnedClipboardHistoryItem(mockClipboardItem.Object, out var errorMessage);

            // Assert
            Assert.That(result, Is.True);
            Assert.That(errorMessage, Is.Null);

            var result2 = pinnedItemProvider.TryGetPinnedClipboardHistoryItemIds(out var itemIds, out var error2);
            Assert.That(result2, Is.True);
            Assert.That(error2, Is.Null);
            Assert.That(itemIds, Has.Exactly(1).Items);
            Assert.That(itemIds, Contains.Item(expectedItemId));

            var pinnedItemPath = Path.Combine(_pinnedItemsDirectory, expectedItemId);

            var pinnedItemMetadataPath = Path.Combine(pinnedItemPath, "metadata.json");
            var actualItemMetadata = File.ReadAllText(pinnedItemMetadataPath);
            Assert.That(actualItemMetadata, Is.EqualTo(expectedItemMetadata));

            string encodedFormatId = Convert.ToBase64String(Encoding.ASCII.GetBytes(StandardDataFormats.Text));
            string itemContentPath = Path.Combine(pinnedItemPath, encodedFormatId);
            var actualContent = File.ReadAllText(itemContentPath);
            Assert.That(actualContent, Is.EqualTo(expectedContent));
        }

        [Test]
        public void TryAddPinnedClipboardHistoryItem_UnsupportedType_ReturnsFalseAndExpectedError()
        {
            // Arrange
            var expectedItemIds = new HashSet<string>();
            var pinnedMetadataList = new PinnedClipboardMetadataList();
            pinnedMetadataList.Items = new Dictionary<string, PinnedClipboardMetadataListItem>();

            CreateTestPinnedItemMetadataFile(pinnedMetadataList);

            var pinnedItemProvider = new PinnedClipboardItemProvider(GetTestPinnedItemBaseDirectory());

            var mockClipboardItem = new Mock<IClipboardHistoryItemWrapper>(MockBehavior.Strict);

            var expectedItemId = GetNewFormattedGuidString();
            mockClipboardItem
                .SetupGet(mock => mock.Id)
                .Returns(expectedItemId);

            mockClipboardItem
                .SetupGet(mock => mock.Timestamp)
                .Returns(DateTimeOffset.Now);

            var mockFile = new Mock<IStorageFile>();
            var imageStream = RandomAccessStreamReference.CreateFromFile(mockFile.Object);
            var dataPackage = new DataPackage();
            dataPackage.SetBitmap(imageStream);
            mockClipboardItem
                .SetupGet(mock => mock.Content)
                .Returns(dataPackage.GetView());

            var expectedErrorMessage = $"Unsupported Format: {string.Join(',', mockClipboardItem.Object.Content.AvailableFormats)}";

            // Act
            var result = pinnedItemProvider.TryAddPinnedClipboardHistoryItem(mockClipboardItem.Object, out var errorMessage);

            // Assert
            Assert.That(result, Is.False);
            Assert.That(errorMessage, Is.EqualTo(expectedErrorMessage));
        }

        [Test]
        public void TryAddPinnedClipboardHistoryItem_NullPinnedItemJsonList_ReturnsFalseAndExpectedError()
        {
            // Arrange
            var expectedErrorMessage = "Failed to read pinned clipboard history items";
            var pinnedMetadataList = new PinnedClipboardMetadataList();
            pinnedMetadataList.Items = null!;
            CreateTestPinnedItemMetadataFile(pinnedMetadataList);

            var mockClipboardItem = new Mock<IClipboardHistoryItemWrapper>(MockBehavior.Strict);

            var expectedItemId = GetNewFormattedGuidString();
            mockClipboardItem
                .SetupGet(mock => mock.Id)
                .Returns(expectedItemId);

            var pinnedItemProvider = new PinnedClipboardItemProvider(GetTestPinnedItemBaseDirectory());

            // Act
            var result = pinnedItemProvider.TryAddPinnedClipboardHistoryItem(mockClipboardItem.Object, out var actualErrorMessage);

            // Assert
            Assert.That(result, Is.False);
            Assert.That(actualErrorMessage, Is.EqualTo(expectedErrorMessage));
        }

        [Test]
        public void TryAddPinnedClipboardHistoryItem_InvalidPinnedItemMetadata_ReturnsFalseAndExpectedError()
        {
            // Arrange
            var expectedErrorMessage = "Failed to read pinned clipboard history items";
            CreateEmptyTestPinnedItemMetadataFile();

            var mockClipboardItem = new Mock<IClipboardHistoryItemWrapper>(MockBehavior.Strict);

            var expectedItemId = GetNewFormattedGuidString();
            mockClipboardItem
                .SetupGet(mock => mock.Id)
                .Returns(expectedItemId);

            var pinnedItemProvider = new PinnedClipboardItemProvider(GetTestPinnedItemBaseDirectory());

            // Act
            var result = pinnedItemProvider.TryAddPinnedClipboardHistoryItem(mockClipboardItem.Object, out var actualErrorMessage);

            // Assert
            Assert.That(result, Is.False);
            Assert.That(actualErrorMessage, Is.EqualTo(expectedErrorMessage));
        }

        [Test]
        public void TryAddPinnedClipboardHistoryItem_BasePinDirectoryNotExist_ReturnsFalseAndExpectedError()
        {
            // Arrange
            var basePinDir = GetTestPinnedItemBaseDirectory();
            var expectedErrorMessage = $"Failed to retrieve pinned clipboard history items: Clipboard app data directory not found ({basePinDir})";

            var mockClipboardItem = new Mock<IClipboardHistoryItemWrapper>(MockBehavior.Strict);

            var pinnedItemProvider = new PinnedClipboardItemProvider(basePinDir);

            // Act
            var result = pinnedItemProvider.TryAddPinnedClipboardHistoryItem(mockClipboardItem.Object, out var actualErrorMessage);

            // Assert
            Assert.That(result, Is.False);
            Assert.That(actualErrorMessage, Is.EqualTo(expectedErrorMessage));
        }

        [Test]
        public void TryAddPinnedClipboardHistoryItem_PinGuidDirectoryNotExist_ReturnsFalseAndExpectedError()
        {
            // Arrange
            var expectedErrorMessage = "Failed to retrieve pinned clipboard history items: Metadata file not found";

            var mockClipboardItem = new Mock<IClipboardHistoryItemWrapper>(MockBehavior.Strict);

            var basePinDir = GetTestPinnedItemBaseDirectory();
            Directory.CreateDirectory(basePinDir);
            var pinnedItemProvider = new PinnedClipboardItemProvider(basePinDir);

            // Act
            var result = pinnedItemProvider.TryAddPinnedClipboardHistoryItem(mockClipboardItem.Object, out var actualErrorMessage);

            // Assert
            Assert.That(result, Is.False);
            Assert.That(actualErrorMessage, Is.EqualTo(expectedErrorMessage));
        }

        [Test]
        public void TryAddPinnedClipboardHistoryItem_PinMetadataFileNotExist_ReturnsFalseAndExpectedError()
        {
            // Arrange
            var expectedErrorMessage = "Failed to retrieve pinned clipboard history items: Metadata file not found";

            var mockClipboardItem = new Mock<IClipboardHistoryItemWrapper>(MockBehavior.Strict);

            var basePinDir = GetTestPinnedItemBaseDirectory();
            var pinGuidDir = GetNewTestPinnedGuidDirectory();
            Directory.CreateDirectory(pinGuidDir);
            var pinnedItemProvider = new PinnedClipboardItemProvider(basePinDir);

            // Act
            var result = pinnedItemProvider.TryAddPinnedClipboardHistoryItem(mockClipboardItem.Object, out var actualErrorMessage);

            // Assert
            Assert.That(result, Is.False);
            Assert.That(actualErrorMessage, Is.EqualTo(expectedErrorMessage));
        }

        [Test]
        public void TryAddPinnedClipboardHistoryItem_ItemAlreadyPinned_ReturnsFalseAndExpectedError()
        {
            // Arrange
            var expectedItemId = GetNewFormattedGuidString();
            var expectedErrorMessage = $"Clipboard history item with ID {expectedItemId} is already pinned.";

            var pinnedMetadataList = new PinnedClipboardMetadataList();
            pinnedMetadataList.Items[expectedItemId] = new PinnedClipboardMetadataListItem();
            CreateTestPinnedItemMetadataFile(pinnedMetadataList);

            var mockClipboardItem = new Mock<IClipboardHistoryItemWrapper>(MockBehavior.Strict);

            mockClipboardItem
                .SetupGet(mock => mock.Id)
                .Returns(expectedItemId);

            var pinnedItemProvider = new PinnedClipboardItemProvider(GetTestPinnedItemBaseDirectory());

            // Act
            var result = pinnedItemProvider.TryAddPinnedClipboardHistoryItem(mockClipboardItem.Object, out var actualErrorMessage);

            // Assert
            Assert.That(result, Is.False);
            Assert.That(actualErrorMessage, Is.EqualTo(expectedErrorMessage));
        }
        #endregion TryAddPinnedClipboardHistoryItem

        #region TryRemovePinnedClipboardHistoryItem
        [Test]
        public void TryRemovePinnedClipboardHistoryItem_Success()
        {
            // Arrange
            var expectedItemIds = new HashSet<string>();
            var pinnedMetadataList = new PinnedClipboardMetadataList();
            pinnedMetadataList.Items = new Dictionary<string, PinnedClipboardMetadataListItem>();

            var pinnedMetadataListItem = new PinnedClipboardMetadataListItem
            {
                Timestamp = DateTimeOffset.Now,
                Source = "Local"
            };

            var expectedItemId = GetNewFormattedGuidString();
            expectedItemIds.Add(expectedItemId);
            pinnedMetadataList.Items.Add(expectedItemId, pinnedMetadataListItem);

            CreateTestPinnedItemMetadataFile(pinnedMetadataList);

            var pinnedItemProvider = new PinnedClipboardItemProvider(GetTestPinnedItemBaseDirectory());

            var mockClipboardItem = new Mock<IClipboardHistoryItemWrapper>(MockBehavior.Strict);
            mockClipboardItem
                .SetupGet(mock => mock.Id)
                .Returns(expectedItemId);

            var expectedContent = "Item 0";
            var dataPackage = new DataPackage();
            dataPackage.SetText(expectedContent);
            mockClipboardItem
                .SetupGet(mock => mock.Content)
                .Returns(dataPackage.GetView());

            CreateTestPinnedItemDirectory(mockClipboardItem.Object);

            // Act
            var result = pinnedItemProvider.TryRemovePinnedClipboardHistoryItem(mockClipboardItem.Object, out var errorMessage);

            // Assert
            Assert.That(result, Is.True);
            Assert.That(errorMessage, Is.Null);

            var result2 = pinnedItemProvider.TryGetPinnedClipboardHistoryItemIds(out var actualItemIds, out var error2);
            Assert.That(result2, Is.True);
            Assert.That(error2, Is.Null);
            Assert.That(actualItemIds, Is.Empty);

            var pinnedItemPath = Path.Combine(_pinnedItemsDirectory, expectedItemId);
            Assert.That(pinnedItemPath, Does.Not.Exist);
        }

        [Test]
        public void TryRemovePinnedClipboardHistoryItem_NullPinnedItemJsonList_ReturnsFalseAndExpectedError()
        {
            // Arrange
            var expectedErrorMessage = "Failed to read pinned clipboard history items";
            var pinnedMetadataList = new PinnedClipboardMetadataList();
            pinnedMetadataList.Items = null!;
            CreateTestPinnedItemMetadataFile(pinnedMetadataList);

            var mockClipboardItem = new Mock<IClipboardHistoryItemWrapper>(MockBehavior.Strict);

            var expectedItemId = GetNewFormattedGuidString();
            mockClipboardItem
                .SetupGet(mock => mock.Id)
                .Returns(expectedItemId);

            var pinnedItemProvider = new PinnedClipboardItemProvider(GetTestPinnedItemBaseDirectory());

            // Act
            var result = pinnedItemProvider.TryRemovePinnedClipboardHistoryItem(mockClipboardItem.Object, out var actualErrorMessage);

            // Assert
            Assert.That(result, Is.False);
            Assert.That(actualErrorMessage, Is.EqualTo(expectedErrorMessage));
        }

        [Test]
        public void TryRemovePinnedClipboardHistoryItem_InvalidPinnedItemMetadata_ReturnsFalseAndExpectedError()
        {
            // Arrange
            var expectedErrorMessage = "Failed to read pinned clipboard history items";
            CreateEmptyTestPinnedItemMetadataFile();

            var mockClipboardItem = new Mock<IClipboardHistoryItemWrapper>(MockBehavior.Strict);

            var expectedItemId = GetNewFormattedGuidString();
            mockClipboardItem
                .SetupGet(mock => mock.Id)
                .Returns(expectedItemId);

            var pinnedItemProvider = new PinnedClipboardItemProvider(GetTestPinnedItemBaseDirectory());

            // Act
            var result = pinnedItemProvider.TryRemovePinnedClipboardHistoryItem(mockClipboardItem.Object, out var actualErrorMessage);

            // Assert
            Assert.That(result, Is.False);
            Assert.That(actualErrorMessage, Is.EqualTo(expectedErrorMessage));
        }

        [Test]
        public void TryRemovePinnedClipboardHistoryItem_BasePinDirectoryNotExist_ReturnsFalseAndExpectedError()
        {
            // Arrange
            var basePinDir = GetTestPinnedItemBaseDirectory();
            var expectedErrorMessage = $"Failed to retrieve pinned clipboard history items: Clipboard app data directory not found ({basePinDir})";

            var mockClipboardItem = new Mock<IClipboardHistoryItemWrapper>(MockBehavior.Strict);

            var pinnedItemProvider = new PinnedClipboardItemProvider(basePinDir);

            // Act
            var result = pinnedItemProvider.TryRemovePinnedClipboardHistoryItem(mockClipboardItem.Object, out var actualErrorMessage);

            // Assert
            Assert.That(result, Is.False);
            Assert.That(actualErrorMessage, Is.EqualTo(expectedErrorMessage));
        }

        [Test]
        public void TryRemovePinnedClipboardHistoryItem_PinGuidDirectoryNotExist_ReturnsFalseAndExpectedError()
        {
            // Arrange
            var expectedErrorMessage = "Failed to retrieve pinned clipboard history items: Metadata file not found";

            var mockClipboardItem = new Mock<IClipboardHistoryItemWrapper>(MockBehavior.Strict);

            var basePinDir = GetTestPinnedItemBaseDirectory();
            Directory.CreateDirectory(basePinDir);
            var pinnedItemProvider = new PinnedClipboardItemProvider(basePinDir);

            // Act
            var result = pinnedItemProvider.TryRemovePinnedClipboardHistoryItem(mockClipboardItem.Object, out var actualErrorMessage);

            // Assert
            Assert.That(result, Is.False);
            Assert.That(actualErrorMessage, Is.EqualTo(expectedErrorMessage));
        }

        [Test]
        public void TryRemovePinnedClipboardHistoryItem_PinMetadataFileNotExist_ReturnsFalseAndExpectedError()
        {
            // Arrange
            var expectedErrorMessage = "Failed to retrieve pinned clipboard history items: Metadata file not found";

            var mockClipboardItem = new Mock<IClipboardHistoryItemWrapper>(MockBehavior.Strict);

            var basePinDir = GetTestPinnedItemBaseDirectory();
            var pinGuidDir = GetNewTestPinnedGuidDirectory();
            Directory.CreateDirectory(pinGuidDir);
            var pinnedItemProvider = new PinnedClipboardItemProvider(basePinDir);

            // Act
            var result = pinnedItemProvider.TryRemovePinnedClipboardHistoryItem(mockClipboardItem.Object, out var actualErrorMessage);

            // Assert
            Assert.That(result, Is.False);
            Assert.That(actualErrorMessage, Is.EqualTo(expectedErrorMessage));
        }

        [Test]
        public void TryRemovePinnedClipboardHistoryItem_ItemAlreadyUnpinned_ReturnsFalseAndExpectedError()
        {
            // Arrange
            var expectedItemId = GetNewFormattedGuidString();
            var expectedErrorMessage = $"Clipboard history item with ID {expectedItemId} is not pinned.";

            var pinnedMetadataList = new PinnedClipboardMetadataList();
            CreateTestPinnedItemMetadataFile(pinnedMetadataList);

            var mockClipboardItem = new Mock<IClipboardHistoryItemWrapper>(MockBehavior.Strict);

            mockClipboardItem
                .SetupGet(mock => mock.Id)
                .Returns(expectedItemId);

            var pinnedItemProvider = new PinnedClipboardItemProvider(GetTestPinnedItemBaseDirectory());

            // Act
            var result = pinnedItemProvider.TryRemovePinnedClipboardHistoryItem(mockClipboardItem.Object, out var actualErrorMessage);

            // Assert
            Assert.That(result, Is.False);
            Assert.That(actualErrorMessage, Is.EqualTo(expectedErrorMessage));
        }

        [Test]
        public void TryRemovePinnedClipboardHistoryItem_PinnedItemDirectoryNotExist_ReturnsFalseAndExpectedError()
        {
            // Arrange
            var expectedItemIds = new HashSet<string>();
            var pinnedMetadataList = new PinnedClipboardMetadataList();
            pinnedMetadataList.Items = new Dictionary<string, PinnedClipboardMetadataListItem>();

            var pinnedMetadataListItem = new PinnedClipboardMetadataListItem
            {
                Timestamp = DateTimeOffset.Now,
                Source = "Local"
            };

            var expectedItemId = GetNewFormattedGuidString();
            var expectedErrorMessage = $"Pinned item {expectedItemId} not found on disk.";

            expectedItemIds.Add(expectedItemId);
            pinnedMetadataList.Items.Add(expectedItemId, pinnedMetadataListItem);

            CreateTestPinnedItemMetadataFile(pinnedMetadataList);

            var pinnedItemProvider = new PinnedClipboardItemProvider(GetTestPinnedItemBaseDirectory());

            var mockClipboardItem = new Mock<IClipboardHistoryItemWrapper>(MockBehavior.Strict);
            mockClipboardItem
                .SetupGet(mock => mock.Id)
                .Returns(expectedItemId);

            var expectedContent = "Item 0";
            var dataPackage = new DataPackage();
            dataPackage.SetText(expectedContent);
            mockClipboardItem
                .SetupGet(mock => mock.Content)
                .Returns(dataPackage.GetView());

            // Act
            var result = pinnedItemProvider.TryRemovePinnedClipboardHistoryItem(mockClipboardItem.Object, out var errorMessage);

            // Assert
            Assert.That(result, Is.False);
            Assert.That(errorMessage, Is.EqualTo(expectedErrorMessage));
        }
        #endregion TryRemovePinnedClipboardHistoryItem

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
            Directory.CreateDirectory(_pinnedItemsDirectory);

            var pinnedMetadataFilePath = GetNewTestPinnedMetadataFilePath(_pinnedItemsDirectory);
            var pinnedMetadataListJson = JsonConvert.SerializeObject(pinnedMetadataList, new JsonSerializerSettings
            {
                DateFormatString = PinnedClipboardMetadataListItem.TimestampFormatString
            });
            File.WriteAllText(pinnedMetadataFilePath, pinnedMetadataListJson, Encoding.Unicode);
        }

        private void CreateEmptyTestPinnedItemMetadataFile()
        {
            Directory.CreateDirectory(_pinnedItemsDirectory);

            var pinnedMetadataFilePath = GetNewTestPinnedMetadataFilePath(_pinnedItemsDirectory);
            File.Create(pinnedMetadataFilePath).Close();
        }

        private void CreateTestPinnedItemDirectory(IClipboardHistoryItemWrapper item)
        {
            // Create directory for item (name is the item GUID)
            var pinnedItemPath = Path.Combine(_pinnedItemsDirectory, item.Id);
            Directory.CreateDirectory(pinnedItemPath);

            // Create metadata file for item
            var pinnedItemMetadataPath = Path.Combine(pinnedItemPath, "metadata.json");
            var pinnedItemMetadata = JsonConvert.SerializeObject(PinnedItemMetadata.Text);
            File.WriteAllText(pinnedItemMetadataPath, pinnedItemMetadata, Encoding.Unicode);

            // Create content file for item (file name is the base64 encoded name of item data type)
            if (item.Content.Contains(StandardDataFormats.Text))
            {
                var value = item.Content.GetTextAsync().GetResults();
                string encodedFormatId = Convert.ToBase64String(Encoding.ASCII.GetBytes(StandardDataFormats.Text));
                string itemContentPath = Path.Combine(pinnedItemPath, encodedFormatId);
                File.WriteAllText(itemContentPath, value);
            }
            else
            {
                Assert.Fail($"Unable to create test pinned item directory for unsupported format: {string.Join(',', item.Content.AvailableFormats)}");
            }
        }
        #endregion Helpers
    }
}
