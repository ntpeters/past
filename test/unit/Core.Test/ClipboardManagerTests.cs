using Moq;
using past.Core.Extensions;
using past.Core.Providers;
using past.Core.Wrappers;
using System;
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
            var expectedValue = "[Unsupported Format: Image support coming soon]";
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
        public void GetCurrentClipboardValueAsync_RequestImageForNonImageContent_ThrowsException ()
        {
            // Arrange
            var expectedExceptionErrorCode = ErrorCode.IncompatibleContentType;
            var expectedExceptionMessage = $"Item does not support the specified content type";

            ContentType type = ContentType.Image;
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
            var mockWinRtClipboard = new Mock<IWinRtClipboardWrapper>(MockBehavior.Strict);
            var mockPinnedClipboardProvider = new Mock<IPinnedClipboardItemProvider>(MockBehavior.Strict);
            var clipboardManager = new ClipboardManager(mockWinRtClipboard.Object, mockWin32Clipboard.Object, mockPinnedClipboardProvider.Object);

            // Act + Assert
            var actualException = Assert.ThrowsAsync<PastException>(() => clipboardManager.GetCurrentClipboardValueAsync(type, null));

            Assert.That(actualException.Message, Is.EqualTo(expectedExceptionMessage));
            Assert.That(actualException.ErrorCode, Is.EqualTo(expectedExceptionErrorCode));

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
        public void GetCurrentClipboardValueAsync_ForUnsupportedContentType_ThrowsException()
        {
            // Arrange
            var expectedExceptionErrorCode = ErrorCode.IncompatibleContentType;
            var expectedExceptionMessage = $"Item does not support the specified content type";

            ContentType type = 0;
            var mockWin32Clipboard = new Mock<IWin32ClipboardWrapper>(MockBehavior.Strict);
            var mockWinRtClipboard = new Mock<IWinRtClipboardWrapper>(MockBehavior.Strict);
            var mockPinnedClipboardProvider = new Mock<IPinnedClipboardItemProvider>(MockBehavior.Strict);
            var clipboardManager = new ClipboardManager(mockWinRtClipboard.Object, mockWin32Clipboard.Object, mockPinnedClipboardProvider.Object);

            // Act + Assert
            var actualException = Assert.ThrowsAsync<PastException>(() => clipboardManager.GetCurrentClipboardValueAsync(type, null));

            Assert.That(actualException.Message, Is.EqualTo(expectedExceptionMessage));
            Assert.That(actualException.ErrorCode, Is.EqualTo(expectedExceptionErrorCode));

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
        public void GetClipboardHistoryItemAsync_FailToGetClipboardHistory_ThrowsException(ClipboardHistoryItemsResultStatus clipboardHistoryItemsResultStatus)
        {
            // Arrange
            var expectedExceptionErrorCode = clipboardHistoryItemsResultStatus.ToErrorCode();
            var expectedExceptionMessage = $"Failed to get clipboard history. Result: {clipboardHistoryItemsResultStatus}";
            var mockClipboardItemsResult = new Mock<IClipboardHistoryItemsResultWrapper>(MockBehavior.Strict);
            mockClipboardItemsResult.SetupGet(mock => mock.Status).Returns(clipboardHistoryItemsResultStatus);

            var mockWin32Clipboard = new Mock<IWin32ClipboardWrapper>(MockBehavior.Strict);
            var mockWinRtClipboard = new Mock<IWinRtClipboardWrapper>(MockBehavior.Strict);
            mockWinRtClipboard.Setup(mock => mock.GetHistoryItemsAsync()).ReturnsAsync(mockClipboardItemsResult.Object);
            var mockPinnedClipboardProvider = new Mock<IPinnedClipboardItemProvider>(MockBehavior.Strict);

            var clipboardManager = new ClipboardManager(mockWinRtClipboard.Object, mockWin32Clipboard.Object, mockPinnedClipboardProvider.Object);

            // Act + Assert
            var actualException = Assert.ThrowsAsync<PastException>(() => clipboardManager.GetClipboardHistoryItemAsync(0, ContentType.Text));
            Assert.That(actualException.Message, Is.EqualTo(expectedExceptionMessage));
            Assert.That(actualException.ErrorCode, Is.EqualTo(expectedExceptionErrorCode));
        }

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public async Task GetClipboardHistoryItemAsync_ValidIndex_Success(int index)
        {
            // Arrange
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
            var actualItem = await clipboardManager.GetClipboardHistoryItemAsync(index, ContentType.Text);

            // Assert
            Assert.That(actualItem, Is.EqualTo(expectedItems[index]));
        }

        [Test]
        [TestCase(-1)]
        [TestCase(1)]
        public void GetClipboardHistoryItemAsync_InvalidIndex_ThrowsException(int index)
        {
            // Arrange
            var expectedExceptionErrorCode = ErrorCode.NotFound;
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
            var actualException = Assert.ThrowsAsync<PastException>(() => clipboardManager.GetClipboardHistoryItemAsync(index, ContentType.Text));
            Assert.That(actualException.Message, Is.EqualTo(expectedExceptionMessage));
            Assert.That(actualException.ErrorCode, Is.EqualTo(expectedExceptionErrorCode));
        }

        [Test]
        public void GetClipboardHistoryItemAsync_EmptyClipboardHistory_ThrowsArgumentOutOfBoundsException()
        {
            // Arrange
            var expectedExceptionErrorCode = ErrorCode.NotFound;
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
            var actualException = Assert.ThrowsAsync<PastException>(() => clipboardManager.GetClipboardHistoryItemAsync(0, ContentType.Text));
            Assert.That(actualException.Message, Is.EqualTo(expectedExceptionMessage));
            Assert.That(actualException.ErrorCode, Is.EqualTo(expectedExceptionErrorCode));
        }

        [Test]
        public async Task GetClipboardHistoryItemAsync_ValidId_Success()
        {
            // Arrange
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
            var actualItem = await clipboardManager.GetClipboardHistoryItemAsync(Guid.Parse(expectedItem.Id), ContentType.Text);

            // Assert
            Assert.That(actualItem, Is.EqualTo(expectedItem));
        }

        [Test]
        public void GetClipboardHistoryItemAsync_InvalidId_ThrowsException()
        {
            // Arrange
            var expectedExceptionErrorCode = ErrorCode.NotFound;
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
            var actualException = Assert.ThrowsAsync<PastException>(() => clipboardManager.GetClipboardHistoryItemAsync(Guid.NewGuid(), ContentType.Text));
            Assert.That(actualException.Message, Is.EqualTo(expectedExceptionMessage));
            Assert.That(actualException.ErrorCode, Is.EqualTo(expectedExceptionErrorCode));
        }

        [Test]
        public void GetClipboardHistoryItemAsync_UnsupportedContentType_ThrowsException()
        {
            // Arrange
            var expectedExceptionErrorCode = ErrorCode.IncompatibleContentType;
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
            var actualException = Assert.ThrowsAsync<PastException>(() => clipboardManager.GetClipboardHistoryItemAsync(0, ContentType.Image));
            Assert.That(actualException.Message, Is.EqualTo(expectedExceptionMessage));
            Assert.That(actualException.ErrorCode, Is.EqualTo(expectedExceptionErrorCode));
        }

        [Test]
        public void GetClipboardHistoryItemAsync_NullIdentifier_ThrowsArgumentNullException()
        {
            // Arrange
            var expectedExceptionMessage = "Value cannot be null. (Parameter 'identifier')";
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
            var actualException = Assert.ThrowsAsync<ArgumentNullException>(() => clipboardManager.GetClipboardHistoryItemAsync(null!, ContentType.Text));
            Assert.That(actualException.Message, Is.EqualTo(expectedExceptionMessage));
            Assert.That(actualException.ParamName, Is.EqualTo("identifier"));
        }
        #endregion GetClipboardHistoryItemAsync

        #region GetClipboardHistoryAsync
        [Test]
        [TestCase(ClipboardHistoryItemsResultStatus.AccessDenied)]
        [TestCase(ClipboardHistoryItemsResultStatus.ClipboardHistoryDisabled)]
        public void GetClipboardHistoryAsync_FailToGetClipboardHistory_ThrowsException(ClipboardHistoryItemsResultStatus clipboardHistoryItemsResultStatus)
        {
            // Arrange
            var expectedExceptionErrorCode = clipboardHistoryItemsResultStatus.ToErrorCode();
            var expectedExceptionMessage = $"Failed to get clipboard history. Result: {clipboardHistoryItemsResultStatus}";
            var mockClipboardItemsResult = new Mock<IClipboardHistoryItemsResultWrapper>(MockBehavior.Strict);
            mockClipboardItemsResult.SetupGet(mock => mock.Status).Returns(clipboardHistoryItemsResultStatus);

            var mockWin32Clipboard = new Mock<IWin32ClipboardWrapper>(MockBehavior.Strict);
            var mockWinRtClipboard = new Mock<IWinRtClipboardWrapper>(MockBehavior.Strict);
            var mockPinnedClipboardProvider = new Mock<IPinnedClipboardItemProvider>(MockBehavior.Strict);
            mockWinRtClipboard.Setup(mock => mock.GetHistoryItemsAsync()).ReturnsAsync(mockClipboardItemsResult.Object);

            var clipboardManager = new ClipboardManager(mockWinRtClipboard.Object, mockWin32Clipboard.Object, mockPinnedClipboardProvider.Object);

            // Act + Assert
            var actualException = Assert.ThrowsAsync<PastException>(() => clipboardManager.GetClipboardHistoryAsync(ContentType.Text, false));
            Assert.That(actualException.Message, Is.EqualTo(expectedExceptionMessage));
            Assert.That(actualException.ErrorCode, Is.EqualTo(expectedExceptionErrorCode));
        }

        [Test]
        public async Task GetClipboardHistoryAsync_EmptyClipboardHistory_ReturnsEmptyList()
        {
            // Arrange
            var items = new List<IClipboardHistoryItemWrapper>();
            var mockClipboardItemsResult = new Mock<IClipboardHistoryItemsResultWrapper>(MockBehavior.Strict);
            mockClipboardItemsResult.SetupGet(mock => mock.Status).Returns(ClipboardHistoryItemsResultStatus.Success);
            mockClipboardItemsResult.SetupGet(mock => mock.Items).Returns(items);

            var mockWin32Clipboard = new Mock<IWin32ClipboardWrapper>(MockBehavior.Strict);
            var mockWinRtClipboard = new Mock<IWinRtClipboardWrapper>(MockBehavior.Strict);
            var mockPinnedClipboardProvider = new Mock<IPinnedClipboardItemProvider>(MockBehavior.Strict);
            mockWinRtClipboard.Setup(mock => mock.GetHistoryItemsAsync()).ReturnsAsync(mockClipboardItemsResult.Object);

            var clipboardManager = new ClipboardManager(mockWinRtClipboard.Object, mockWin32Clipboard.Object, mockPinnedClipboardProvider.Object);

            // Act
            var actualItems = await clipboardManager.GetClipboardHistoryAsync(ContentType.Text, false);

            // Assert
            Assert.That(actualItems, Is.EqualTo(items));
        }

        [Test]
        public async Task GetClipboardHistoryAsync_AllItemsMatchContentType_ReturnsAllItems()
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
            var actualItems = await clipboardManager.GetClipboardHistoryAsync(ContentType.Text, false);

            // Assert
            Assert.That(actualItems, Is.EqualTo(expectedItems));
        }

        [Test]
        public async Task GetClipboardHistoryAsync_SomeItemsMatchContentType_ReturnsMatchingItems()
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
            var actualItems = await clipboardManager.GetClipboardHistoryAsync(ContentType.Text, false);

            // Assert
            Assert.That(actualItems, Is.EqualTo(expectedItems));
        }

        [Test]
        public async Task GetClipboardHistoryAsync_NoItemsMatchContentType_ReturnsEmptyList()
        {
            // Arrange
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

            // Act
            var actualItems = await clipboardManager.GetClipboardHistoryAsync(ContentType.Image, false);

            // Assert
            Assert.That(actualItems, Is.Empty);
        }

        [Test]
        public async Task GetClipboardHistoryAsync_GetPinnedItems_ReturnsOnlyPinnedItems()
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
            var actualPinnedItems = await clipboardManager.GetClipboardHistoryAsync(ContentType.Text, true);

            // Assert
            Assert.That(actualPinnedItems, Is.EqualTo(expectedPinnedItems));
        }

        [Test]
        public void GetClipboardHistoryAsync_FailToGetPinnedItems_ThrowsException()
        {
            // Arrange
            var expectedExceptionErrorCode = ErrorCode.NotFound;
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
            var actualException = Assert.ThrowsAsync<PastException>(() => clipboardManager.GetClipboardHistoryAsync(ContentType.Text, true));
            Assert.That(actualException.Message, Is.EqualTo(expectedExceptionMessage));
            Assert.That(actualException.ErrorCode, Is.EqualTo(expectedExceptionErrorCode));
        }

        [Test]
        public async Task GetClipboardHistoryAsync_NoPinnedItems_ReturnsEmptyList()
        {
            // Arrange
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

            // Act
            var actualItems = await clipboardManager.GetClipboardHistoryAsync(ContentType.Text, true);

            // Assert
            Assert.That(actualItems, Is.Empty);
        }

        [Test]
        public async Task GetClipboardHistoryAsync_NoMatchingPinnedItems_ReturnsEmptyList()
        {
            // Arrange
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

            // Act
            var actualItems = await clipboardManager.GetClipboardHistoryAsync(ContentType.Text, true);

            // Assert
            Assert.That(actualItems, Is.Empty);
        }
        #endregion GetClipboardHistoryAsync

        #region PinClipboardItemAsync
        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public async Task PinClipboardItemAsync_ValidIndex_Success(int index)
        {
            // Arrange
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
            mockWinRtClipboard.Setup(mock => mock.GetHistoryItemsAsync()).ReturnsAsync(mockClipboardItemsResult.Object);

            var mockPinnedClipboardItemProvider = new Mock<IPinnedClipboardItemProvider>(MockBehavior.Strict);
            mockPinnedClipboardItemProvider
                .Setup(mock => mock.TryAddPinnedClipboardHistoryItem(
                    It.Is<IClipboardHistoryItemWrapper>(item => item == expectedItems[index]),
                    out It.Ref<string?>.IsAny))
                .Callback((IClipboardHistoryItemWrapper _, out string ? errorMessage) =>
                {
                    errorMessage = null;
                })
                .Returns(true);

            var clipboardManager = new ClipboardManager(mockWinRtClipboard.Object, mockWin32Clipboard.Object, mockPinnedClipboardItemProvider.Object);

            // Act
            await clipboardManager.PinClipboardItemAsync(index);
        }

        [Test]
        [TestCase(-1)]
        [TestCase(1)]
        public void PinClipboardItemAsync_InvalidIndex_ThrowsException(int index)
        {
            // Arrange
            var expectedExceptionErrorCode = ErrorCode.NotFound;
            var expectedExceptionMessage = "Failed to get specified clipboard history item";
            var mockClipboardItem = new Mock<IClipboardHistoryItemWrapper>(MockBehavior.Strict);
            var expectedItems = new List<IClipboardHistoryItemWrapper> { mockClipboardItem.Object };
            var mockClipboardItemsResult = new Mock<IClipboardHistoryItemsResultWrapper>(MockBehavior.Strict);
            mockClipboardItemsResult.SetupGet(mock => mock.Status).Returns(ClipboardHistoryItemsResultStatus.Success);
            mockClipboardItemsResult.SetupGet(mock => mock.Items).Returns(expectedItems);

            var mockWin32Clipboard = new Mock<IWin32ClipboardWrapper>(MockBehavior.Strict);
            var mockWinRtClipboard = new Mock<IWinRtClipboardWrapper>(MockBehavior.Strict);
            mockWinRtClipboard.Setup(mock => mock.GetHistoryItemsAsync()).ReturnsAsync(mockClipboardItemsResult.Object);

            var mockPinnedClipboardItemProvider = new Mock<IPinnedClipboardItemProvider>(MockBehavior.Strict);

            var clipboardManager = new ClipboardManager(mockWinRtClipboard.Object, mockWin32Clipboard.Object, mockPinnedClipboardItemProvider.Object);

            // Act + Assert
            var actualException = Assert.ThrowsAsync<PastException>(() => clipboardManager.PinClipboardItemAsync(index));
            Assert.That(actualException.Message, Is.EqualTo(expectedExceptionMessage));
            Assert.That(actualException.ErrorCode, Is.EqualTo(expectedExceptionErrorCode));
        }

        [Test]
        public void PinClipboardItemAsync_EmptyClipboardHistory_ThrowsArgumentOutOfBoundsException()
        {
            // Arrange
            var expectedExceptionErrorCode = ErrorCode.NotFound;
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
            var actualException = Assert.ThrowsAsync<PastException>(() => clipboardManager.PinClipboardItemAsync(0));
            Assert.That(actualException.Message, Is.EqualTo(expectedExceptionMessage));
            Assert.That(actualException.ErrorCode, Is.EqualTo(expectedExceptionErrorCode));
        }

        [Test]
        public async Task PinClipboardItemAsync_ValidId_Success()
        {
            // Arrange
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
            mockWinRtClipboard.Setup(mock => mock.GetHistoryItemsAsync()).ReturnsAsync(mockClipboardItemsResult.Object);

            var mockPinnedClipboardItemProvider = new Mock<IPinnedClipboardItemProvider>(MockBehavior.Strict);
            mockPinnedClipboardItemProvider
                .Setup(mock => mock.TryAddPinnedClipboardHistoryItem(
                    It.Is<IClipboardHistoryItemWrapper>(item => item == expectedItem),
                    out It.Ref<string?>.IsAny))
                .Callback((IClipboardHistoryItemWrapper _, out string? errorMessage) =>
                {
                    errorMessage = null;
                })
                .Returns(true);

            var clipboardManager = new ClipboardManager(mockWinRtClipboard.Object, mockWin32Clipboard.Object, mockPinnedClipboardItemProvider.Object);

            // Act
            await clipboardManager.PinClipboardItemAsync(Guid.Parse(expectedItem.Id));
        }

        [Test]
        public void PinClipboardItemAsync_InvalidId_ThrowsException()
        {
            // Arrange
            var expectedExceptionErrorCode = ErrorCode.NotFound;
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
            var actualException = Assert.ThrowsAsync<PastException>(() => clipboardManager.PinClipboardItemAsync(Guid.NewGuid()));
            Assert.That(actualException.Message, Is.EqualTo(expectedExceptionMessage));
            Assert.That(actualException.ErrorCode, Is.EqualTo(expectedExceptionErrorCode));
        }

        [Test]
        [TestCase(ClipboardHistoryItemsResultStatus.AccessDenied)]
        [TestCase(ClipboardHistoryItemsResultStatus.ClipboardHistoryDisabled)]
        public void PinClipboardItemAsync_FailToGetClipboardHistory_ThrowsException(ClipboardHistoryItemsResultStatus clipboardHistoryItemsResultStatus)
        {
            // Arrange
            var expectedExceptionErrorCode = clipboardHistoryItemsResultStatus.ToErrorCode();
            var expectedExceptionMessage = $"Failed to get clipboard history. Result: {clipboardHistoryItemsResultStatus}";
            var mockClipboardItemsResult = new Mock<IClipboardHistoryItemsResultWrapper>(MockBehavior.Strict);
            mockClipboardItemsResult.SetupGet(mock => mock.Status).Returns(clipboardHistoryItemsResultStatus);

            var mockWin32Clipboard = new Mock<IWin32ClipboardWrapper>(MockBehavior.Strict);
            var mockWinRtClipboard = new Mock<IWinRtClipboardWrapper>(MockBehavior.Strict);
            mockWinRtClipboard.Setup(mock => mock.GetHistoryItemsAsync()).ReturnsAsync(mockClipboardItemsResult.Object);
            var mockPinnedClipboardProvider = new Mock<IPinnedClipboardItemProvider>(MockBehavior.Strict);

            var clipboardManager = new ClipboardManager(mockWinRtClipboard.Object, mockWin32Clipboard.Object, mockPinnedClipboardProvider.Object);

            // Act + Assert
            var actualException = Assert.ThrowsAsync<PastException>(() => clipboardManager.PinClipboardItemAsync(0));
            Assert.That(actualException.Message, Is.EqualTo(expectedExceptionMessage));
            Assert.That(actualException.ErrorCode, Is.EqualTo(expectedExceptionErrorCode));
        }

        [Test]
        public void PinClipboardItemAsync_PinningFailed_ThrowsException()
        {
            // Arrange
            var expectedExceptionErrorCode = ErrorCode.NotFound;
            var expectedExceptionMessage = "Uh-oh spaghettio!";
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
            mockWinRtClipboard.Setup(mock => mock.GetHistoryItemsAsync()).ReturnsAsync(mockClipboardItemsResult.Object);

            var mockPinnedClipboardItemProvider = new Mock<IPinnedClipboardItemProvider>(MockBehavior.Strict);
            mockPinnedClipboardItemProvider
                .Setup(mock => mock.TryAddPinnedClipboardHistoryItem(
                    It.Is<IClipboardHistoryItemWrapper>(item => item == mockClipboardItem.Object),
                    out It.Ref<string?>.IsAny))
                .Callback((IClipboardHistoryItemWrapper _, out string? errorMessage) =>
                {
                    errorMessage = expectedExceptionMessage;
                })
                .Returns(false);

            var clipboardManager = new ClipboardManager(mockWinRtClipboard.Object, mockWin32Clipboard.Object, mockPinnedClipboardItemProvider.Object);

            // Act + Assert
            var actualException = Assert.ThrowsAsync<PastException>(() => clipboardManager.PinClipboardItemAsync(Guid.Parse(mockClipboardItem.Object.Id)));
            Assert.That(actualException.Message, Is.EqualTo(expectedExceptionMessage));
            Assert.That(actualException.ErrorCode, Is.EqualTo(expectedExceptionErrorCode));
        }

        [Test]
        public void PinClipboardItemAsync_NullIdentifier_ThrowsArgumentNullException()
        {
            // Arrange
            var expectedExceptionMessage = "Value cannot be null. (Parameter 'identifier')";

            var mockWin32Clipboard = new Mock<IWin32ClipboardWrapper>(MockBehavior.Strict);
            var mockWinRtClipboard = new Mock<IWinRtClipboardWrapper>(MockBehavior.Strict);
            var mockPinnedClipboardItemProvider = new Mock<IPinnedClipboardItemProvider>(MockBehavior.Strict);

            var clipboardManager = new ClipboardManager(mockWinRtClipboard.Object, mockWin32Clipboard.Object, mockPinnedClipboardItemProvider.Object);

            // Act + Assert
            var actualException = Assert.ThrowsAsync<ArgumentNullException>(() => clipboardManager.PinClipboardItemAsync(null!));
            Assert.That(actualException.Message, Is.EqualTo(expectedExceptionMessage));
            Assert.That(actualException.ParamName, Is.EqualTo("identifier"));
        }
        #endregion PinClipboardItemAsync

        #region UnpinClipboardItemAsync
        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public async Task UnpinClipboardItemAsync_ValidIndex_Success(int index)
        {
            // Arrange
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
            mockWinRtClipboard.Setup(mock => mock.GetHistoryItemsAsync()).ReturnsAsync(mockClipboardItemsResult.Object);

            var mockPinnedClipboardItemProvider = new Mock<IPinnedClipboardItemProvider>(MockBehavior.Strict);
            mockPinnedClipboardItemProvider
                .Setup(mock => mock.TryRemovePinnedClipboardHistoryItem(
                    It.Is<IClipboardHistoryItemWrapper>(item => item == expectedItems[index]),
                    out It.Ref<string?>.IsAny))
                .Callback((IClipboardHistoryItemWrapper _, out string? errorMessage) =>
                {
                    errorMessage = null;
                })
                .Returns(true);

            var clipboardManager = new ClipboardManager(mockWinRtClipboard.Object, mockWin32Clipboard.Object, mockPinnedClipboardItemProvider.Object);

            // Act
            await clipboardManager.UnpinClipboardItemAsync(index);
        }

        [Test]
        [TestCase(-1)]
        [TestCase(1)]
        public void UnpinClipboardItemAsync_InvalidIndex_ThrowsException(int index)
        {
            // Arrange
            var expectedExceptionErrorCode = ErrorCode.NotFound;
            var expectedExceptionMessage = "Failed to get specified clipboard history item";
            var mockClipboardItem = new Mock<IClipboardHistoryItemWrapper>(MockBehavior.Strict);
            var expectedItems = new List<IClipboardHistoryItemWrapper> { mockClipboardItem.Object };
            var mockClipboardItemsResult = new Mock<IClipboardHistoryItemsResultWrapper>(MockBehavior.Strict);
            mockClipboardItemsResult.SetupGet(mock => mock.Status).Returns(ClipboardHistoryItemsResultStatus.Success);
            mockClipboardItemsResult.SetupGet(mock => mock.Items).Returns(expectedItems);

            var mockWin32Clipboard = new Mock<IWin32ClipboardWrapper>(MockBehavior.Strict);
            var mockWinRtClipboard = new Mock<IWinRtClipboardWrapper>(MockBehavior.Strict);
            mockWinRtClipboard.Setup(mock => mock.GetHistoryItemsAsync()).ReturnsAsync(mockClipboardItemsResult.Object);

            var mockPinnedClipboardItemProvider = new Mock<IPinnedClipboardItemProvider>(MockBehavior.Strict);

            var clipboardManager = new ClipboardManager(mockWinRtClipboard.Object, mockWin32Clipboard.Object, mockPinnedClipboardItemProvider.Object);

            // Act + Assert
            var actualException = Assert.ThrowsAsync<PastException>(() => clipboardManager.UnpinClipboardItemAsync(index));
            Assert.That(actualException.Message, Is.EqualTo(expectedExceptionMessage));
            Assert.That(actualException.ErrorCode, Is.EqualTo(expectedExceptionErrorCode));
        }

        [Test]
        public void UnpinClipboardItemAsync_EmptyClipboardHistory_ThrowsArgumentOutOfBoundsException()
        {
            // Arrange
            var expectedExceptionErrorCode = ErrorCode.NotFound;
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
            var actualException = Assert.ThrowsAsync<PastException>(() => clipboardManager.UnpinClipboardItemAsync(0));
            Assert.That(actualException.Message, Is.EqualTo(expectedExceptionMessage));
            Assert.That(actualException.ErrorCode, Is.EqualTo(expectedExceptionErrorCode));
        }

        [Test]
        public async Task UnpinClipboardItemAsync_ValidId_Success()
        {
            // Arrange
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
            mockWinRtClipboard.Setup(mock => mock.GetHistoryItemsAsync()).ReturnsAsync(mockClipboardItemsResult.Object);

            var mockPinnedClipboardItemProvider = new Mock<IPinnedClipboardItemProvider>(MockBehavior.Strict);
            mockPinnedClipboardItemProvider
                .Setup(mock => mock.TryRemovePinnedClipboardHistoryItem(
                    It.Is<IClipboardHistoryItemWrapper>(item => item == expectedItem),
                    out It.Ref<string?>.IsAny))
                .Callback((IClipboardHistoryItemWrapper _, out string? errorMessage) =>
                {
                    errorMessage = null;
                })
                .Returns(true);

            var clipboardManager = new ClipboardManager(mockWinRtClipboard.Object, mockWin32Clipboard.Object, mockPinnedClipboardItemProvider.Object);

            // Act
            await clipboardManager.UnpinClipboardItemAsync(Guid.Parse(expectedItem.Id));
        }

        [Test]
        public void UnpinClipboardItemAsync_InvalidId_ThrowsException()
        {
            // Arrange
            var expectedExceptionErrorCode = ErrorCode.NotFound;
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
            var actualException = Assert.ThrowsAsync<PastException>(() => clipboardManager.UnpinClipboardItemAsync(Guid.NewGuid()));
            Assert.That(actualException.Message, Is.EqualTo(expectedExceptionMessage));
            Assert.That(actualException.ErrorCode, Is.EqualTo(expectedExceptionErrorCode));
        }

        [Test]
        [TestCase(ClipboardHistoryItemsResultStatus.AccessDenied)]
        [TestCase(ClipboardHistoryItemsResultStatus.ClipboardHistoryDisabled)]
        public void UnpinClipboardItemAsync_FailToGetClipboardHistory_ThrowsException(ClipboardHistoryItemsResultStatus clipboardHistoryItemsResultStatus)
        {
            // Arrange
            var expectedExceptionErrorCode = clipboardHistoryItemsResultStatus.ToErrorCode();
            var expectedExceptionMessage = $"Failed to get clipboard history. Result: {clipboardHistoryItemsResultStatus}";
            var mockClipboardItemsResult = new Mock<IClipboardHistoryItemsResultWrapper>(MockBehavior.Strict);
            mockClipboardItemsResult.SetupGet(mock => mock.Status).Returns(clipboardHistoryItemsResultStatus);

            var mockWin32Clipboard = new Mock<IWin32ClipboardWrapper>(MockBehavior.Strict);
            var mockWinRtClipboard = new Mock<IWinRtClipboardWrapper>(MockBehavior.Strict);
            mockWinRtClipboard.Setup(mock => mock.GetHistoryItemsAsync()).ReturnsAsync(mockClipboardItemsResult.Object);
            var mockPinnedClipboardProvider = new Mock<IPinnedClipboardItemProvider>(MockBehavior.Strict);

            var clipboardManager = new ClipboardManager(mockWinRtClipboard.Object, mockWin32Clipboard.Object, mockPinnedClipboardProvider.Object);

            // Act + Assert
            var actualException = Assert.ThrowsAsync<PastException>(() => clipboardManager.UnpinClipboardItemAsync(0));
            Assert.That(actualException.Message, Is.EqualTo(expectedExceptionMessage));
            Assert.That(actualException.ErrorCode, Is.EqualTo(expectedExceptionErrorCode));
        }

        [Test]
        public void UnpinClipboardItemAsync_PinningFailed_ThrowsException()
        {
            // Arrange
            var expectedExceptionErrorCode = ErrorCode.NotFound;
            var expectedExceptionMessage = "Uh-oh spaghettio!";
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
            mockWinRtClipboard.Setup(mock => mock.GetHistoryItemsAsync()).ReturnsAsync(mockClipboardItemsResult.Object);

            var mockPinnedClipboardItemProvider = new Mock<IPinnedClipboardItemProvider>(MockBehavior.Strict);
            mockPinnedClipboardItemProvider
                .Setup(mock => mock.TryRemovePinnedClipboardHistoryItem(
                    It.Is<IClipboardHistoryItemWrapper>(item => item == mockClipboardItem.Object),
                    out It.Ref<string?>.IsAny))
                .Callback((IClipboardHistoryItemWrapper _, out string? errorMessage) =>
                {
                    errorMessage = expectedExceptionMessage;
                })
                .Returns(false);

            var clipboardManager = new ClipboardManager(mockWinRtClipboard.Object, mockWin32Clipboard.Object, mockPinnedClipboardItemProvider.Object);

            // Act + Assert
            var actualException = Assert.ThrowsAsync<PastException>(() => clipboardManager.UnpinClipboardItemAsync(Guid.Parse(mockClipboardItem.Object.Id)));
            Assert.That(actualException.Message, Is.EqualTo(expectedExceptionMessage));
            Assert.That(actualException.ErrorCode, Is.EqualTo(expectedExceptionErrorCode));
        }

        [Test]
        public void UnpinClipboardItemAsync_NullIdentifier_ThrowsArgumentNullException()
        {
            // Arrange
            var expectedExceptionMessage = "Value cannot be null. (Parameter 'identifier')";

            var mockWin32Clipboard = new Mock<IWin32ClipboardWrapper>(MockBehavior.Strict);
            var mockWinRtClipboard = new Mock<IWinRtClipboardWrapper>(MockBehavior.Strict);
            var mockPinnedClipboardItemProvider = new Mock<IPinnedClipboardItemProvider>(MockBehavior.Strict);

            var clipboardManager = new ClipboardManager(mockWinRtClipboard.Object, mockWin32Clipboard.Object, mockPinnedClipboardItemProvider.Object);

            // Act + Assert
            var actualException = Assert.ThrowsAsync<ArgumentNullException>(() => clipboardManager.UnpinClipboardItemAsync(null!));
            Assert.That(actualException.Message, Is.EqualTo(expectedExceptionMessage));
            Assert.That(actualException.ParamName, Is.EqualTo("identifier"));
        }
        #endregion UnpinClipboardItemAsync

        #region SetHistoryItemAsContent
        [Test]
        [TestCase(SetHistoryItemAsContentStatus.Success)]
        [TestCase(SetHistoryItemAsContentStatus.AccessDenied)]
        [TestCase(SetHistoryItemAsContentStatus.ItemDeleted)]
        public void SetHistoryItemAsContent_ReturnsExpectedValue(SetHistoryItemAsContentStatus expectedStatus)
        {
            // Arrange
            var mockClipboardItem = new Mock<IClipboardHistoryItemWrapper>(MockBehavior.Strict);

            var mockWin32Clipboard = new Mock<IWin32ClipboardWrapper>(MockBehavior.Strict);
            var mockWinRtClipboard = new Mock<IWinRtClipboardWrapper>(MockBehavior.Strict);
            mockWinRtClipboard
                .Setup(mock => mock.SetHistoryItemAsContent(
                    It.Is<IClipboardHistoryItemWrapper>(actualClipboardItem => actualClipboardItem == mockClipboardItem.Object)))
                .Returns(expectedStatus);

            var mockPinnedClipboardProvider = new Mock<IPinnedClipboardItemProvider>(MockBehavior.Strict);
            var clipboardManager = new ClipboardManager(mockWinRtClipboard.Object, mockWin32Clipboard.Object, mockPinnedClipboardProvider.Object);

            // Act
            var actualStatus = clipboardManager.SetHistoryItemAsContent(mockClipboardItem.Object);

            // Assert
            Assert.That(actualStatus, Is.EqualTo(expectedStatus));
        }
        #endregion SetHistoryItemAsContent

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
