using Moq;
using past.Core;

namespace past.ConsoleApp.Test
{
    public class ConsoleClipboardTests
    {
        #region Constructors
        [Test]
        public void Constructor_Parameterless_Success()
        {
            Assert.Fail("Implement Me!");
        }

        [Test]
        public void Constructor_NonNullClipboardManager_Success()
        {
            Assert.Fail("Implement Me!");
        }

        [Test]
        public void Constructor_NullClipboardManager_ThrowsArgumentNullException()
        {
            Assert.Fail("Implement Me!");
        }
        #endregion Constructors

        #region GetClipboardHistoryStatus
        [Test]
        public void GetClipboardHistoryStatus_ValidParameters_WritesStatusAndSetsExitCode()
        {
            Assert.Fail("Implement Me!");
        }

        [Test]
        public void GetClipboardHistoryStatus_IsHistoryEnabledThrowsWithQuietFalse_WritesErrorAndSetsExitCode()
        {
            Assert.Fail("Implement Me!");
        }

        [Test]
        public void GetClipboardHistoryStatus_IsRoamingEnabledThrowsWithQuietFalse_SetsExitCode()
        {
            Assert.Fail("Implement Me!");
        }

        [Test]
        public void GetClipboardHistoryStatus_IsHistoryEnabledThrowsWithQuietTrue_SetsExitCode()
        {
            Assert.Fail("Implement Me!");
        }

        [Test]
        public void GetClipboardHistoryStatus_IsRoamingEnabledThrowsWithQuietTrue_WritesErrorAndSetsExitCode()
        {
            Assert.Fail("Implement Me!");
        }

        [Test]
        public void GetClipboardHistoryStatus_NullInvocationContext_ThrowsArgumentNullException()
        {
            Assert.Fail("Implement Me!");
        }

        [Test]
        public void GetClipboardHistoryStatus_NullConsole_ThrowsArgumentNullException()
        {
            Assert.Fail("Implement Me!");
        }
        #endregion GetClipboardHistoryStatus

        #region GetCurrentClipboardValueAsync
        [Test]
        [TestCase(ContentType.Text)]
        [TestCase(ContentType.Image)]
        [TestCase(ContentType.All)]
        public async Task GetCurrentClipboardValueAsync_ValidParameters_GetsAndWritesItemReturnsSuccess(ContentType expectedType)
        {
            // Arrange
            var expectedReturnValue = 0;
            var expectedCancellationTokenSource = new CancellationTokenSource();
            var expectedValue = "Copyin' copyin', yeah!";

            var mockClipboardManager = new Mock<IClipboardManager>(MockBehavior.Strict);
            mockClipboardManager
                .Setup(mock => mock.GetCurrentClipboardValueAsync(
                    It.Is<ContentType>(actualType => actualType == expectedType),
                    It.Is<CancellationToken>(actualCancellationToken => actualCancellationToken == expectedCancellationTokenSource.Token)))
                .ReturnsAsync(expectedValue)
                .Verifiable();

            var mockConsoleWriter = new Mock<IConsoleWriter>(MockBehavior.Strict);
            mockConsoleWriter
                .Setup(mock => mock.WriteValue(It.Is<string>(actualValue => actualValue == expectedValue)))
                .Verifiable();

            var consoleClipboard = new ConsoleClipboard(mockClipboardManager.Object);

            // Act
            var actualReturnValue = await consoleClipboard.GetCurrentClipboardValueAsync(mockConsoleWriter.Object, expectedType, expectedCancellationTokenSource.Token);

            // Assert
            Assert.That(actualReturnValue, Is.EqualTo(expectedReturnValue));
            mockClipboardManager.Verify();
            mockConsoleWriter.Verify();
        }

        [Test]
        [TestCase(ContentType.Text)]
        [TestCase(ContentType.Image)]
        [TestCase(ContentType.All)]
        public async Task GetCurrentClipboardValueAsync_GetValueThrows_WritesErrorAndReturnsErrorCode(ContentType expectedType)
        {
            // Arrange
            var expectedReturnValue = -1;
            var expectedCancellationTokenSource = new CancellationTokenSource();
            var expectedException = new Exception("Oh no! :O");
            var expectedErrorMessage = $"Failed to get current clipboard contents. Error: {expectedException.Message}";

            var mockClipboardManager = new Mock<IClipboardManager>(MockBehavior.Strict);
            mockClipboardManager
                .Setup(mock => mock.GetCurrentClipboardValueAsync(
                    It.Is<ContentType>(actualType => actualType == expectedType),
                    It.Is<CancellationToken>(actualCancellationToken => actualCancellationToken == expectedCancellationTokenSource.Token)))
                .ThrowsAsync(expectedException)
                .Verifiable();

            var mockConsoleWriter = new Mock<IConsoleWriter>(MockBehavior.Strict);
            mockConsoleWriter
                .Setup(mock => mock.WriteErrorLine(It.Is<string>(actualMessage => actualMessage == expectedErrorMessage)))
                .Verifiable();

            var consoleClipboard = new ConsoleClipboard(mockClipboardManager.Object);

            // Act
            var actualReturnValue = await consoleClipboard.GetCurrentClipboardValueAsync(mockConsoleWriter.Object, expectedType, expectedCancellationTokenSource.Token);

            // Assert
            Assert.That(actualReturnValue, Is.EqualTo(expectedReturnValue));
            mockClipboardManager.Verify();
            mockConsoleWriter.Verify();
        }

        [Test]
        [TestCase(ContentType.Text)]
        [TestCase(ContentType.Image)]
        [TestCase(ContentType.All)]
        public async Task GetCurrentClipboardValueAsync_WriteValueThrows_WritesErrorAndReturnsErrorCode(ContentType expectedType)
        {
            // Arrange
            var expectedReturnValue = -1;
            var expectedCancellationTokenSource = new CancellationTokenSource();
            var expectedException = new Exception("Oh no! :O");
            var expectedErrorMessage = $"Failed to get current clipboard contents. Error: {expectedException.Message}";
            var expectedValue = "Copyin' copyin', yeah!";


            var mockClipboardManager = new Mock<IClipboardManager>(MockBehavior.Strict);
            mockClipboardManager
                .Setup(mock => mock.GetCurrentClipboardValueAsync(
                    It.Is<ContentType>(actualType => actualType == expectedType),
                    It.Is<CancellationToken>(actualCancellationToken => actualCancellationToken == expectedCancellationTokenSource.Token)))
                .ReturnsAsync(expectedValue)
                .Verifiable();

            var mockConsoleWriter = new Mock<IConsoleWriter>(MockBehavior.Strict);
            mockConsoleWriter
                .Setup(mock => mock.WriteValue(It.Is<string>(actualValue => actualValue == expectedValue)))
                .Throws(expectedException)
                .Verifiable();

            mockConsoleWriter
                .Setup(mock => mock.WriteErrorLine(It.Is<string>(actualMessage => actualMessage == expectedErrorMessage)))
                .Verifiable();

            var consoleClipboard = new ConsoleClipboard(mockClipboardManager.Object);

            // Act
            var actualReturnValue = await consoleClipboard.GetCurrentClipboardValueAsync(mockConsoleWriter.Object, expectedType, expectedCancellationTokenSource.Token);

            // Assert
            Assert.That(actualReturnValue, Is.EqualTo(expectedReturnValue));
            mockClipboardManager.Verify();
            mockConsoleWriter.Verify();
        }

        [Test]
        public void GetCurrentClipboardValueAsync_NullConsoleWriter_ThrowsArgumentNullException()
        {
            // Arrange
            var cancellationTokenSource = new CancellationTokenSource();
            var mockClipboardManager = new Mock<IClipboardManager>(MockBehavior.Strict);
            var consoleClipboard = new ConsoleClipboard(mockClipboardManager.Object);

            // Act + Assert
            Assert.ThrowsAsync<ArgumentNullException>(() => consoleClipboard.GetCurrentClipboardValueAsync(null!, ContentType.Text, cancellationTokenSource.Token));
        }
        #endregion GetCurrentClipboardValueAsync

        #region GetClipboardHistoryItemAsync
        // TODO: Add remaining test stubs for GetClipboardHistoryItemAsync
        [Test]
        public void GetClipboardHistoryItemAsync_AnsiTrue_EnablesVirtualTerminalProcessing()
        {
            Assert.Fail("Implement Me!");
        }

        [Test]
        public void GetClipboardHistoryItemAsync_FailToEnableVirtualTerminalProcessing_WritesErrorAndExpectedItem()
        {
            Assert.Fail("Implement Me!");
        }
        #endregion GetClipboardHistoryItemAsync

        #region ListClipboardHistoryAsync
        // TODO: Add test stubs for ListClipboardHistoryAsync
        #endregion ListClipboardHistoryAsync
    }
}
