using Moq;
using past.Core.Extensions;
using past.Core.Wrappers;
using Windows.ApplicationModel.DataTransfer;

namespace past.Core.Test
{
    public class ClipboardHistoryItemsResultWrapperExtensionsTests
    {
        #region TryGetItem
        [Test]
        public void TryGetItem_ValidIndexIdentifier_ReturnsTrueAndExpectedItem()
        {
            // Arrange
            var mockItems = new List<IClipboardHistoryItemWrapper>();
            for (int i = 0; i < 3; i++)
            {
                var mockClipboardItem = new Mock<IClipboardHistoryItemWrapper>(MockBehavior.Strict);
                mockItems.Add(mockClipboardItem.Object);
            }

            var mockClipboardItemsResult = new Mock<IClipboardHistoryItemsResultWrapper>(MockBehavior.Strict);
            mockClipboardItemsResult.SetupGet(mock => mock.Status).Returns(ClipboardHistoryItemsResultStatus.Success);
            mockClipboardItemsResult.SetupGet(mock => mock.Items).Returns(mockItems);

            int index = 1;
            var expectedItem = mockItems[index];
            var identifier = new ClipboardItemIdentifier(index);

            // Act
            var result = mockClipboardItemsResult.Object.TryGetItem(identifier, out var actualItem);

            // Assert
            Assert.That(result, Is.True);
            Assert.That(actualItem, Is.EqualTo(expectedItem));
        }

        [Test]
        public void TryGetItem_InvalidIndexIdentifier_ReturnsFalseAndNullItem()
        {
            // Arrange
            var mockClipboardItem = new Mock<IClipboardHistoryItemWrapper>(MockBehavior.Strict);
            var mockItems = new List<IClipboardHistoryItemWrapper> { mockClipboardItem.Object };

            var mockClipboardItemsResult = new Mock<IClipboardHistoryItemsResultWrapper>(MockBehavior.Strict);
            mockClipboardItemsResult.SetupGet(mock => mock.Status).Returns(ClipboardHistoryItemsResultStatus.Success);
            mockClipboardItemsResult.SetupGet(mock => mock.Items).Returns(mockItems);

            var identifier = new ClipboardItemIdentifier(1);

            // Act
            var result = mockClipboardItemsResult.Object.TryGetItem(identifier, out var actualItem);

            // Assert
            Assert.That(result, Is.False);
            Assert.That(actualItem, Is.Null);
        }

        [Test]
        public void TryGetItem_ValidGuidIdentifier_ReturnsTrueAndExpectedItem()
        {
            // Arrange
            var mockItems = new List<IClipboardHistoryItemWrapper>();
            for (int i = 0; i < 3; i++)
            {
                var mockClipboardItem = new Mock<IClipboardHistoryItemWrapper>(MockBehavior.Strict);
                mockClipboardItem.SetupGet(mock => mock.Id).Returns(Guid.NewGuid().ToString());
                mockItems.Add(mockClipboardItem.Object);
            }

            var mockClipboardItemsResult = new Mock<IClipboardHistoryItemsResultWrapper>(MockBehavior.Strict);
            mockClipboardItemsResult.SetupGet(mock => mock.Status).Returns(ClipboardHistoryItemsResultStatus.Success);
            mockClipboardItemsResult.SetupGet(mock => mock.Items).Returns(mockItems);

            var expectedItem = mockItems[1];
            var identifier = new ClipboardItemIdentifier(Guid.Parse(expectedItem.Id));

            // Act
            var result = mockClipboardItemsResult.Object.TryGetItem(identifier, out var actualItem);

            // Assert
            Assert.That(result, Is.True);
            Assert.That(actualItem, Is.EqualTo(expectedItem));
        }

        [Test]
        public void TryGetItem_InvalidGuidIdentifier_ReturnsFalseAndNullItem()
        {
            // Arrange
            var mockClipboardItem = new Mock<IClipboardHistoryItemWrapper>(MockBehavior.Strict);
            mockClipboardItem.SetupGet(mock => mock.Id).Returns(Guid.NewGuid().ToString());
            var mockItems = new List<IClipboardHistoryItemWrapper> { mockClipboardItem.Object };

            var mockClipboardItemsResult = new Mock<IClipboardHistoryItemsResultWrapper>(MockBehavior.Strict);
            mockClipboardItemsResult.SetupGet(mock => mock.Status).Returns(ClipboardHistoryItemsResultStatus.Success);
            mockClipboardItemsResult.SetupGet(mock => mock.Items).Returns(mockItems);

            var identifier = new ClipboardItemIdentifier(Guid.NewGuid());

            // Act
            var result = mockClipboardItemsResult.Object.TryGetItem(identifier, out var actualItem);

            // Assert
            Assert.That(result, Is.False);
            Assert.That(actualItem, Is.Null);
        }

        [Test]
        [TestCase(ClipboardHistoryItemsResultStatus.AccessDenied)]
        [TestCase(ClipboardHistoryItemsResultStatus.ClipboardHistoryDisabled)]
        public void TryGetItem_NonSuccessClipboardHistoryItemsResult_ReturnsFalseAndNullItem(ClipboardHistoryItemsResultStatus clipboardHistoryItemsResultStatus)
        {
            // Arrange
            var mockClipboardItemsResult = new Mock<IClipboardHistoryItemsResultWrapper>(MockBehavior.Strict);
            mockClipboardItemsResult.SetupGet(mock => mock.Status).Returns(clipboardHistoryItemsResultStatus);

            var identifier = new ClipboardItemIdentifier(0);

            // Act
            var result = mockClipboardItemsResult.Object.TryGetItem(identifier, out var actualItem);

            // Assert
            Assert.That(result, Is.False);
            Assert.That(actualItem, Is.Null);
        }

        [Test]
        public void TryGetItem_NullClipboardHistoryItemsResult_ThrowsArgumentNullException()
        {
            // Arrange
            var identifier = new ClipboardItemIdentifier(0);
            IClipboardHistoryItemsResultWrapper clipboardItemsResult = null!;

            // Act + Assert
            Assert.Throws<ArgumentNullException>(() => clipboardItemsResult.TryGetItem(identifier, out _));
        }

        [Test]
        public void TryGetItem_NullIdentifier_ThrowsArgumentNullException()
        {
            // Arrange
            ClipboardItemIdentifier identifier = null!;
            var mockClipboardItemsResult = new Mock<IClipboardHistoryItemsResultWrapper>(MockBehavior.Strict);

            // Act + Assert
            Assert.Throws<ArgumentNullException>(() => mockClipboardItemsResult.Object.TryGetItem(identifier, out _));
        }
        #endregion TryGetItem
    }
}
