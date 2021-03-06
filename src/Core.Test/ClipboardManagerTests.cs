using Moq;
using past.Core.Wrappers;
using System.Windows;
using Windows.ApplicationModel.DataTransfer;

namespace past.Core.Test
{
    public class ClipboardManagerTests
    {
        #region Constructors
        [Test]
        public void Constructor_Parameterless_Success()
        {
            Assert.DoesNotThrow(() => new ClipboardManager());
        }

        [Test]
        public void Constructor_CreateWithNonNullParameters_ThrowsArgumentNullException()
        {
            var mockWinRtClipboard = new Mock<IWinRtClipboardWrapper>(MockBehavior.Strict);
            var mockWin32Clipboard = new Mock<IWin32ClipboardWrapper>(MockBehavior.Strict);
            var mockPinnedClipboardProvider = new Mock<IPinnedClipboardItemProvider>(MockBehavior.Strict);
            Assert.DoesNotThrow(() => new ClipboardManager(mockWinRtClipboard.Object, mockWin32Clipboard.Object, mockPinnedClipboardProvider.Object));
        }

        [Test]
        public void Constructor_CreateWithNullWinRtClipboard_ThrowsArgumentNullException()
        {
            var mockWin32Clipboard = new Mock<IWin32ClipboardWrapper>(MockBehavior.Strict);
            var mockPinnedClipboardProvider = new Mock<IPinnedClipboardItemProvider>(MockBehavior.Strict);
            Assert.Throws<ArgumentNullException>(() => new ClipboardManager(null!, mockWin32Clipboard.Object, mockPinnedClipboardProvider.Object));
        }

        [Test]
        public void Constructor_CreateWithNullWin32Clipboard_ThrowsArgumentNullException()
        {
            var mockWinRtClipboard = new Mock<IWinRtClipboardWrapper>(MockBehavior.Strict);
            var mockPinnedClipboardProvider = new Mock<IPinnedClipboardItemProvider>(MockBehavior.Strict);
            Assert.Throws<ArgumentNullException>(() => new ClipboardManager(mockWinRtClipboard.Object, null!, mockPinnedClipboardProvider.Object));
        }

        [Test]
        public void Constructor_CreateWithNullPinnedClipboardItemProvider_ThrowsArgumentNullException()
        {
            var mockWinRtClipboard = new Mock<IWinRtClipboardWrapper>(MockBehavior.Strict);
            var mockWin32Clipboard = new Mock<IWin32ClipboardWrapper>(MockBehavior.Strict);
            Assert.Throws<ArgumentNullException>(() => new ClipboardManager(mockWinRtClipboard.Object, mockWin32Clipboard.Object, null!));
        }
        #endregion Constructors

        #region GetCurrentClipboardValueAsync
        [Test]
        [TestCase(ContentType.Text, TextDataFormat.Text)]
        [TestCase(ContentType.Text, TextDataFormat.UnicodeText)]
        [TestCase(ContentType.All, TextDataFormat.Text)]
        [TestCase(ContentType.All, TextDataFormat.UnicodeText)]
        public async Task GetCurrentClipboardValueAsync_ForTextContent_Success(ContentType type, TextDataFormat clipboardTextDataFormat)
        {
            // Arrange
            var expectedValue = "I'm a little clipboard, short and stout!";
            var mockWin32Clipboard = new Mock<IWin32ClipboardWrapper>(MockBehavior.Strict);
            mockWin32Clipboard
                .Setup(
                mock => mock.ContainsText(It.Is<TextDataFormat>(
                    format => format == TextDataFormat.Text || format == TextDataFormat.UnicodeText)))
                .Returns<TextDataFormat>(format => format == clipboardTextDataFormat)
                .Verifiable();
            mockWin32Clipboard.Setup(
                mock => mock.GetText(It.Is<TextDataFormat>(
                    format => format == clipboardTextDataFormat)))
                .Returns(expectedValue)
                .Verifiable();
            var mockWinRtClipboard = new Mock<IWinRtClipboardWrapper>(MockBehavior.Strict);
            var mockPinnedClipboardProvider = new Mock<IPinnedClipboardItemProvider>(MockBehavior.Strict);
            var clipboardManager = new ClipboardManager(mockWinRtClipboard.Object, mockWin32Clipboard.Object, mockPinnedClipboardProvider.Object);

            // Act
            var actualValue = await clipboardManager.GetCurrentClipboardValueAsync(type, null);

            // Assert
            Assert.That(actualValue, Is.EqualTo(expectedValue));
            mockWin32Clipboard.Verify();
            mockWinRtClipboard.Verify();
        }

        [Test]
        [TestCase(ContentType.Image)]
        [TestCase(ContentType.All)]
        public async Task GetCurrentClipboardValueAsync_ForImageContent_Success(ContentType type)
        {
            // Arrange
            var expectedValue = "[Unsupported Format: Image]";
            var mockWin32Clipboard = new Mock<IWin32ClipboardWrapper>(MockBehavior.Strict);
            mockWin32Clipboard
                .Setup(
                mock => mock.ContainsText(It.Is<TextDataFormat>(
                    format => format == TextDataFormat.Text || format == TextDataFormat.UnicodeText)))
                .Returns(false)
                .Verifiable();
            mockWin32Clipboard
                .Setup(mock => mock.ContainsImage())
                .Returns(true)
                .Verifiable();
            var mockWinRtClipboard = new Mock<IWinRtClipboardWrapper>(MockBehavior.Strict);
            var mockPinnedClipboardProvider = new Mock<IPinnedClipboardItemProvider>(MockBehavior.Strict);
            var clipboardManager = new ClipboardManager(mockWinRtClipboard.Object, mockWin32Clipboard.Object, mockPinnedClipboardProvider.Object);

            // Act
            var actualValue = await clipboardManager.GetCurrentClipboardValueAsync(type, null);

            // Assert
            Assert.That(actualValue, Is.EqualTo(expectedValue));
            mockWin32Clipboard.Verify(mock => mock.ContainsText(It.Is<TextDataFormat>(
                format => format == TextDataFormat.Text)), Times.AtMostOnce);
            mockWin32Clipboard.Verify(mock => mock.ContainsText(It.Is<TextDataFormat>(
                format => format == TextDataFormat.UnicodeText)), Times.AtMostOnce);
            mockWin32Clipboard.Verify(mock => mock.ContainsImage(), Times.Once);
            mockWinRtClipboard.Verify();
        }

        [Test]
        public async Task GetCurrentClipboardValueAsync_ForUnsupportedContent_Success()
        {
            // Arrange
            ContentType type = ContentType.All;
            var dataFormats = new string[] { "foo", "bar", "baz" };
            var expectedValue = $"[Unsupported Format: {string.Join(',', dataFormats)}]";
            var mockWin32Clipboard = new Mock<IWin32ClipboardWrapper>(MockBehavior.Strict);
            mockWin32Clipboard
                .Setup(
                mock => mock.ContainsText(It.Is<TextDataFormat>(
                    format => format == TextDataFormat.Text || format == TextDataFormat.UnicodeText)))
                .Returns(false)
                .Verifiable();
            mockWin32Clipboard
                .Setup(mock => mock.ContainsImage())
                .Returns(false)
                .Verifiable();

            var mockDataObject = new Mock<IDataObject>(MockBehavior.Strict);
            mockDataObject
                .Setup(mock => mock.GetFormats())
                .Returns(dataFormats)
                .Verifiable();
            mockWin32Clipboard
                .Setup(mock => mock.GetDataObject())
                .Returns(mockDataObject.Object)
                .Verifiable();

            var mockWinRtClipboard = new Mock<IWinRtClipboardWrapper>(MockBehavior.Strict);
            var mockPinnedClipboardProvider = new Mock<IPinnedClipboardItemProvider>(MockBehavior.Strict);
            var clipboardManager = new ClipboardManager(mockWinRtClipboard.Object, mockWin32Clipboard.Object, mockPinnedClipboardProvider.Object);

            // Act
            var actualValue = await clipboardManager.GetCurrentClipboardValueAsync(type, null);

            // Assert
            Assert.That(actualValue, Is.EqualTo(expectedValue));
            mockWin32Clipboard.Verify();
            mockWinRtClipboard.Verify();
        }

        [Test]
        public void GetCurrentClipboardValueAsync_ClipboardThrowsException_RethrowsException()
        {
            // Arrange
            var expectedException = new InvalidOperationException("Uh-oh, spaghettio!");
            var mockWin32Clipboard = new Mock<IWin32ClipboardWrapper>(MockBehavior.Strict);
            mockWin32Clipboard
                .Setup(mock => mock.ContainsText(It.IsAny<TextDataFormat>()))
                .Throws(expectedException);
            var mockWinRtClipboard = new Mock<IWinRtClipboardWrapper>(MockBehavior.Strict);
            var mockPinnedClipboardProvider = new Mock<IPinnedClipboardItemProvider>(MockBehavior.Strict);
            var clipboardManager = new ClipboardManager(mockWinRtClipboard.Object, mockWin32Clipboard.Object, mockPinnedClipboardProvider.Object);

            // Act + Assert
            var actualException = Assert.ThrowsAsync<InvalidOperationException>(() => clipboardManager.GetCurrentClipboardValueAsync(ContentType.Text));
            Assert.That(actualException, Is.EqualTo(expectedException));
        }

        [Test]
        public void GetCurrentClipboardValueAsync_TimeoutAccessingClipboard_ThrowsTimeoutException()
        {
            // Arrange
            int methodCallTimeMs = 550; // 50ms more than the set timeout
            var expectedExceptionMessage = "Timeout while getting current clipboard contents.";
            var mockWin32Clipboard = new Mock<IWin32ClipboardWrapper>(MockBehavior.Strict);
            mockWin32Clipboard
                .Setup(mock => mock.ContainsText(It.IsAny<TextDataFormat>()))
                .Callback(() => Thread.Sleep(methodCallTimeMs));
            var mockWinRtClipboard = new Mock<IWinRtClipboardWrapper>(MockBehavior.Strict);
            var mockPinnedClipboardProvider = new Mock<IPinnedClipboardItemProvider>(MockBehavior.Strict);
            var clipboardManager = new ClipboardManager(mockWinRtClipboard.Object, mockWin32Clipboard.Object, mockPinnedClipboardProvider.Object);

            // Act + Assert
            var actualException = Assert.ThrowsAsync<TimeoutException>(() => clipboardManager.GetCurrentClipboardValueAsync(ContentType.Text));
            Assert.That(actualException.Message, Is.EqualTo(expectedExceptionMessage));
        }
        #endregion GetCurrentClipboardValueAsync

        #region GetClipboardHistoryItemAsync
        [Test]
        [TestCase(ClipboardHistoryItemsResultStatus.AccessDenied)]
        [TestCase(ClipboardHistoryItemsResultStatus.ClipboardHistoryDisabled)]
        public void GetCurrentClipboardValueAsync_FailToGetClipboardHistory_ThrowsException(ClipboardHistoryItemsResultStatus clipboardHistoryItemsResultStatus)
        {
            // Arrange
            var expectedExceptionMessage = $"Failed to get clipboard history. Result: {clipboardHistoryItemsResultStatus}";
            var mockClipboardItemsResult = new Mock<IClipboardHistoryItemsResultWrapper>(MockBehavior.Strict);
            mockClipboardItemsResult.SetupGet(mock => mock.Status).Returns(clipboardHistoryItemsResultStatus);

            var mockWin32Clipboard = new Mock<IWin32ClipboardWrapper>(MockBehavior.Strict);
            var mockWinRtClipboard = new Mock<IWinRtClipboardWrapper>(MockBehavior.Strict);
            mockWinRtClipboard.Setup(mock => mock.GetHistoryItemsAsync()).ReturnsAsync(mockClipboardItemsResult.Object);
            var mockPinnedClipboardProvider = new Mock<IPinnedClipboardItemProvider>(MockBehavior.Strict);

            var clipboardManager = new ClipboardManager(mockWinRtClipboard.Object, mockWin32Clipboard.Object, mockPinnedClipboardProvider.Object);

            // Act + Assert
            var actualException = Assert.ThrowsAsync<Exception>(() => clipboardManager.GetClipboardHistoryItemAsync(0, false, ContentType.Text));
            Assert.That(actualException.Message, Is.EqualTo(expectedExceptionMessage));
        }

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public async Task GetClipboardHistoryItemAsync_ValidIndex_Success(int index)
        {
            // Arrange
            SetHistoryItemAsContentStatus? expectedStatus = null;
            var expectedItems = new List<IClipboardHistoryItemWrapper>();
            for (int i = 0; i < 3; i++)
            {
                var mockClipboardItem = new Mock<IClipboardHistoryItemWrapper>(MockBehavior.Strict);
                var dataPackage = new DataPackage();
                dataPackage.SetText($"Item {i}");
                mockClipboardItem.SetupGet(mock => mock.Content).Returns(dataPackage.GetView());
                expectedItems.Add(mockClipboardItem.Object);
            }

            var mockClipboardItemsResult = new Mock<IClipboardHistoryItemsResultWrapper>(MockBehavior.Strict);
            mockClipboardItemsResult.SetupGet(mock => mock.Status).Returns(ClipboardHistoryItemsResultStatus.Success);
            mockClipboardItemsResult.SetupGet(mock => mock.Items).Returns(expectedItems);

            var mockWin32Clipboard = new Mock<IWin32ClipboardWrapper>(MockBehavior.Strict);
            var mockWinRtClipboard = new Mock<IWinRtClipboardWrapper>(MockBehavior.Strict);
            var mockPinnedClipboardItemProvider = new Mock<IPinnedClipboardItemProvider>(MockBehavior.Strict);
            mockWinRtClipboard.Setup(mock => mock.GetHistoryItemsAsync()).ReturnsAsync(mockClipboardItemsResult.Object);

            var clipboardManager = new ClipboardManager(mockWinRtClipboard.Object, mockWin32Clipboard.Object, mockPinnedClipboardItemProvider.Object);

            // Act
            var (actualItem, actualStatus) = await clipboardManager.GetClipboardHistoryItemAsync(index, false, ContentType.Text);

            // Assert
            Assert.That(actualStatus, Is.EqualTo(expectedStatus));
            Assert.That(actualItem, Is.EqualTo(expectedItems[index]));
        }

        [Test]
        [TestCase(-1)]
        [TestCase(1)]
        public void GetClipboardHistoryItemAsync_InvalidIndex_ThrowsException(int index)
        {
            // Arrange
            var expectedExceptionMessage = "Failed to get specified clipboard history item";
            var mockClipboardItem = new Mock<IClipboardHistoryItemWrapper>(MockBehavior.Strict);
            var expectedItems = new List<IClipboardHistoryItemWrapper> { mockClipboardItem.Object };
            var mockClipboardItemsResult = new Mock<IClipboardHistoryItemsResultWrapper>(MockBehavior.Strict);
            mockClipboardItemsResult.SetupGet(mock => mock.Status).Returns(ClipboardHistoryItemsResultStatus.Success);
            mockClipboardItemsResult.SetupGet(mock => mock.Items).Returns(expectedItems);

            var mockWin32Clipboard = new Mock<IWin32ClipboardWrapper>(MockBehavior.Strict);
            var mockWinRtClipboard = new Mock<IWinRtClipboardWrapper>(MockBehavior.Strict);
            mockWinRtClipboard.Setup(mock => mock.GetHistoryItemsAsync()).ReturnsAsync(mockClipboardItemsResult.Object);
            var mockPinnedClipboardProvider = new Mock<IPinnedClipboardItemProvider>(MockBehavior.Strict);

            var clipboardManager = new ClipboardManager(mockWinRtClipboard.Object, mockWin32Clipboard.Object, mockPinnedClipboardProvider.Object);

            // Act + Assert
            var actualException = Assert.ThrowsAsync<Exception>(() => clipboardManager.GetClipboardHistoryItemAsync(index, false, ContentType.Text));
            Assert.That(actualException.Message, Is.EqualTo(expectedExceptionMessage));
        }

        [Test]
        public void GetClipboardHistoryItemAsync_EmptyClipboardHistory_ThrowsArgumentOutOfBoundsException()
        {
            // Arrange
            var expectedExceptionMessage = "Failed to get specified clipboard history item";
            var mockClipboardItem = new Mock<IClipboardHistoryItemWrapper>(MockBehavior.Strict);
            var items = new List<IClipboardHistoryItemWrapper>();
            var mockClipboardItemsResult = new Mock<IClipboardHistoryItemsResultWrapper>(MockBehavior.Strict);
            mockClipboardItemsResult.SetupGet(mock => mock.Status).Returns(ClipboardHistoryItemsResultStatus.Success);
            mockClipboardItemsResult.SetupGet(mock => mock.Items).Returns(items);

            var mockWin32Clipboard = new Mock<IWin32ClipboardWrapper>(MockBehavior.Strict);
            var mockWinRtClipboard = new Mock<IWinRtClipboardWrapper>(MockBehavior.Strict);
            mockWinRtClipboard.Setup(mock => mock.GetHistoryItemsAsync()).ReturnsAsync(mockClipboardItemsResult.Object);
            var mockPinnedClipboardProvider = new Mock<IPinnedClipboardItemProvider>(MockBehavior.Strict);

            var clipboardManager = new ClipboardManager(mockWinRtClipboard.Object, mockWin32Clipboard.Object, mockPinnedClipboardProvider.Object);

            // Act + Assert
            var actualException = Assert.ThrowsAsync<Exception>(() => clipboardManager.GetClipboardHistoryItemAsync(0, false, ContentType.Text));
            Assert.That(actualException.Message, Is.EqualTo(expectedExceptionMessage));
        }

        [Test]
        public async Task GetClipboardHistoryItemAsync_ValidId_Success()
        {
            // Arrange
            SetHistoryItemAsContentStatus? expectedStatus = null;
            var mockClipboardItems = new List<IClipboardHistoryItemWrapper>();
            for (int i = 0; i < 3; i++)
            {
                var mockClipboardItem = new Mock<IClipboardHistoryItemWrapper>(MockBehavior.Strict);
                mockClipboardItem.SetupGet(mock => mock.Id).Returns(Guid.NewGuid().ToString());
                var dataPackage = new DataPackage();
                dataPackage.SetText($"Item {i}");
                mockClipboardItem.SetupGet(mock => mock.Content).Returns(dataPackage.GetView());
                mockClipboardItems.Add(mockClipboardItem.Object);
            }
            var expectedItem = mockClipboardItems[1];

            var mockClipboardItemsResult = new Mock<IClipboardHistoryItemsResultWrapper>(MockBehavior.Strict);
            mockClipboardItemsResult.SetupGet(mock => mock.Status).Returns(ClipboardHistoryItemsResultStatus.Success);
            mockClipboardItemsResult.SetupGet(mock => mock.Items).Returns(mockClipboardItems);

            var mockWin32Clipboard = new Mock<IWin32ClipboardWrapper>(MockBehavior.Strict);
            var mockWinRtClipboard = new Mock<IWinRtClipboardWrapper>(MockBehavior.Strict);
            var mockPinnedClipboardItemProvider = new Mock<IPinnedClipboardItemProvider>(MockBehavior.Strict);
            mockWinRtClipboard.Setup(mock => mock.GetHistoryItemsAsync()).ReturnsAsync(mockClipboardItemsResult.Object);

            var clipboardManager = new ClipboardManager(mockWinRtClipboard.Object, mockWin32Clipboard.Object, mockPinnedClipboardItemProvider.Object);

            // Act
            var (actualItem, actualStatus) = await clipboardManager.GetClipboardHistoryItemAsync(Guid.Parse(expectedItem.Id), false, ContentType.Text);

            // Assert
            Assert.That(actualStatus, Is.EqualTo(expectedStatus));
            Assert.That(actualItem, Is.EqualTo(expectedItem));
        }

        [Test]
        public void GetClipboardHistoryItemAsync_InvalidId_ThrowsException()
        {
            // Arrange
            var expectedExceptionMessage = "Failed to get specified clipboard history item";
            var mockClipboardItem = new Mock<IClipboardHistoryItemWrapper>(MockBehavior.Strict);
            mockClipboardItem.SetupGet(mock => mock.Id).Returns(Guid.NewGuid().ToString());
            var dataPackage = new DataPackage();
            dataPackage.SetText("Item 0");
            mockClipboardItem.SetupGet(mock => mock.Content).Returns(dataPackage.GetView());
            var mockClipboardItems = new List<IClipboardHistoryItemWrapper> { mockClipboardItem.Object };

            var mockClipboardItemsResult = new Mock<IClipboardHistoryItemsResultWrapper>(MockBehavior.Strict);
            mockClipboardItemsResult.SetupGet(mock => mock.Status).Returns(ClipboardHistoryItemsResultStatus.Success);
            mockClipboardItemsResult.SetupGet(mock => mock.Items).Returns(mockClipboardItems);

            var mockWin32Clipboard = new Mock<IWin32ClipboardWrapper>(MockBehavior.Strict);
            var mockWinRtClipboard = new Mock<IWinRtClipboardWrapper>(MockBehavior.Strict);
            var mockPinnedClipboardItemProvider = new Mock<IPinnedClipboardItemProvider>(MockBehavior.Strict);
            mockWinRtClipboard.Setup(mock => mock.GetHistoryItemsAsync()).ReturnsAsync(mockClipboardItemsResult.Object);

            var clipboardManager = new ClipboardManager(mockWinRtClipboard.Object, mockWin32Clipboard.Object, mockPinnedClipboardItemProvider.Object);

            // Act + Assert
            var actualException = Assert.ThrowsAsync<Exception>(() => clipboardManager.GetClipboardHistoryItemAsync(Guid.NewGuid(), false, ContentType.Text));
            Assert.That(actualException.Message, Is.EqualTo(expectedExceptionMessage));
        }

        [Test]
        public void GetClipboardHistoryItemAsync_UnsupportedContentType_ThrowsException()
        {
            // Arrange
            var expectedExceptionMessage = "Item does not support the specified content type";
            var mockClipboardItem = new Mock<IClipboardHistoryItemWrapper>(MockBehavior.Strict);
            var dataPackage = new DataPackage();
            dataPackage.SetText("Item 0");
            mockClipboardItem.SetupGet(mock => mock.Content).Returns(dataPackage.GetView());
            var mockClipboardItems = new List<IClipboardHistoryItemWrapper> { mockClipboardItem.Object };

            var mockClipboardItemsResult = new Mock<IClipboardHistoryItemsResultWrapper>(MockBehavior.Strict);
            mockClipboardItemsResult.SetupGet(mock => mock.Status).Returns(ClipboardHistoryItemsResultStatus.Success);
            mockClipboardItemsResult.SetupGet(mock => mock.Items).Returns(mockClipboardItems);

            var mockWin32Clipboard = new Mock<IWin32ClipboardWrapper>(MockBehavior.Strict);
            var mockWinRtClipboard = new Mock<IWinRtClipboardWrapper>(MockBehavior.Strict);
            var mockPinnedClipboardItemProvider = new Mock<IPinnedClipboardItemProvider>(MockBehavior.Strict);
            mockWinRtClipboard.Setup(mock => mock.GetHistoryItemsAsync()).ReturnsAsync(mockClipboardItemsResult.Object);

            var clipboardManager = new ClipboardManager(mockWinRtClipboard.Object, mockWin32Clipboard.Object, mockPinnedClipboardItemProvider.Object);

            // Act + Assert
            var actualException = Assert.ThrowsAsync<Exception>(() => clipboardManager.GetClipboardHistoryItemAsync(0, false, ContentType.Image));
            Assert.That(actualException.Message, Is.EqualTo(expectedExceptionMessage));
        }

        [Test]
        [TestCase(SetHistoryItemAsContentStatus.Success)]
        [TestCase(SetHistoryItemAsContentStatus.AccessDenied)]
        [TestCase(SetHistoryItemAsContentStatus.ItemDeleted)]
        public async Task GetClipboardHistoryItemAsync_SetItemAsCurrent_ReturnsExpectedStatus(SetHistoryItemAsContentStatus expectedStatus)
        {
            // Arrange
            int itemIndex = 0;
            var mockClipboardItem = new Mock<IClipboardHistoryItemWrapper>(MockBehavior.Strict);
            var dataPackage = new DataPackage();
            dataPackage.SetText("Item 0");
            mockClipboardItem.SetupGet(mock => mock.Content).Returns(dataPackage.GetView());
            var expectedItems = new List<IClipboardHistoryItemWrapper> { mockClipboardItem.Object };
            var mockClipboardItemsResult = new Mock<IClipboardHistoryItemsResultWrapper>(MockBehavior.Strict);
            mockClipboardItemsResult.SetupGet(mock => mock.Status).Returns(ClipboardHistoryItemsResultStatus.Success);
            mockClipboardItemsResult.SetupGet(mock => mock.Items).Returns(expectedItems);

            var mockWin32Clipboard = new Mock<IWin32ClipboardWrapper>(MockBehavior.Strict);
            var mockWinRtClipboard = new Mock<IWinRtClipboardWrapper>(MockBehavior.Strict);
            mockWinRtClipboard.Setup(mock => mock.GetHistoryItemsAsync()).ReturnsAsync(mockClipboardItemsResult.Object);
            mockWinRtClipboard
                .Setup(mock => mock.SetHistoryItemAsContent(
                    It.Is<IClipboardHistoryItemWrapper>(actualClipboardItem => actualClipboardItem == mockClipboardItem.Object)))
                .Returns(expectedStatus);

            var mockPinnedClipboardProvider = new Mock<IPinnedClipboardItemProvider>(MockBehavior.Strict);
            var clipboardManager = new ClipboardManager(mockWinRtClipboard.Object, mockWin32Clipboard.Object, mockPinnedClipboardProvider.Object);

            // Act
            var (actualItem, actualStatus) = await clipboardManager.GetClipboardHistoryItemAsync(itemIndex, true, ContentType.Text);

            // Assert
            Assert.That(actualStatus, Is.EqualTo(expectedStatus));
            Assert.That(actualItem, Is.EqualTo(expectedItems[itemIndex]));
        }
        #endregion GetClipboardHistoryItemAsync

        #region ListClipboardHistoryAsync
        [Test]
        [TestCase(ClipboardHistoryItemsResultStatus.AccessDenied)]
        [TestCase(ClipboardHistoryItemsResultStatus.ClipboardHistoryDisabled)]
        public void ListClipboardHistoryAsync_FailToGetClipboardHistory_ThrowsException(ClipboardHistoryItemsResultStatus clipboardHistoryItemsResultStatus)
        {
            // Arrange
            var expectedExceptionMessage = $"Failed to get clipboard history. Result: {clipboardHistoryItemsResultStatus}";
            var mockClipboardItemsResult = new Mock<IClipboardHistoryItemsResultWrapper>(MockBehavior.Strict);
            mockClipboardItemsResult.SetupGet(mock => mock.Status).Returns(clipboardHistoryItemsResultStatus);

            var mockWin32Clipboard = new Mock<IWin32ClipboardWrapper>(MockBehavior.Strict);
            var mockWinRtClipboard = new Mock<IWinRtClipboardWrapper>(MockBehavior.Strict);
            var mockPinnedClipboardProvider = new Mock<IPinnedClipboardItemProvider>(MockBehavior.Strict);
            mockWinRtClipboard.Setup(mock => mock.GetHistoryItemsAsync()).ReturnsAsync(mockClipboardItemsResult.Object);

            var clipboardManager = new ClipboardManager(mockWinRtClipboard.Object, mockWin32Clipboard.Object, mockPinnedClipboardProvider.Object);

            // Act + Assert
            var actualException = Assert.ThrowsAsync<Exception>(() => clipboardManager.ListClipboardHistoryAsync(ContentType.Text, false));
            Assert.That(actualException.Message, Is.EqualTo(expectedExceptionMessage));
        }

        [Test]
        public void ListClipboardHistoryAsync_EmptyClipboardHistory_ThrowsException()
        {
            // Arrange
            var expectedExceptionMessage = "Clipboard history is empty";
            var items = new List<IClipboardHistoryItemWrapper>();
            var mockClipboardItemsResult = new Mock<IClipboardHistoryItemsResultWrapper>(MockBehavior.Strict);
            mockClipboardItemsResult.SetupGet(mock => mock.Status).Returns(ClipboardHistoryItemsResultStatus.Success);
            mockClipboardItemsResult.SetupGet(mock => mock.Items).Returns(items);

            var mockWin32Clipboard = new Mock<IWin32ClipboardWrapper>(MockBehavior.Strict);
            var mockWinRtClipboard = new Mock<IWinRtClipboardWrapper>(MockBehavior.Strict);
            var mockPinnedClipboardProvider = new Mock<IPinnedClipboardItemProvider>(MockBehavior.Strict);
            mockWinRtClipboard.Setup(mock => mock.GetHistoryItemsAsync()).ReturnsAsync(mockClipboardItemsResult.Object);

            var clipboardManager = new ClipboardManager(mockWinRtClipboard.Object, mockWin32Clipboard.Object, mockPinnedClipboardProvider.Object);

            // Act + Assert
            var actualException = Assert.ThrowsAsync<Exception>(() => clipboardManager.ListClipboardHistoryAsync(ContentType.Text, false));
            Assert.That(actualException.Message, Is.EqualTo(expectedExceptionMessage));
        }

        [Test]
        public async Task ListClipboardHistoryAsync_AllItemsMatchContentType_ReturnsAllItems()
        {
            // Arrange
            var expectedItems = new List<IClipboardHistoryItemWrapper>();
            for (int i = 0; i < 3; i++)
            {
                var dataPackage = new DataPackage();
                dataPackage.SetText($"Item {i}");
                var mockItem = new Mock<IClipboardHistoryItemWrapper>(MockBehavior.Strict);
                mockItem.SetupGet(mock => mock.Content).Returns(dataPackage.GetView());
                expectedItems.Add(mockItem.Object);
            }

            var mockClipboardItemsResult = new Mock<IClipboardHistoryItemsResultWrapper>(MockBehavior.Strict);
            mockClipboardItemsResult.SetupGet(mock => mock.Status).Returns(ClipboardHistoryItemsResultStatus.Success);
            mockClipboardItemsResult.SetupGet(mock => mock.Items).Returns(expectedItems);

            var mockWin32Clipboard = new Mock<IWin32ClipboardWrapper>(MockBehavior.Strict);
            var mockWinRtClipboard = new Mock<IWinRtClipboardWrapper>(MockBehavior.Strict);
            var mockPinnedClipboardProvider = new Mock<IPinnedClipboardItemProvider>(MockBehavior.Strict);
            mockWinRtClipboard.Setup(mock => mock.GetHistoryItemsAsync()).ReturnsAsync(mockClipboardItemsResult.Object);

            var clipboardManager = new ClipboardManager(mockWinRtClipboard.Object, mockWin32Clipboard.Object, mockPinnedClipboardProvider.Object);

            // Act
            var actualItems = await clipboardManager.ListClipboardHistoryAsync(ContentType.Text, false);

            // Assert
            Assert.That(actualItems, Is.EqualTo(expectedItems));
        }

        [Test]
        public async Task ListClipboardHistoryAsync_SomeItemsMatchContentType_ReturnsMatchingItems()
        {
            // Arrange
            var allItems = new List<IClipboardHistoryItemWrapper>();
            var expectedItems = new List<IClipboardHistoryItemWrapper>();
            for (int i = 0; i < 5; i++)
            {
                var dataPackage = new DataPackage();
                var mockItem = new Mock<IClipboardHistoryItemWrapper>(MockBehavior.Strict);
                mockItem.SetupGet(mock => mock.Content).Returns(dataPackage.GetView());
                if (i % 2 == 0)
                {
                    dataPackage.SetText($"Item {i}");
                    expectedItems.Add(mockItem.Object);
                }
                else
                {
                    dataPackage.SetWebLink(new Uri("http://localhost"));
                }
                allItems.Add(mockItem.Object);
            }

            var mockClipboardItemsResult = new Mock<IClipboardHistoryItemsResultWrapper>(MockBehavior.Strict);
            mockClipboardItemsResult.SetupGet(mock => mock.Status).Returns(ClipboardHistoryItemsResultStatus.Success);
            mockClipboardItemsResult.SetupGet(mock => mock.Items).Returns(allItems);

            var mockWin32Clipboard = new Mock<IWin32ClipboardWrapper>(MockBehavior.Strict);
            var mockWinRtClipboard = new Mock<IWinRtClipboardWrapper>(MockBehavior.Strict);
            var mockPinnedClipboardProvider = new Mock<IPinnedClipboardItemProvider>(MockBehavior.Strict);
            mockWinRtClipboard.Setup(mock => mock.GetHistoryItemsAsync()).ReturnsAsync(mockClipboardItemsResult.Object);

            var clipboardManager = new ClipboardManager(mockWinRtClipboard.Object, mockWin32Clipboard.Object, mockPinnedClipboardProvider.Object);

            // Act
            var actualItems = await clipboardManager.ListClipboardHistoryAsync(ContentType.Text, false);

            // Assert
            Assert.That(actualItems, Is.EqualTo(expectedItems));
        }

        [Test]
        public void ListClipboardHistoryAsync_NoItemsMatchContentType_ThrowsException()
        {
            // Arrange
            var expectedExceptionMessage = "No supported items in clipboard history";
            var items = new List<IClipboardHistoryItemWrapper>();
            for (int i = 0; i < 3; i++)
            {
                var dataPackage = new DataPackage();
                dataPackage.SetText($"Item {i}");
                var mockItem = new Mock<IClipboardHistoryItemWrapper>(MockBehavior.Strict);
                mockItem.SetupGet(mock => mock.Content).Returns(dataPackage.GetView());
                items.Add(mockItem.Object);
            }

            var mockClipboardItemsResult = new Mock<IClipboardHistoryItemsResultWrapper>(MockBehavior.Strict);
            mockClipboardItemsResult.SetupGet(mock => mock.Status).Returns(ClipboardHistoryItemsResultStatus.Success);
            mockClipboardItemsResult.SetupGet(mock => mock.Items).Returns(items);

            var mockWin32Clipboard = new Mock<IWin32ClipboardWrapper>(MockBehavior.Strict);
            var mockWinRtClipboard = new Mock<IWinRtClipboardWrapper>(MockBehavior.Strict);
            var mockPinnedClipboardProvider = new Mock<IPinnedClipboardItemProvider>(MockBehavior.Strict);
            mockWinRtClipboard.Setup(mock => mock.GetHistoryItemsAsync()).ReturnsAsync(mockClipboardItemsResult.Object);

            var clipboardManager = new ClipboardManager(mockWinRtClipboard.Object, mockWin32Clipboard.Object, mockPinnedClipboardProvider.Object);

            // Act + Assert
            var actualException = Assert.ThrowsAsync<Exception>(() => clipboardManager.ListClipboardHistoryAsync(ContentType.Image, false));
            Assert.That(actualException.Message, Is.EqualTo(expectedExceptionMessage));
        }

        [Test]
        public async Task ListClipboardHistoryAsync_GetPinnedItems_ReturnsOnlyPinnedItems()
        {
            // Arrange
            var mockClipboardItems = new List<IClipboardHistoryItemWrapper>();
            for (int i = 0; i < 5; i++)
            {
                var mockClipboardItem = new Mock<IClipboardHistoryItemWrapper>(MockBehavior.Strict);
                mockClipboardItem.SetupGet(mock => mock.Id).Returns(Guid.NewGuid().ToString());
                var dataPackage = new DataPackage();
                dataPackage.SetText($"Item {i}");
                mockClipboardItem.SetupGet(mock => mock.Content).Returns(dataPackage.GetView());
                mockClipboardItems.Add(mockClipboardItem.Object);
            }

            var expectedPinnedItemIds = new HashSet<string> { mockClipboardItems[1].Id, mockClipboardItems[4].Id };
            var expectedPinnedItems = mockClipboardItems.Where(item => expectedPinnedItemIds.Contains(item.Id));

            var mockClipboardItemsResult = new Mock<IClipboardHistoryItemsResultWrapper>(MockBehavior.Strict);
            mockClipboardItemsResult.SetupGet(mock => mock.Status).Returns(ClipboardHistoryItemsResultStatus.Success);
            mockClipboardItemsResult.SetupGet(mock => mock.Items).Returns(mockClipboardItems);

            var mockWin32Clipboard = new Mock<IWin32ClipboardWrapper>(MockBehavior.Strict);
            var mockWinRtClipboard = new Mock<IWinRtClipboardWrapper>(MockBehavior.Strict);
            mockWinRtClipboard.Setup(mock => mock.GetHistoryItemsAsync()).ReturnsAsync(mockClipboardItemsResult.Object);

            var mockPinnedClipboardProvider = new Mock<IPinnedClipboardItemProvider>(MockBehavior.Strict);
            mockPinnedClipboardProvider
                .Setup(mock => mock.TryGetPinnedClipboardHistoryItemIds(out It.Ref<HashSet<string>?>.IsAny, out It.Ref<string?>.IsAny))
                .Callback((out HashSet<string>? pinnedItemIds, out string? errorMessage) =>
                {
                    pinnedItemIds = expectedPinnedItemIds;
                    errorMessage = null;
                })
                .Returns(true);

            var clipboardManager = new ClipboardManager(mockWinRtClipboard.Object, mockWin32Clipboard.Object, mockPinnedClipboardProvider.Object);

            // Act
            var actualPinnedItems = await clipboardManager.ListClipboardHistoryAsync(ContentType.Text, true);

            // Assert
            Assert.That(actualPinnedItems, Is.EqualTo(expectedPinnedItems));
        }

        [Test]
        public void ListClipboardHistoryAsync_FailToGetPinnedItems_ThrowsException()
        {
            // Arrange
            var expectedExceptionMessage = "Unable to get pinned items";
            var mockClipboardItem = new Mock<IClipboardHistoryItemWrapper>(MockBehavior.Strict);
            var dataPackage = new DataPackage();
            dataPackage.SetText($"Item 0");
            mockClipboardItem.SetupGet(mock => mock.Content).Returns(dataPackage.GetView());
            var items = new List<IClipboardHistoryItemWrapper> { mockClipboardItem.Object };
            var mockClipboardItemsResult = new Mock<IClipboardHistoryItemsResultWrapper>(MockBehavior.Strict);
            mockClipboardItemsResult.SetupGet(mock => mock.Status).Returns(ClipboardHistoryItemsResultStatus.Success);
            mockClipboardItemsResult.SetupGet(mock => mock.Items).Returns(items);

            var mockWin32Clipboard = new Mock<IWin32ClipboardWrapper>(MockBehavior.Strict);
            var mockWinRtClipboard = new Mock<IWinRtClipboardWrapper>(MockBehavior.Strict);
            mockWinRtClipboard.Setup(mock => mock.GetHistoryItemsAsync()).ReturnsAsync(mockClipboardItemsResult.Object);

            var mockPinnedClipboardProvider = new Mock<IPinnedClipboardItemProvider>(MockBehavior.Strict);
            mockPinnedClipboardProvider
                .Setup(mock => mock.TryGetPinnedClipboardHistoryItemIds(out It.Ref<HashSet<string>?>.IsAny, out It.Ref<string?>.IsAny))
                .Callback((out HashSet<string>? pinnedItemIds, out string? errorMessage) =>
                {
                    pinnedItemIds = null;
                    errorMessage = expectedExceptionMessage;
                })
                .Returns(false);

            var clipboardManager = new ClipboardManager(mockWinRtClipboard.Object, mockWin32Clipboard.Object, mockPinnedClipboardProvider.Object);

            // Act + Assert
            var actualException = Assert.ThrowsAsync<Exception>(() => clipboardManager.ListClipboardHistoryAsync(ContentType.Text, true));
            Assert.That(actualException.Message, Is.EqualTo(expectedExceptionMessage));
        }

        [Test]
        public void ListClipboardHistoryAsync_NoPinnedItems_ThrowsException()
        {
            // Arrange
            var expectedExceptionMessage = "No pinned items in clipboard history";
            var mockClipboardItem = new Mock<IClipboardHistoryItemWrapper>(MockBehavior.Strict);
            var dataPackage = new DataPackage();
            dataPackage.SetText($"Item 0");
            mockClipboardItem.SetupGet(mock => mock.Content).Returns(dataPackage.GetView());
            mockClipboardItem.SetupGet(mock => mock.Id).Returns(Guid.NewGuid().ToString());

            var items = new List<IClipboardHistoryItemWrapper> { mockClipboardItem.Object };
            var mockClipboardItemsResult = new Mock<IClipboardHistoryItemsResultWrapper>(MockBehavior.Strict);
            mockClipboardItemsResult.SetupGet(mock => mock.Status).Returns(ClipboardHistoryItemsResultStatus.Success);
            mockClipboardItemsResult.SetupGet(mock => mock.Items).Returns(items);

            var mockWin32Clipboard = new Mock<IWin32ClipboardWrapper>(MockBehavior.Strict);
            var mockWinRtClipboard = new Mock<IWinRtClipboardWrapper>(MockBehavior.Strict);
            mockWinRtClipboard.Setup(mock => mock.GetHistoryItemsAsync()).ReturnsAsync(mockClipboardItemsResult.Object);

            var mockPinnedClipboardProvider = new Mock<IPinnedClipboardItemProvider>(MockBehavior.Strict);
            mockPinnedClipboardProvider
                .Setup(mock => mock.TryGetPinnedClipboardHistoryItemIds(out It.Ref<HashSet<string>?>.IsAny, out It.Ref<string?>.IsAny))
                .Callback((out HashSet<string>? pinnedItemIds, out string? errorMessage) =>
                {
                    pinnedItemIds = new HashSet<string>();
                    errorMessage = null;
                })
                .Returns(true);

            var clipboardManager = new ClipboardManager(mockWinRtClipboard.Object, mockWin32Clipboard.Object, mockPinnedClipboardProvider.Object);

            // Act + Assert
            var actualException = Assert.ThrowsAsync<Exception>(() => clipboardManager.ListClipboardHistoryAsync(ContentType.Text, true));
            Assert.That(actualException.Message, Is.EqualTo(expectedExceptionMessage));
        }

        [Test]
        public void ListClipboardHistoryAsync_NoMatchingPinnedItems_ThrowsException()
        {
            // Arrange
            var expectedExceptionMessage = "No pinned items in clipboard history";
            var mockClipboardItem = new Mock<IClipboardHistoryItemWrapper>(MockBehavior.Strict);
            var dataPackage = new DataPackage();
            dataPackage.SetText($"Item 0");
            mockClipboardItem.SetupGet(mock => mock.Content).Returns(dataPackage.GetView());
            mockClipboardItem.SetupGet(mock => mock.Id).Returns(Guid.NewGuid().ToString());

            var items = new List<IClipboardHistoryItemWrapper> { mockClipboardItem.Object };
            var mockClipboardItemsResult = new Mock<IClipboardHistoryItemsResultWrapper>(MockBehavior.Strict);
            mockClipboardItemsResult.SetupGet(mock => mock.Status).Returns(ClipboardHistoryItemsResultStatus.Success);
            mockClipboardItemsResult.SetupGet(mock => mock.Items).Returns(items);

            var mockWin32Clipboard = new Mock<IWin32ClipboardWrapper>(MockBehavior.Strict);
            var mockWinRtClipboard = new Mock<IWinRtClipboardWrapper>(MockBehavior.Strict);
            mockWinRtClipboard.Setup(mock => mock.GetHistoryItemsAsync()).ReturnsAsync(mockClipboardItemsResult.Object);

            var mockPinnedClipboardProvider = new Mock<IPinnedClipboardItemProvider>(MockBehavior.Strict);
            mockPinnedClipboardProvider
                .Setup(mock => mock.TryGetPinnedClipboardHistoryItemIds(out It.Ref<HashSet<string>?>.IsAny, out It.Ref<string?>.IsAny))
                .Callback((out HashSet<string>? pinnedItemIds, out string? errorMessage) =>
                {
                    pinnedItemIds = new HashSet<string> { Guid.NewGuid().ToString() };
                    errorMessage = null;
                })
                .Returns(true);

            var clipboardManager = new ClipboardManager(mockWinRtClipboard.Object, mockWin32Clipboard.Object, mockPinnedClipboardProvider.Object);

            // Act + Assert
            var actualException = Assert.ThrowsAsync<Exception>(() => clipboardManager.ListClipboardHistoryAsync(ContentType.Text, true));
            Assert.That(actualException.Message, Is.EqualTo(expectedExceptionMessage));
        }
        #endregion ListClipboardHistoryAsync

        #region IsHistoryEnabled
        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void IsHistoryEnabled_ReturnsExpectedValue(bool expectedIsHistoryEnabled)
        {
            // Arrange
            var mockWin32Clipboard = new Mock<IWin32ClipboardWrapper>(MockBehavior.Strict);
            var mockWinRtClipboard = new Mock<IWinRtClipboardWrapper>(MockBehavior.Strict);
            var mockPinnedClipboardProvider = new Mock<IPinnedClipboardItemProvider>(MockBehavior.Strict);
            mockWinRtClipboard.Setup(mock => mock.IsHistoryEnabled()).Returns(expectedIsHistoryEnabled);

            var clipboardManager = new ClipboardManager(mockWinRtClipboard.Object, mockWin32Clipboard.Object, mockPinnedClipboardProvider.Object);

            // Act
            var actualIsHistoryEnabled = clipboardManager.IsHistoryEnabled();

            // Assert
            Assert.That(actualIsHistoryEnabled, Is.EqualTo(expectedIsHistoryEnabled));
        }
        #endregion IsHistoryEnabled

        #region IsRoamingEnabled
        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void IsRoamingEnabled_ReturnsExpectedValue(bool expectedIsRoamingEnabled)
        {
            // Arrange
            var mockWin32Clipboard = new Mock<IWin32ClipboardWrapper>(MockBehavior.Strict);
            var mockWinRtClipboard = new Mock<IWinRtClipboardWrapper>(MockBehavior.Strict);
            var mockPinnedClipboardProvider = new Mock<IPinnedClipboardItemProvider>(MockBehavior.Strict);
            mockWinRtClipboard.Setup(mock => mock.IsRoamingEnabled()).Returns(expectedIsRoamingEnabled);

            var clipboardManager = new ClipboardManager(mockWinRtClipboard.Object, mockWin32Clipboard.Object, mockPinnedClipboardProvider.Object);

            // Act
            var actualIsRoamingEnabled = clipboardManager.IsRoamingEnabled();

            // Assert
            Assert.That(actualIsRoamingEnabled, Is.EqualTo(expectedIsRoamingEnabled));
        }
        #endregion IsRoamingEnabled
    }
}
