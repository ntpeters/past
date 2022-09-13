using Moq;
using past.ConsoleApp.Output;
using past.Core;
using past.Core.Models;
using past.Core.Wrappers;
using System.CommandLine;
using System.CommandLine.Invocation;
using Windows.ApplicationModel.DataTransfer;

namespace past.ConsoleApp.Test
{
    public class ConsoleClipboardTests
    {
        #region Constructors
        [Test]
        public void Constructor_Parameterless_Success()
        {
            Assert.DoesNotThrow(() => new ConsoleClipboard());
        }

        [Test]
        public void Constructor_NonNullClipboardManager_Success()
        {
            var mockClipboardManager = new Mock<IClipboardManager>(MockBehavior.Strict);
            Assert.DoesNotThrow( () => new ConsoleClipboard(mockClipboardManager.Object));
        }

        [Test]
        public void Constructor_NullClipboardManager_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ConsoleClipboard(null!));
        }
        #endregion Constructors

        #region GetClipboardHistoryStatus
        [Test]
        [TestCase(true, true)]
        [TestCase(true, false)]
        [TestCase(false, true)]
        [TestCase(false, false)]
        public void GetClipboardHistoryStatus_ValidParameters_WritesStatusAndSetsExitCode(bool expectedHistoryEnabled, bool expectedRoamingEnabled)
        {
            // Arrange
            int expectedExitCode = 0;
            var expectedHistoryMessage = $"Clipboard History Enabled: {expectedHistoryEnabled}";
            var expectedRoamingMessage = $"Clipboard Roaming Enabled: {expectedRoamingEnabled}";

            // Unused command since this is the only way to get an invocation context for testing
            var dummyCommand = new Command("test");
            var testContext = new InvocationContext(dummyCommand.Parse());

            var actualMessages = new List<string>();
            var mockConsoleWriter = new Mock<IConsoleWriter>();
            mockConsoleWriter
                .Setup(mock => mock.WriteLine(It.IsAny<string>()))
                .Callback<string>(actualMessage => actualMessages.Add(actualMessage))
                .Verifiable();

            var mockClipboardManager = new Mock<IClipboardManager>(MockBehavior.Strict);
            mockClipboardManager
                .Setup(mock => mock.IsHistoryEnabled())
                .Returns(expectedHistoryEnabled)
                .Verifiable();

            mockClipboardManager
                .Setup(mock => mock.IsRoamingEnabled())
                .Returns(expectedRoamingEnabled)
                .Verifiable();

            var consoleClipboard = new ConsoleClipboard(mockClipboardManager.Object);

            // Act
            consoleClipboard.GetClipboardHistoryStatus(mockConsoleWriter.Object, testContext);

            // Assert
            Assert.That(actualMessages.Count, Is.EqualTo(2));
            Assert.That(actualMessages[0], Is.EqualTo(expectedHistoryMessage));
            Assert.That(actualMessages[1], Is.EqualTo(expectedRoamingMessage));
            Assert.That(testContext.ExitCode, Is.EqualTo(expectedExitCode));
        }

        [Test]
        public void GetClipboardHistoryStatus_IsHistoryEnabledThrowsPastException_WritesErrorAndSetsErrorCode()
        {
            // Arrange
            var expectedErrorCode = ErrorCode.AccessDenied;
            int expectedExitCode = (int)expectedErrorCode;
            var expectedException = new PastException(expectedErrorCode, "Oh no! :o");
            var expectedErrorMessage = $"Failed to get current clipboard history status. Error: {expectedException.Message}";

            var testContext = new InvocationContext(new Command("test").Parse());

            var mockConsoleWriter = new Mock<IConsoleWriter>();

            mockConsoleWriter
                .Setup(mock => mock.WriteErrorLine(It.Is<string>(actualErrorMessage => actualErrorMessage == expectedErrorMessage)))
                .Verifiable();

            var mockClipboardManager = new Mock<IClipboardManager>(MockBehavior.Strict);
            mockClipboardManager
                .Setup(mock => mock.IsHistoryEnabled())
                .Throws(expectedException)
                .Verifiable();

            var consoleClipboard = new ConsoleClipboard(mockClipboardManager.Object);

            // Act
            consoleClipboard.GetClipboardHistoryStatus(mockConsoleWriter.Object, testContext);

            // Assert
            Assert.That(testContext.ExitCode, Is.EqualTo(expectedExitCode));
        }

        [Test]
        public void GetClipboardHistoryStatus_IsHistoryEnabledThrowsUnexpectedException_ExceptionUnhandled()
        {
            // Arrange
            var expectedException = new Exception("Oh no! :o");
            var expectedErrorMessage = $"Failed to get current clipboard history status. Error: {expectedException.Message}";

            var testContext = new InvocationContext(new Command("test").Parse());

            var mockConsoleWriter = new Mock<IConsoleWriter>();

            mockConsoleWriter
                .Setup(mock => mock.WriteErrorLine(It.Is<string>(actualErrorMessage => actualErrorMessage == expectedErrorMessage)))
                .Verifiable();

            var mockClipboardManager = new Mock<IClipboardManager>(MockBehavior.Strict);
            mockClipboardManager
                .Setup(mock => mock.IsHistoryEnabled())
                .Throws(expectedException)
                .Verifiable();

            var consoleClipboard = new ConsoleClipboard(mockClipboardManager.Object);

            // Act
            var actualException = Assert.Throws<Exception>(() => consoleClipboard.GetClipboardHistoryStatus(mockConsoleWriter.Object, testContext));

            // Assert
            Assert.That(actualException, Is.EqualTo(expectedException));
        }

        [Test]
        public void GetClipboardHistoryStatus_IsRoamingEnabledThrowsPastException_WritesErrorAndSetsErrorCode()
        {
            // Arrange
            var expectedErrorCode = ErrorCode.AccessDenied;
            int expectedExitCode = (int)expectedErrorCode;
            bool expectedHistoryEnabled = true;
            var expectedHistoryMessage = $"Clipboard History Enabled: {expectedHistoryEnabled}";
            var expectedException = new PastException(expectedErrorCode, "Oh no! :o");
            var expectedErrorMessage = $"Failed to get current clipboard history status. Error: {expectedException.Message}";

            var testContext = new InvocationContext(new Command("test").Parse());

            var mockConsoleWriter = new Mock<IConsoleWriter>();
            mockConsoleWriter
                .Setup(mock => mock.WriteLine(It.Is<string>(actualMessage => actualMessage == expectedHistoryMessage)))
                .Verifiable();

            mockConsoleWriter
                .Setup(mock => mock.WriteErrorLine(It.Is<string>(actualErrorMessage => actualErrorMessage == expectedErrorMessage)))
                .Verifiable();

            var mockClipboardManager = new Mock<IClipboardManager>(MockBehavior.Strict);
            mockClipboardManager
                .Setup(mock => mock.IsHistoryEnabled())
                .Returns(expectedHistoryEnabled)
                .Verifiable();

            mockClipboardManager
                .Setup(mock => mock.IsRoamingEnabled())
                .Throws(expectedException)
                .Verifiable();

            var consoleClipboard = new ConsoleClipboard(mockClipboardManager.Object);

            // Act
            consoleClipboard.GetClipboardHistoryStatus(mockConsoleWriter.Object, testContext);

            // Assert
            Assert.That(testContext.ExitCode, Is.EqualTo(expectedExitCode));
        }

        [Test]
        public void GetClipboardHistoryStatus_IsRoamingEnabledThrowsUnexpectedException_ExceptionUnhandled()
        {
            // Arrange
            bool expectedHistoryEnabled = true;
            var expectedHistoryMessage = $"Clipboard History Enabled: {expectedHistoryEnabled}";
            var expectedException = new Exception("Oh no! :o");
            var expectedErrorMessage = $"Failed to get current clipboard history status. Error: {expectedException.Message}";

            var testContext = new InvocationContext(new Command("test").Parse());

            var mockConsoleWriter = new Mock<IConsoleWriter>();

            var mockClipboardManager = new Mock<IClipboardManager>(MockBehavior.Strict);
            mockClipboardManager
                .Setup(mock => mock.IsHistoryEnabled())
                .Returns(expectedHistoryEnabled)
                .Verifiable();

            mockClipboardManager
                .Setup(mock => mock.IsRoamingEnabled())
                .Throws(expectedException)
                .Verifiable();

            var consoleClipboard = new ConsoleClipboard(mockClipboardManager.Object);

            // Act
            var actualException = Assert.Throws<Exception>(() => consoleClipboard.GetClipboardHistoryStatus(mockConsoleWriter.Object, testContext));

            // Assert
            Assert.That(actualException, Is.EqualTo(expectedException));
        }

        [Test]
        public void GetClipboardHistoryStatus_NullConsoleWriter_ThrowsArgumentNullException()
        {
            // Arrange
            var testContext = new InvocationContext(new Command("test").Parse());
            var mockClipboardManager = new Mock<IClipboardManager>();
            var consoleClipboard = new ConsoleClipboard(mockClipboardManager.Object);

            // Act + Assert
            Assert.Throws<ArgumentNullException>(() => consoleClipboard.GetClipboardHistoryStatus(null!, testContext));
        }

        [Test]
        public void GetClipboardHistoryStatus_NullContext_ThrowsArgumentNullException()
        {
            // Arrange
            var mockConsoleWriter = new Mock<IConsoleWriter>();
            var mockClipboardManager = new Mock<IClipboardManager>();
            var consoleClipboard = new ConsoleClipboard(mockClipboardManager.Object);

            // Act + Assert
            Assert.Throws<ArgumentNullException>(() => consoleClipboard.GetClipboardHistoryStatus(mockConsoleWriter.Object, null!));
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

            var mockFormatter = new Mock<IValueFormatter>(MockBehavior.Strict);
            var mockConsoleWriter = new Mock<IConsoleWriter>(MockBehavior.Strict);
            mockConsoleWriter
                .Setup(mock => mock.WriteValue(
                    It.Is<string>(actualValue => actualValue == expectedValue),
                    It.Is<IValueFormatter>(actualFormatter => actualFormatter == mockFormatter.Object)))
                .Verifiable();

            var consoleClipboard = new ConsoleClipboard(mockClipboardManager.Object);

            // Act
            var actualReturnValue = await consoleClipboard.GetCurrentClipboardValueAsync(mockConsoleWriter.Object, mockFormatter.Object, expectedType, expectedCancellationTokenSource.Token);

            // Assert
            Assert.That(actualReturnValue, Is.EqualTo(expectedReturnValue));
            mockClipboardManager.Verify();
            mockConsoleWriter.Verify();
        }

        [Test]
        [TestCase(ContentType.Text)]
        [TestCase(ContentType.Image)]
        [TestCase(ContentType.All)]
        public async Task GetCurrentClipboardValueAsync_GetValueThrowsPastException_WritesErrorAndReturnsErrorCode(ContentType expectedType)
        {
            // Arrange
            var expectedErrorCode = ErrorCode.NotFound;
            var expectedReturnValue = (int)expectedErrorCode;
            var expectedCancellationTokenSource = new CancellationTokenSource();
            var expectedException = new PastException(expectedErrorCode, "Oh no! :O");
            var expectedErrorMessage = $"Failed to get current clipboard contents. Error: {expectedException.Message}";

            var mockClipboardManager = new Mock<IClipboardManager>(MockBehavior.Strict);
            mockClipboardManager
                .Setup(mock => mock.GetCurrentClipboardValueAsync(
                    It.Is<ContentType>(actualType => actualType == expectedType),
                    It.Is<CancellationToken>(actualCancellationToken => actualCancellationToken == expectedCancellationTokenSource.Token)))
                .ThrowsAsync(expectedException)
                .Verifiable();

            var mockFormatter = new Mock<IValueFormatter>(MockBehavior.Strict);
            var mockConsoleWriter = new Mock<IConsoleWriter>(MockBehavior.Strict);
            mockConsoleWriter
                .Setup(mock => mock.WriteErrorLine(It.Is<string>(actualMessage => actualMessage == expectedErrorMessage)))
                .Verifiable();

            var consoleClipboard = new ConsoleClipboard(mockClipboardManager.Object);

            // Act
            var actualReturnValue = await consoleClipboard.GetCurrentClipboardValueAsync(mockConsoleWriter.Object, mockFormatter.Object, expectedType, expectedCancellationTokenSource.Token);

            // Assert
            Assert.That(actualReturnValue, Is.EqualTo(expectedReturnValue));
            mockClipboardManager.Verify();
            mockConsoleWriter.Verify();
        }

        [Test]
        [TestCase(ContentType.Text)]
        [TestCase(ContentType.Image)]
        [TestCase(ContentType.All)]
        public void GetCurrentClipboardValueAsync_GetValueThrowsUnexpectedException_ExceptionUnhandled(ContentType expectedType)
        {
            // Arrange
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

            var mockFormatter = new Mock<IValueFormatter>(MockBehavior.Strict);
            var mockConsoleWriter = new Mock<IConsoleWriter>(MockBehavior.Strict);

            var consoleClipboard = new ConsoleClipboard(mockClipboardManager.Object);

            // Act
            var actualException = Assert.ThrowsAsync<Exception>(() => consoleClipboard.GetCurrentClipboardValueAsync(mockConsoleWriter.Object, mockFormatter.Object, expectedType, expectedCancellationTokenSource.Token));

            // Assert
            Assert.That(actualException, Is.EqualTo(expectedException));
            mockClipboardManager.Verify();
            mockConsoleWriter.Verify();
        }

        [Test]
        [TestCase(ContentType.Text)]
        [TestCase(ContentType.Image)]
        [TestCase(ContentType.All)]
        public void GetCurrentClipboardValueAsync_WriteValueThrowsUnexpectedException_ExceptionUnhandled(ContentType expectedType)
        {
            // Arrange
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

            var mockFormatter = new Mock<IValueFormatter>(MockBehavior.Strict);
            var mockConsoleWriter = new Mock<IConsoleWriter>(MockBehavior.Strict);
            mockConsoleWriter
                .Setup(mock => mock.WriteValue(
                    It.Is<string>(actualValue => actualValue == expectedValue),
                    It.Is<IValueFormatter>(actualFormatter => actualFormatter == mockFormatter.Object)))
                .Throws(expectedException)
                .Verifiable();

            var consoleClipboard = new ConsoleClipboard(mockClipboardManager.Object);

            // Act
            var actualException = Assert.ThrowsAsync<Exception>(() => consoleClipboard.GetCurrentClipboardValueAsync(mockConsoleWriter.Object, mockFormatter.Object, expectedType, expectedCancellationTokenSource.Token));

            // Assert
            Assert.That(actualException, Is.EqualTo(expectedException));
            mockClipboardManager.Verify();
            mockConsoleWriter.Verify();
        }

        [Test]
        public void GetCurrentClipboardValueAsync_NullConsoleWriter_ThrowsArgumentNullException()
        {
            // Arrange
            var cancellationTokenSource = new CancellationTokenSource();
            var mockFormatter = new Mock<IValueFormatter>(MockBehavior.Strict);
            var mockClipboardManager = new Mock<IClipboardManager>(MockBehavior.Strict);
            var consoleClipboard = new ConsoleClipboard(mockClipboardManager.Object);

            // Act + Assert
            Assert.ThrowsAsync<ArgumentNullException>(() => consoleClipboard.GetCurrentClipboardValueAsync(null!, mockFormatter.Object, ContentType.Text, cancellationTokenSource.Token));
        }

        [Test]
        public void GetCurrentClipboardValueAsync_NullFormatter_ThrowsArgumentNullException()
        {
            // Arrange
            var cancellationTokenSource = new CancellationTokenSource();
            var mockConsoleWriter = new Mock<IConsoleWriter>(MockBehavior.Strict);
            var mockClipboardManager = new Mock<IClipboardManager>(MockBehavior.Strict);
            var consoleClipboard = new ConsoleClipboard(mockClipboardManager.Object);

            // Act + Assert
            Assert.ThrowsAsync<ArgumentNullException>(() => consoleClipboard.GetCurrentClipboardValueAsync(mockConsoleWriter.Object, null!, ContentType.Text, cancellationTokenSource.Token));
        }
        #endregion GetCurrentClipboardValueAsync

        #region GetClipboardHistoryItemAsync
        [Test]
        [TestCase(ContentType.Text, true)]
        [TestCase(ContentType.Text, false)]
        [TestCase(ContentType.Image, true)]
        [TestCase(ContentType.Image, false)]
        [TestCase(ContentType.All, true)]
        [TestCase(ContentType.All, false)]
        public async Task GetClipboardHistoryItemAsync_ValidParameters_GetsAndWritesItemReturnsSuccess(ContentType expectedType, bool expectedSetCurrent)
        {
            // Arrange
            var expectedReturnValue = 0;
            var expectedCancellationTokenSource = new CancellationTokenSource();
            var expectedIdentifier = new ClipboardItemIdentifier(0);
            SetHistoryItemAsContentStatus expectedSetCurrentStatus = SetHistoryItemAsContentStatus.Success;

            var mockItem = new Mock<IClipboardHistoryItemWrapper>(MockBehavior.Strict);

            var mockClipboardManager = new Mock<IClipboardManager>(MockBehavior.Strict);
            mockClipboardManager
                .Setup(mock => mock.GetClipboardHistoryItemAsync(
                    It.Is<ClipboardItemIdentifier>(actualIdentifier => actualIdentifier == expectedIdentifier),
                    It.Is<ContentType>(actualType => actualType == expectedType),
                    It.Is<CancellationToken>(actualCancellationToken => actualCancellationToken == expectedCancellationTokenSource.Token)))
                .ReturnsAsync(mockItem.Object)
                .Verifiable();

            if (expectedSetCurrent)
            {
                mockClipboardManager
                    .Setup(mock => mock.SetHistoryItemAsContent(
                        It.Is<IClipboardHistoryItemWrapper>(actualItem => actualItem == mockItem.Object)))
                    .Returns(expectedSetCurrentStatus)
                    .Verifiable();
            }

            var mockFormatter = new Mock<IValueFormatter>(MockBehavior.Strict);
            var mockConsoleWriter = new Mock<IConsoleWriter>(MockBehavior.Strict);
            mockConsoleWriter
                .Setup(mock => mock.WriteItemAsync(
                    It.Is<IClipboardHistoryItemWrapper>(actualItem => actualItem == mockItem.Object),
                    It.Is<ContentType>(actualType => actualType == expectedType),
                    It.Is<int?>(actualIndex => actualIndex == null),
                    It.Is<IValueFormatter>(actualFormatter => actualFormatter == mockFormatter.Object),
                    It.Is<bool>(actualEmitLineEnding => !actualEmitLineEnding)))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var consoleClipboard = new ConsoleClipboard(mockClipboardManager.Object);

            // Act
            var actualReturnValue = await consoleClipboard.GetClipboardHistoryItemAsync(mockConsoleWriter.Object, mockFormatter.Object, expectedIdentifier, expectedType, expectedSetCurrent, expectedCancellationTokenSource.Token);

            // Assert
            Assert.That(actualReturnValue, Is.EqualTo(expectedReturnValue));
            mockClipboardManager.Verify();
            mockConsoleWriter.Verify();
        }

        [Test]
        [TestCase(SetHistoryItemAsContentStatus.AccessDenied)]
        [TestCase(SetHistoryItemAsContentStatus.ItemDeleted)]
        public async Task GetClipboardHistoryItemAsync_SetCurrentFailed_WritesItemAndErrorReturnsSuccess(SetHistoryItemAsContentStatus expectedSetCurrentStatus)
        {
            // Arrange
            var expectedReturnValue = 0;
            var expectedCancellationTokenSource = new CancellationTokenSource();
            bool expectedSetCurrent = true;
            var expectedIdentifier = new ClipboardItemIdentifier(0);
            ContentType expectedType = ContentType.Text;
            var expectedErrorMessage = $"Failed updating the current clipboard content with the selected history item. Error: {expectedSetCurrentStatus}";

            var mockItem = new Mock<IClipboardHistoryItemWrapper>(MockBehavior.Strict);

            var mockClipboardManager = new Mock<IClipboardManager>(MockBehavior.Strict);
            mockClipboardManager
                .Setup(mock => mock.GetClipboardHistoryItemAsync(
                    It.Is<ClipboardItemIdentifier>(actualIdentifier => actualIdentifier == expectedIdentifier),
                    It.Is<ContentType>(actualType => actualType == expectedType),
                    It.Is<CancellationToken>(actualCancellationToken => actualCancellationToken == expectedCancellationTokenSource.Token)))
                .ReturnsAsync(mockItem.Object)
                .Verifiable();

            mockClipboardManager
                .Setup(mock => mock.SetHistoryItemAsContent(
                    It.Is<IClipboardHistoryItemWrapper>(actualItem => actualItem == mockItem.Object)))
                .Returns(expectedSetCurrentStatus)
                .Verifiable();

            var mockFormatter = new Mock<IValueFormatter>(MockBehavior.Strict);
            var mockConsoleWriter = new Mock<IConsoleWriter>(MockBehavior.Strict);
            mockConsoleWriter
                .Setup(mock => mock.WriteItemAsync(
                    It.Is<IClipboardHistoryItemWrapper>(actualItem => actualItem == mockItem.Object),
                    It.Is<ContentType>(actualType => actualType == expectedType),
                    It.Is<int?>(actualIndex => actualIndex == null),
                    It.Is<IValueFormatter>(actualFormatter => actualFormatter == mockFormatter.Object),
                    It.Is<bool>(actualEmitLineEnding => !actualEmitLineEnding)))
                .Returns(Task.CompletedTask)
                .Verifiable();

            mockConsoleWriter
                .Setup(mock => mock.WriteErrorLine(It.Is<string>(actualErrorMessage => actualErrorMessage == expectedErrorMessage)))
                .Verifiable();

            var consoleClipboard = new ConsoleClipboard(mockClipboardManager.Object);

            // Act
            var actualReturnValue = await consoleClipboard.GetClipboardHistoryItemAsync(mockConsoleWriter.Object, mockFormatter.Object, expectedIdentifier, expectedType, expectedSetCurrent, expectedCancellationTokenSource.Token);

            // Assert
            Assert.That(actualReturnValue, Is.EqualTo(expectedReturnValue));
            mockClipboardManager.Verify();
            mockConsoleWriter.Verify();
        }

        [Test]
        public async Task GetClipboardHistoryItemAsync_GetItemThrowsPastException_WritesErrorAndReturnsErrorCode()
        {
            // Arrange
            var expectedErrorCode = ErrorCode.ClipboardHistoryDisabled;
            var expectedReturnValue = (int)expectedErrorCode;
            var expectedCancellationTokenSource = new CancellationTokenSource();
            bool expectedSetCurrent = false;
            var expectedIdentifier = new ClipboardItemIdentifier(0);
            ContentType expectedType = ContentType.Text;
            var expectedException = new PastException(expectedErrorCode, "Oh no! :o");
            var expectedErrorMessage = $"Failed to get clipboard history. Error: {expectedException.Message}";

            var mockItem = new Mock<IClipboardHistoryItemWrapper>(MockBehavior.Strict);

            var mockClipboardManager = new Mock<IClipboardManager>(MockBehavior.Strict);
            mockClipboardManager
                .Setup(mock => mock.GetClipboardHistoryItemAsync(
                    It.Is<ClipboardItemIdentifier>(actualIdentifier => actualIdentifier == expectedIdentifier),
                    It.Is<ContentType>(actualType => actualType == expectedType),
                    It.Is<CancellationToken>(actualCancellationToken => actualCancellationToken == expectedCancellationTokenSource.Token)))
                .ThrowsAsync(expectedException)
                .Verifiable();

            var mockFormatter = new Mock<IValueFormatter>(MockBehavior.Strict);
            var mockConsoleWriter = new Mock<IConsoleWriter>(MockBehavior.Strict);
            mockConsoleWriter
                .Setup(mock => mock.WriteErrorLine(It.Is<string>(actualErrorMessage => actualErrorMessage == expectedErrorMessage)))
                .Verifiable();

            var consoleClipboard = new ConsoleClipboard(mockClipboardManager.Object);

            // Act
            var actualReturnValue = await consoleClipboard.GetClipboardHistoryItemAsync(mockConsoleWriter.Object, mockFormatter.Object, expectedIdentifier, expectedType, expectedSetCurrent, expectedCancellationTokenSource.Token);

            // Assert
            Assert.That(actualReturnValue, Is.EqualTo(expectedReturnValue));
            mockClipboardManager.Verify();
            mockConsoleWriter.Verify();
        }

        [Test]
        public void GetClipboardHistoryItemAsync_GetItemThrowsUnexpectedException_ExceptionUnhandled()
        {
            // Arrange
            var expectedCancellationTokenSource = new CancellationTokenSource();
            bool expectedSetCurrent = false;
            var expectedIdentifier = new ClipboardItemIdentifier(0);
            ContentType expectedType = ContentType.Text;
            var expectedException = new Exception("Oh no! :o");
            var expectedErrorMessage = $"Failed to get clipboard history. Error: {expectedException.Message}";

            var mockItem = new Mock<IClipboardHistoryItemWrapper>(MockBehavior.Strict);

            var mockClipboardManager = new Mock<IClipboardManager>(MockBehavior.Strict);
            mockClipboardManager
                .Setup(mock => mock.GetClipboardHistoryItemAsync(
                    It.Is<ClipboardItemIdentifier>(actualIdentifier => actualIdentifier == expectedIdentifier),
                    It.Is<ContentType>(actualType => actualType == expectedType),
                    It.Is<CancellationToken>(actualCancellationToken => actualCancellationToken == expectedCancellationTokenSource.Token)))
                .ThrowsAsync(expectedException)
                .Verifiable();

            var mockFormatter = new Mock<IValueFormatter>(MockBehavior.Strict);
            var mockConsoleWriter = new Mock<IConsoleWriter>(MockBehavior.Strict);

            var consoleClipboard = new ConsoleClipboard(mockClipboardManager.Object);

            // Act
            var actualException = Assert.ThrowsAsync<Exception>(() => consoleClipboard.GetClipboardHistoryItemAsync(mockConsoleWriter.Object, mockFormatter.Object, expectedIdentifier, expectedType, expectedSetCurrent, expectedCancellationTokenSource.Token));

            // Assert
            Assert.That(actualException, Is.EqualTo(expectedException));
            mockClipboardManager.Verify();
            mockConsoleWriter.Verify();
        }

        [Test]
        public void GetClipboardHistoryItemAsync_WriteItemThrowsUnexpectedException_ExceptionUnhandled()
        {
            // Arrange
            var expectedCancellationTokenSource = new CancellationTokenSource();
            bool expectedSetCurrent = false;
            var expectedIdentifier = new ClipboardItemIdentifier(0);
            ContentType expectedType = ContentType.Text;
            var expectedException = new Exception("Oh no! :o");
            var expectedErrorMessage = $"Failed to get clipboard history. Error: {expectedException.Message}";

            var mockItem = new Mock<IClipboardHistoryItemWrapper>(MockBehavior.Strict);

            var mockClipboardManager = new Mock<IClipboardManager>(MockBehavior.Strict);
            mockClipboardManager
                .Setup(mock => mock.GetClipboardHistoryItemAsync(
                    It.Is<ClipboardItemIdentifier>(actualIdentifier => actualIdentifier == expectedIdentifier),
                    It.Is<ContentType>(actualType => actualType == expectedType),
                    It.Is<CancellationToken>(actualCancellationToken => actualCancellationToken == expectedCancellationTokenSource.Token)))
                .ReturnsAsync(mockItem.Object)
                .Verifiable();

            var mockFormatter = new Mock<IValueFormatter>(MockBehavior.Strict);
            var mockConsoleWriter = new Mock<IConsoleWriter>(MockBehavior.Strict);
            mockConsoleWriter
                .Setup(mock => mock.WriteItemAsync(
                    It.Is<IClipboardHistoryItemWrapper>(actualItem => actualItem == mockItem.Object),
                    It.Is<ContentType>(actualType => actualType == expectedType),
                    It.Is<int?>(actualIndex => actualIndex == null),
                    It.Is<IValueFormatter>(actualFormatter => actualFormatter == mockFormatter.Object),
                    It.Is<bool>(actualEmitLineEnding => !actualEmitLineEnding)))
                .ThrowsAsync(expectedException)
                .Verifiable();

            var consoleClipboard = new ConsoleClipboard(mockClipboardManager.Object);

            // Act
            var actualException = Assert.ThrowsAsync<Exception>(() => consoleClipboard.GetClipboardHistoryItemAsync(mockConsoleWriter.Object, mockFormatter.Object, expectedIdentifier, expectedType, expectedSetCurrent, expectedCancellationTokenSource.Token));

            // Assert
            Assert.That(actualException, Is.EqualTo(expectedException));
            mockClipboardManager.Verify();
            mockConsoleWriter.Verify();
        }

        [Test]
        public void GetClipboardHistoryItemAsync_NullConsoleWriter_ThrowsArgumentNullException()
        {
            // Arrange
            var expectedCancellationTokenSource = new CancellationTokenSource();
            bool expectedSetCurrent = false;
            var expectedIdentifier = new ClipboardItemIdentifier(0);
            ContentType expectedType = ContentType.Text;

            var mockFormatter = new Mock<IValueFormatter>(MockBehavior.Strict);
            var mockClipboardManager = new Mock<IClipboardManager>(MockBehavior.Strict);
            var consoleClipboard = new ConsoleClipboard(mockClipboardManager.Object);

            // Act + Assert
            Assert.ThrowsAsync<ArgumentNullException>(() => consoleClipboard.GetClipboardHistoryItemAsync(null!, mockFormatter.Object, expectedIdentifier, expectedType, expectedSetCurrent, expectedCancellationTokenSource.Token));
        }

        [Test]
        public void GetClipboardHistoryItemAsync_NullFormatter_ThrowsArgumentNullException()
        {
            // Arrange
            var expectedCancellationTokenSource = new CancellationTokenSource();
            bool expectedSetCurrent = false;
            var expectedIdentifier = new ClipboardItemIdentifier(0);
            ContentType expectedType = ContentType.Text;

            var mockConsoleWriter = new Mock<IConsoleWriter>(MockBehavior.Strict);
            var mockClipboardManager = new Mock<IClipboardManager>(MockBehavior.Strict);
            var consoleClipboard = new ConsoleClipboard(mockClipboardManager.Object);

            // Act + Assert
            Assert.ThrowsAsync<ArgumentNullException>(() => consoleClipboard.GetClipboardHistoryItemAsync(mockConsoleWriter.Object, null!, expectedIdentifier, expectedType, expectedSetCurrent, expectedCancellationTokenSource.Token));
        }

        [Test]
        public void GetClipboardHistoryItemAsync_NullIdentifier_ThrowsArgumentNullException()
        {
            // Arrange
            var expectedCancellationTokenSource = new CancellationTokenSource();
            bool expectedSetCurrent = false;
            ContentType expectedType = ContentType.Text;


            var mockFormatter = new Mock<IValueFormatter>(MockBehavior.Strict);
            var mockConsoleWriter = new Mock<IConsoleWriter>(MockBehavior.Strict);
            var mockClipboardManager = new Mock<IClipboardManager>(MockBehavior.Strict);
            var consoleClipboard = new ConsoleClipboard(mockClipboardManager.Object);

            // Act + Assert
            Assert.ThrowsAsync<ArgumentNullException>(() => consoleClipboard.GetClipboardHistoryItemAsync(mockConsoleWriter.Object, mockFormatter.Object, null!, expectedType, expectedSetCurrent, expectedCancellationTokenSource.Token));
        }
        #endregion GetClipboardHistoryItemAsync

        #region ListClipboardHistoryAsync
        [Test]
        [TestCase(ContentType.Text, true)]
        [TestCase(ContentType.Text, false)]
        [TestCase(ContentType.Image, true)]
        [TestCase(ContentType.Image, false)]
        [TestCase(ContentType.All, true)]
        [TestCase(ContentType.All, false)]
        public async Task ListClipboardHistoryAsync_ValidParameters_GetsAndWritesItemReturnsSuccess(ContentType expectedType, bool expectedPinned)
        {
            // Arrange
            var expectedReturnValue = 0;
            var expectedCancellationTokenSource = new CancellationTokenSource();

            var expectedItems = new List<IClipboardHistoryItemWrapper>();
            for (int i = 0; i < 5; i++)
            {
                expectedItems.Add(new Mock<IClipboardHistoryItemWrapper>(MockBehavior.Strict).Object);
            }

            var mockClipboardManager = new Mock<IClipboardManager>(MockBehavior.Strict);
            mockClipboardManager
                .Setup(mock => mock.GetClipboardHistoryAsync(
                    It.Is<ContentType>(actualType => actualType == expectedType),
                    It.Is<bool>(actualPinned => actualPinned == expectedPinned),
                    It.Is<CancellationToken>(actualCancellationToken => actualCancellationToken == expectedCancellationTokenSource.Token)))
                .ReturnsAsync(expectedItems)
                .Verifiable();

            var mockFormatter = new Mock<IValueFormatter>(MockBehavior.Strict);
            var mockConsoleWriter = new Mock<IConsoleWriter>(MockBehavior.Strict);

            bool? initialEmitLineEnding = null;
            bool? finalEmitLineEnding = null;
            var actualItems = new List<IClipboardHistoryItemWrapper>();
            mockConsoleWriter
                .Setup(mock => mock.WriteItemAsync(
                    It.Is<IClipboardHistoryItemWrapper>(actualItem => expectedItems.Contains(actualItem)),
                    It.Is<ContentType>(actualType => actualType == expectedType),
                    It.Is<int?>(actualIndex => actualIndex < expectedItems.Count),
                    It.Is<IValueFormatter>(actualFormatter => actualFormatter == mockFormatter.Object),
                    It.IsAny<bool>()))
                .Returns(Task.CompletedTask)
                .Callback<IClipboardHistoryItemWrapper, ContentType, int?, IValueFormatter?, bool>((actualItem, type, index, formatter, actualEmitLineEnding) =>
                {
                    actualItems.Add(actualItem);
                    if (initialEmitLineEnding == null)
                    {
                        initialEmitLineEnding = actualEmitLineEnding;
                    }
                    else if (initialEmitLineEnding != actualEmitLineEnding)
                    {
                        if (finalEmitLineEnding == null)
                        {
                            finalEmitLineEnding = actualEmitLineEnding;
                        }
                        else
                        {
                            Assert.Fail("Unexpected emit line ending state");
                        }
                    }
                })
                .Verifiable();

            var consoleClipboard = new ConsoleClipboard(mockClipboardManager.Object);

            // Act
            var actualReturnValue = await consoleClipboard.ListClipboardHistoryAsync(mockConsoleWriter.Object, mockFormatter.Object, expectedType, expectedPinned, expectedCancellationTokenSource.Token);

            // Assert
            Assert.That(actualReturnValue, Is.EqualTo(expectedReturnValue));
            Assert.That(actualItems, Is.EqualTo(expectedItems));
            Assert.That(initialEmitLineEnding, Is.True);
            Assert.That(finalEmitLineEnding, Is.False);
            mockClipboardManager.Verify();
            mockConsoleWriter.Verify();
        }

        [Test]
        public async Task ListClipboardHistoryAsync_ListItemsThrowsPastException_WritesErrorAndReturnsErrorCode()
        {
            // Arrange
            var expectedExceptionErrorCode = ErrorCode.ClipboardHistoryDisabled;
            var expectedReturnValue = (int)expectedExceptionErrorCode;
            var expectedType = ContentType.Text;
            bool expectedPinned = true;
            var expectedCancellationTokenSource = new CancellationTokenSource();
            var expectedException = new PastException(expectedExceptionErrorCode, "Oh no! :o");
            var expectedErrorMessage = $"Failed to get clipboard history. Error: {expectedException.Message}";

            var mockClipboardManager = new Mock<IClipboardManager>(MockBehavior.Strict);
            mockClipboardManager
                .Setup(mock => mock.GetClipboardHistoryAsync(
                    It.Is<ContentType>(actualType => actualType == expectedType),
                    It.Is<bool>(actualPinned => actualPinned == expectedPinned),
                    It.Is<CancellationToken>(actualCancellationToken => actualCancellationToken == expectedCancellationTokenSource.Token)))
                .ThrowsAsync(expectedException)
                .Verifiable();

            var mockFormatter = new Mock<IValueFormatter>(MockBehavior.Strict);
            var mockConsoleWriter = new Mock<IConsoleWriter>(MockBehavior.Strict);
            mockConsoleWriter
                .Setup(mock => mock.WriteErrorLine(It.Is<string>(actualErrorMessage => actualErrorMessage == expectedErrorMessage)))
                .Verifiable();

            var consoleClipboard = new ConsoleClipboard(mockClipboardManager.Object);

            // Act
            var actualReturnValue = await consoleClipboard.ListClipboardHistoryAsync(mockConsoleWriter.Object, mockFormatter.Object, expectedType, expectedPinned, expectedCancellationTokenSource.Token);

            // Assert
            Assert.That(actualReturnValue, Is.EqualTo(expectedReturnValue));
            mockClipboardManager.Verify();
            mockConsoleWriter.Verify();
        }

        [Test]
        public void ListClipboardHistoryAsync_ListItemsThrowsUnexpectedException_ExceptionUnhandled()
        {
            // Arrange
            var expectedType = ContentType.Text;
            bool expectedPinned = true;
            var expectedCancellationTokenSource = new CancellationTokenSource();
            var expectedException = new Exception("Oh no! :o");
            var expectedErrorMessage = $"Failed to get clipboard history. Error: {expectedException.Message}";

            var mockClipboardManager = new Mock<IClipboardManager>(MockBehavior.Strict);
            mockClipboardManager
                .Setup(mock => mock.GetClipboardHistoryAsync(
                    It.Is<ContentType>(actualType => actualType == expectedType),
                    It.Is<bool>(actualPinned => actualPinned == expectedPinned),
                    It.Is<CancellationToken>(actualCancellationToken => actualCancellationToken == expectedCancellationTokenSource.Token)))
                .ThrowsAsync(expectedException)
                .Verifiable();

            var mockFormatter = new Mock<IValueFormatter>(MockBehavior.Strict);
            var mockConsoleWriter = new Mock<IConsoleWriter>(MockBehavior.Strict);

            var consoleClipboard = new ConsoleClipboard(mockClipboardManager.Object);

            // Act
            var actualException = Assert.ThrowsAsync<Exception>(() => consoleClipboard.ListClipboardHistoryAsync(mockConsoleWriter.Object, mockFormatter.Object, expectedType, expectedPinned, expectedCancellationTokenSource.Token));

            // Assert
            Assert.That(actualException, Is.EqualTo(expectedException));
            mockClipboardManager.Verify();
            mockConsoleWriter.Verify();
        }

        [Test]
        public void ListClipboardHistoryAsync_WriteItemThrowsUnexpectedException_ExceptionUnhandled()
        {
            // Arrange
            var expectedType = ContentType.Text;
            bool expectedPinned = true;
            var expectedCancellationTokenSource = new CancellationTokenSource();
            var expectedException = new Exception("Oh no! :o");
            var expectedErrorMessage = $"Failed to get clipboard history. Error: {expectedException.Message}";

            var mockItem = new Mock<IClipboardHistoryItemWrapper>(MockBehavior.Strict);
            var expectedItems = new List<IClipboardHistoryItemWrapper> { mockItem.Object };

            var mockClipboardManager = new Mock<IClipboardManager>(MockBehavior.Strict);
            mockClipboardManager
                .Setup(mock => mock.GetClipboardHistoryAsync(
                    It.Is<ContentType>(actualType => actualType == expectedType),
                    It.Is<bool>(actualPinned => actualPinned == expectedPinned),
                    It.Is<CancellationToken>(actualCancellationToken => actualCancellationToken == expectedCancellationTokenSource.Token)))
                .ReturnsAsync(expectedItems)
                .Verifiable();

            var mockFormatter = new Mock<IValueFormatter>(MockBehavior.Strict);
            var mockConsoleWriter = new Mock<IConsoleWriter>(MockBehavior.Strict);
            mockConsoleWriter
                .Setup(mock => mock.WriteItemAsync(
                    It.Is<IClipboardHistoryItemWrapper>(actualItem => actualItem == mockItem.Object),
                    It.Is<ContentType>(actualType => actualType == expectedType),
                    It.Is<int?>(actualIndex => actualIndex == 0),
                    It.Is<IValueFormatter>(actualFormatter => actualFormatter == mockFormatter.Object),
                    It.IsAny<bool>()))
                .ThrowsAsync(expectedException)
                .Verifiable();

            var consoleClipboard = new ConsoleClipboard(mockClipboardManager.Object);

            // Act
            var actualException = Assert.ThrowsAsync<Exception>(() => consoleClipboard.ListClipboardHistoryAsync(mockConsoleWriter.Object, mockFormatter.Object, expectedType, expectedPinned, expectedCancellationTokenSource.Token));

            // Assert
            Assert.That(actualException, Is.EqualTo(expectedException));
            mockClipboardManager.Verify();
            mockConsoleWriter.Verify();
        }

        [Test]
        public void ListClipboardHistoryAsync_NullConsoleWriter_ThrowsArgumentNullException()
        {
            // Arrange
            var expectedType = ContentType.Text;
            bool expectedPinned = true;
            var expectedCancellationTokenSource = new CancellationTokenSource();

            var mockFormatter = new Mock<IValueFormatter>(MockBehavior.Strict);
            var mockClipboardManager = new Mock<IClipboardManager>(MockBehavior.Strict);
            var consoleClipboard = new ConsoleClipboard(mockClipboardManager.Object);

            // Act + Assert
            Assert.ThrowsAsync<ArgumentNullException>(() => consoleClipboard.ListClipboardHistoryAsync(null!, mockFormatter.Object, expectedType, expectedPinned, expectedCancellationTokenSource.Token));
        }

        [Test]
        public void ListClipboardHistoryAsync_NullFormatter_ThrowsArgumentNullException()
        {
            // Arrange
            var expectedType = ContentType.Text;
            bool expectedPinned = true;
            var expectedCancellationTokenSource = new CancellationTokenSource();

            var mockConsoleWriter = new Mock<IConsoleWriter>(MockBehavior.Strict);
            var mockClipboardManager = new Mock<IClipboardManager>(MockBehavior.Strict);
            var consoleClipboard = new ConsoleClipboard(mockClipboardManager.Object);

            // Act + Assert
            Assert.ThrowsAsync<ArgumentNullException>(() => consoleClipboard.ListClipboardHistoryAsync(mockConsoleWriter.Object, null!, expectedType, expectedPinned, expectedCancellationTokenSource.Token));
        }
        #endregion ListClipboardHistoryAsync
    }
}
