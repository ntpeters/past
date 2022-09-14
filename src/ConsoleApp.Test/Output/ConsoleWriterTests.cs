using Moq;
using past.ConsoleApp.Output;
using past.ConsoleApp.Wrappers;
using past.Core;
using past.Core.Wrappers;
using System.CommandLine;
using System.CommandLine.IO;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;

namespace past.ConsoleApp.Test.Output
{
    public class ConsoleWriterTests
    {
        #region Constructor
        [Test]
        [TestCaseSource(nameof(ContructorTestCases))]
        public void Constructor_ValidParameters_Success(bool expectedSuppressErrorOutput, bool expectedEnableAnsiProcessing, AnsiResetType expectedAnsiResetType)
        {
            var mockConsole = new Mock<IConsole>(MockBehavior.Strict);
            var mockEnvironment = new Mock<IEnvironmentWrapper>(MockBehavior.Strict);
            Assert.DoesNotThrow(() => new ConsoleWriter(
                mockConsole.Object,
                mockEnvironment.Object,
                expectedSuppressErrorOutput,
                expectedEnableAnsiProcessing,
                expectedAnsiResetType));
        }

        [Test]
        public void Constructor_NullConsole_ThrowsArgumentNullException()
        {
            var mockEnvironment = new Mock<IEnvironmentWrapper>(MockBehavior.Strict);
            Assert.Throws<ArgumentNullException>(() => new ConsoleWriter(
                null!,
                mockEnvironment.Object,
                true,
                true,
                AnsiResetType.Auto));
        }

        [Test]
        public void Constructor_NullEnvironment_ThrowsArgumentNullException()
        {
            var mockConsole = new Mock<IConsole>(MockBehavior.Strict);
            Assert.Throws<ArgumentNullException>(() => new ConsoleWriter(
                mockConsole.Object,
                null!,
                true,
                true,
                AnsiResetType.Auto));
        }
        #endregion Constructor

        #region WriteItemAsync
        [Test]
        public void WriteItemAsync_NullItem_ThrowsArgumentNullException()
        {
            // Arrange
            var ansiResetType = AnsiResetType.On;
            bool enableAnsiProcessing = false;
            var suppressErrorOutput = false;
            var expectedEmitLineEnding = false;
            var expectedType = ContentType.Text;
            var expectedIndex = 0;

            var mockFormatter = new Mock<IValueFormatter>(MockBehavior.Strict);
            var mockConsole = new Mock<IConsole>(MockBehavior.Strict);
            var mockEnvironment = new Mock<IEnvironmentWrapper>(MockBehavior.Strict);

            var consoleWriter = new ConsoleWriter(
                mockConsole.Object,
                mockEnvironment.Object,
                suppressErrorOutput,
                enableAnsiProcessing,
                ansiResetType);

            // Act + Assert
            Assert.ThrowsAsync<ArgumentNullException>(
                () => consoleWriter.WriteItemAsync(
                    null!,
                    expectedType,
                    expectedIndex,
                    mockFormatter.Object,
                    expectedEmitLineEnding));
        }

        [Test]
        [TestCase(ContentType.Text)]
        [TestCase(ContentType.All)]
        public async Task WriteItemAsync_TextValueWithCompatibleContentType_WritesValue(ContentType compatibleType)
        {
            // Arrange
            var ansiResetType = AnsiResetType.On; // This won't do anything unless a formatter is provided
            bool enableAnsiProcessing = false;
            var suppressErrorOutput = false;
            var emitLineEnding = false;

            var expectedText = "~meow~";
            var dataPackage = new DataPackage();
            dataPackage.SetText(expectedText);
            var mockItem = new Mock<IClipboardHistoryItemWrapper>(MockBehavior.Strict);
            mockItem.SetupGet(mock => mock.Content).Returns(dataPackage.GetView()).Verifiable();

            var mockStreamWriter = new Mock<IStandardStreamWriter>(MockBehavior.Strict);

            string? actualWrittenValue = null;
            mockStreamWriter
                .Setup(mock => mock.Write(It.IsAny<string?>()))
                .Callback<string?>(value => actualWrittenValue = value)
                .Verifiable();

            var mockConsole = new Mock<IConsole>(MockBehavior.Strict);
            mockConsole
                .SetupGet(mock => mock.Out)
                .Returns(mockStreamWriter.Object)
                .Verifiable();

            var mockEnvironment = new Mock<IEnvironmentWrapper>(MockBehavior.Strict);

            var consoleWriter = new ConsoleWriter(
                mockConsole.Object,
                mockEnvironment.Object,
                suppressErrorOutput,
                enableAnsiProcessing,
                ansiResetType);

            // Act
            await consoleWriter.WriteItemAsync(
                mockItem.Object,
                compatibleType,
                null,
                null,
                emitLineEnding);

            // Assert
            Assert.That(actualWrittenValue, Is.EqualTo(expectedText));

            mockItem.Verify();
            mockStreamWriter.Verify();
            mockConsole.Verify();
            mockEnvironment.Verify();
        }


        [Test]
        public async Task WriteItemAsync_TextValueWithIncompatibleContentType_NoOutput()
        {
            // Arrange
            var incompatibleType = ContentType.Image;
            var ansiResetType = AnsiResetType.On;
            bool enableAnsiProcessing = false;
            var suppressErrorOutput = false;
            var emitLineEnding = false;

            var expectedText = "~meow~";
            var dataPackage = new DataPackage();
            dataPackage.SetText(expectedText);
            var mockItem = new Mock<IClipboardHistoryItemWrapper>(MockBehavior.Strict);
            mockItem.SetupGet(mock => mock.Content).Returns(dataPackage.GetView()).Verifiable();

            var mockConsole = new Mock<IConsole>(MockBehavior.Strict);
            var mockEnvironment = new Mock<IEnvironmentWrapper>(MockBehavior.Strict);

            var consoleWriter = new ConsoleWriter(
                mockConsole.Object,
                mockEnvironment.Object,
                suppressErrorOutput,
                enableAnsiProcessing,
                ansiResetType);

            // Act
            await consoleWriter.WriteItemAsync(
                mockItem.Object,
                incompatibleType,
                null,
                null,
                emitLineEnding);

            // NOTE: No asserts needed for this test, as it will fail if something is written to the console since
            // no setup was proved for the output stream and the console mock is created with MockBehavior.Strict

            mockItem.Verify();
            mockConsole.Verify();
            mockEnvironment.Verify();
        }

        [Test]
        [TestCase(ContentType.Text)]
        [TestCase(ContentType.Image)]
        public async Task WriteItemAsync_UnsupportedValueWithSpecificContentType_NoOutput(ContentType type)
        {
            // Arrange
            var ansiResetType = AnsiResetType.On;
            bool enableAnsiProcessing = false;
            var suppressErrorOutput = false;
            var emitLineEnding = false;

            var mockStorageItems = new List<IStorageItem>
            {
                new Mock<IStorageItem>(MockBehavior.Strict).Object
            };

            var dataPackage = new DataPackage();
            dataPackage.SetStorageItems(mockStorageItems, false);
            var mockItem = new Mock<IClipboardHistoryItemWrapper>(MockBehavior.Strict);
            mockItem.SetupGet(mock => mock.Content).Returns(dataPackage.GetView()).Verifiable();

            var mockConsole = new Mock<IConsole>(MockBehavior.Strict);
            var mockEnvironment = new Mock<IEnvironmentWrapper>(MockBehavior.Strict);

            var consoleWriter = new ConsoleWriter(
                mockConsole.Object,
                mockEnvironment.Object,
                suppressErrorOutput,
                enableAnsiProcessing,
                ansiResetType);

            // Act
            await consoleWriter.WriteItemAsync(
                mockItem.Object,
                type,
                null,
                null,
                emitLineEnding);

            // NOTE: No asserts needed for this test, as it will fail if something is written to the console since
            // no setup was proved for the output stream and the console mock is created with MockBehavior.Strict

            mockItem.Verify();
            mockConsole.Verify();
            mockEnvironment.Verify();
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public async Task WriteItemAsync_UnsupportedValueWithAllContentType_WritesUnsupportedMessage(bool enableAnsiProcessing)
        {
            // Arrange
            var type = ContentType.All;
            var ansiResetType = AnsiResetType.On;
            var suppressErrorOutput = false;
            var emitLineEnding = false;
            var index = 0;

            var mockStorageItems = new List<IStorageItem>
            {
                new Mock<IStorageItem>(MockBehavior.Strict).Object
            };

            var dataPackage = new DataPackage();
            dataPackage.SetStorageItems(mockStorageItems, false);
            var dataPackageView = dataPackage.GetView();

            var expectedText = $"[Unsupported Format: {string.Join(',', dataPackageView.AvailableFormats)}]";
            if (enableAnsiProcessing)
            {
                expectedText = $"{NativeConstants.ANSI_RED}{expectedText}";
            }

            var mockItem = new Mock<IClipboardHistoryItemWrapper>(MockBehavior.Strict);
            mockItem.SetupGet(mock => mock.Content).Returns(dataPackageView).Verifiable();

            var mockStreamWriter = new Mock<IStandardStreamWriter>(MockBehavior.Strict);

            string? actualWrittenValue = null;
            mockStreamWriter
                .Setup(mock => mock.Write(It.IsAny<string?>()))
                .Callback<string?>(value => actualWrittenValue = value)
                .Verifiable();

            var mockConsole = new Mock<IConsole>(MockBehavior.Strict);
            mockConsole
                .SetupGet(mock => mock.Out)
                .Returns(mockStreamWriter.Object)
                .Verifiable();

            var mockEnvironment = new Mock<IEnvironmentWrapper>(MockBehavior.Strict);

            var consoleWriter = new ConsoleWriter(
                mockConsole.Object,
                mockEnvironment.Object,
                suppressErrorOutput,
                enableAnsiProcessing,
                ansiResetType);

            // Act
            await consoleWriter.WriteItemAsync(
                mockItem.Object,
                type,
                index,
                null,
                emitLineEnding);

            // Assert
            Assert.That(actualWrittenValue, Is.EqualTo(expectedText));

            mockItem.Verify();
            mockStreamWriter.Verify();
            mockConsole.Verify();
            mockEnvironment.Verify();
        }

        [Test]
        [TestCase(int.MaxValue)]
        [TestCase(int.MinValue)]
        [TestCase(null)]
        public async Task WriteItemAsync_NonNullFormatter_WritesFormattedValue(int? expectedIndex)
        {
            // Arrange
            var type = ContentType.Text;
            var ansiResetType = AnsiResetType.Off;
            bool enableAnsiProcessing = false;
            var suppressErrorOutput = false;
            var emitLineEnding = false;
            var expectedId = Guid.NewGuid();
            var expectedTimestamp = DateTimeOffset.Now;

            var valueText = "~meow~";
            var expectedFormattedText = "🐱";
            var dataPackage = new DataPackage();
            dataPackage.SetText(valueText);
            var mockItem = new Mock<IClipboardHistoryItemWrapper>(MockBehavior.Strict);
            mockItem.SetupGet(mock => mock.Content).Returns(dataPackage.GetView()).Verifiable();
            mockItem.SetupGet(mock => mock.Id).Returns(expectedId.ToString()).Verifiable();
            mockItem.SetupGet(mock => mock.Timestamp).Returns(expectedTimestamp).Verifiable();

            var mockFormatter = new Mock<IValueFormatter>(MockBehavior.Strict);
            mockFormatter
                .Setup(mock => mock.Format(
                    It.Is<string>(actualValue => actualValue == valueText),
                    It.Is<int?>(actualIndex => actualIndex == expectedIndex),
                    It.Is<string>(actualId => actualId == expectedId.ToString()),
                    It.Is<DateTimeOffset>(actualTimestamp => actualTimestamp == expectedTimestamp),
                    It.Is<bool>(actualEmitAnsiReset => actualEmitAnsiReset == false),
                    It.Is<bool>(actualEmitLineEnding => actualEmitLineEnding == emitLineEnding)))
                .Returns(expectedFormattedText)
                .Verifiable();

            var mockStreamWriter = new Mock<IStandardStreamWriter>(MockBehavior.Strict);

            string? actualWrittenValue = null;
            mockStreamWriter
                .Setup(mock => mock.Write(It.IsAny<string?>()))
                .Callback<string?>(value => actualWrittenValue = value)
                .Verifiable();

            var mockConsole = new Mock<IConsole>(MockBehavior.Strict);
            mockConsole
                .SetupGet(mock => mock.Out)
                .Returns(mockStreamWriter.Object)
                .Verifiable();

            var mockEnvironment = new Mock<IEnvironmentWrapper>(MockBehavior.Strict);

            var consoleWriter = new ConsoleWriter(
                mockConsole.Object,
                mockEnvironment.Object,
                suppressErrorOutput,
                enableAnsiProcessing,
                ansiResetType);

            // Act
            await consoleWriter.WriteItemAsync(
                mockItem.Object,
                type,
                expectedIndex,
                mockFormatter.Object,
                emitLineEnding);

            // Assert
            Assert.That(actualWrittenValue, Is.EqualTo(expectedFormattedText));

            mockItem.Verify();
            mockFormatter.Verify();
            mockStreamWriter.Verify();
            mockConsole.Verify();
            mockEnvironment.Verify();
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public async Task WriteItemAsync_AnsiResetOff_EmitAnsiResetFalse(bool enableAnsiProcessing)
        {
            // Arrange
            var ansiResetType = AnsiResetType.Off;
            var expectedEmitAnsiReset = false;

            var type = ContentType.Text;
            var suppressErrorOutput = false;
            var emitLineEnding = false;
            var expectedIndex = 42;
            var expectedId = Guid.NewGuid();
            var expectedTimestamp = DateTimeOffset.Now;

            var valueText = "~meow~";
            var expectedFormattedText = "🐱";
            var dataPackage = new DataPackage();
            dataPackage.SetText(valueText);
            var mockItem = new Mock<IClipboardHistoryItemWrapper>(MockBehavior.Strict);
            mockItem.SetupGet(mock => mock.Content).Returns(dataPackage.GetView()).Verifiable();
            mockItem.SetupGet(mock => mock.Id).Returns(expectedId.ToString()).Verifiable();
            mockItem.SetupGet(mock => mock.Timestamp).Returns(expectedTimestamp).Verifiable();

            var mockFormatter = new Mock<IValueFormatter>(MockBehavior.Strict);
            mockFormatter
                .Setup(mock => mock.Format(
                    It.Is<string>(actualValue => actualValue == valueText),
                    It.Is<int?>(actualIndex => actualIndex == expectedIndex),
                    It.Is<string>(actualId => actualId == expectedId.ToString()),
                    It.Is<DateTimeOffset>(actualTimestamp => actualTimestamp == expectedTimestamp),
                    It.Is<bool>(actualEmitAnsiReset => actualEmitAnsiReset == expectedEmitAnsiReset),
                    It.Is<bool>(actualEmitLineEnding => actualEmitLineEnding == emitLineEnding)))
                .Returns(expectedFormattedText)
                .Verifiable();

            var mockStreamWriter = new Mock<IStandardStreamWriter>(MockBehavior.Strict);

            string? actualWrittenValue = null;
            mockStreamWriter
                .Setup(mock => mock.Write(It.IsAny<string?>()))
                .Callback<string?>(value => actualWrittenValue = value)
                .Verifiable();

            var mockConsole = new Mock<IConsole>(MockBehavior.Strict);
            mockConsole
                .SetupGet(mock => mock.Out)
                .Returns(mockStreamWriter.Object)
                .Verifiable();

            var mockEnvironment = new Mock<IEnvironmentWrapper>(MockBehavior.Strict);

            var consoleWriter = new ConsoleWriter(
                mockConsole.Object,
                mockEnvironment.Object,
                suppressErrorOutput,
                enableAnsiProcessing,
                ansiResetType);

            // Act
            await consoleWriter.WriteItemAsync(
                mockItem.Object,
                type,
                expectedIndex,
                mockFormatter.Object,
                emitLineEnding);

            // Assert
            Assert.That(actualWrittenValue, Is.EqualTo(expectedFormattedText));

            mockItem.Verify();
            mockFormatter.Verify();
            mockStreamWriter.Verify();
            mockConsole.Verify();
            mockEnvironment.Verify();
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public async Task WriteItemAsync_AnsiResetOn_EmitAnsiResetTrue(bool enableAnsiProcessing)
        {
            // Arrange
            var ansiResetType = AnsiResetType.On;
            var expectedEmitAnsiReset = true;

            var type = ContentType.Text;
            var suppressErrorOutput = false;
            var emitLineEnding = false;
            var expectedIndex = 42;
            var expectedId = Guid.NewGuid();
            var expectedTimestamp = DateTimeOffset.Now;

            var valueText = "~meow~";
            var expectedFormattedText = "🐱";
            var dataPackage = new DataPackage();
            dataPackage.SetText(valueText);
            var mockItem = new Mock<IClipboardHistoryItemWrapper>(MockBehavior.Strict);
            mockItem.SetupGet(mock => mock.Content).Returns(dataPackage.GetView()).Verifiable();
            mockItem.SetupGet(mock => mock.Id).Returns(expectedId.ToString()).Verifiable();
            mockItem.SetupGet(mock => mock.Timestamp).Returns(expectedTimestamp).Verifiable();

            var mockFormatter = new Mock<IValueFormatter>(MockBehavior.Strict);
            mockFormatter
                .Setup(mock => mock.Format(
                    It.Is<string>(actualValue => actualValue == valueText),
                    It.Is<int?>(actualIndex => actualIndex == expectedIndex),
                    It.Is<string>(actualId => actualId == expectedId.ToString()),
                    It.Is<DateTimeOffset>(actualTimestamp => actualTimestamp == expectedTimestamp),
                    It.Is<bool>(actualEmitAnsiReset => actualEmitAnsiReset == expectedEmitAnsiReset),
                    It.Is<bool>(actualEmitLineEnding => actualEmitLineEnding == emitLineEnding)))
                .Returns(expectedFormattedText)
                .Verifiable();

            var mockStreamWriter = new Mock<IStandardStreamWriter>(MockBehavior.Strict);

            string? actualWrittenValue = null;
            mockStreamWriter
                .Setup(mock => mock.Write(It.IsAny<string?>()))
                .Callback<string?>(value => actualWrittenValue = value)
                .Verifiable();

            var mockConsole = new Mock<IConsole>(MockBehavior.Strict);
            mockConsole
                .SetupGet(mock => mock.Out)
                .Returns(mockStreamWriter.Object)
                .Verifiable();

            var mockEnvironment = new Mock<IEnvironmentWrapper>(MockBehavior.Strict);

            var consoleWriter = new ConsoleWriter(
                mockConsole.Object,
                mockEnvironment.Object,
                suppressErrorOutput,
                enableAnsiProcessing,
                ansiResetType);

            // Act
            await consoleWriter.WriteItemAsync(
                mockItem.Object,
                type,
                expectedIndex,
                mockFormatter.Object,
                emitLineEnding);

            // Assert
            Assert.That(actualWrittenValue, Is.EqualTo(expectedFormattedText));

            mockItem.Verify();
            mockFormatter.Verify();
            mockStreamWriter.Verify();
            mockConsole.Verify();
            mockEnvironment.Verify();
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public async Task WriteItemAsync_AnsiResetAutoWithAnsiEnabled_EmitAnsiResetTrueIfContainsAnsi(bool valueContainsAnsi)
        {
            // Arrange
            var ansiResetType = AnsiResetType.Auto;
            var enableAnsiProcessing = true;
            var expectedEmitAnsiReset = valueContainsAnsi;

            var type = ContentType.Text;
            var suppressErrorOutput = false;
            var emitLineEnding = false;
            var expectedIndex = 42;
            var expectedId = Guid.NewGuid();
            var expectedTimestamp = DateTimeOffset.Now;

            var valueText = "~meow~";
            var expectedFormattedText = "🐱";
            if (valueContainsAnsi)
            {
                valueText = $"{NativeConstants.ANSI_ESCAPE}[38;2;131;95;177m{valueText}";
                expectedFormattedText += NativeConstants.ANSI_RESET;
            }

            var dataPackage = new DataPackage();
            dataPackage.SetText(valueText);
            var mockItem = new Mock<IClipboardHistoryItemWrapper>(MockBehavior.Strict);
            mockItem.SetupGet(mock => mock.Content).Returns(dataPackage.GetView()).Verifiable();
            mockItem.SetupGet(mock => mock.Id).Returns(expectedId.ToString()).Verifiable();
            mockItem.SetupGet(mock => mock.Timestamp).Returns(expectedTimestamp).Verifiable();

            var mockFormatter = new Mock<IValueFormatter>(MockBehavior.Strict);
            mockFormatter
                .Setup(mock => mock.Format(
                    It.Is<string>(actualValue => actualValue == valueText),
                    It.Is<int?>(actualIndex => actualIndex == expectedIndex),
                    It.Is<string>(actualId => actualId == expectedId.ToString()),
                    It.Is<DateTimeOffset>(actualTimestamp => actualTimestamp == expectedTimestamp),
                    It.Is<bool>(actualEmitAnsiReset => actualEmitAnsiReset == expectedEmitAnsiReset),
                    It.Is<bool>(actualEmitLineEnding => actualEmitLineEnding == emitLineEnding)))
                .Returns(expectedFormattedText)
                .Verifiable();

            var mockStreamWriter = new Mock<IStandardStreamWriter>(MockBehavior.Strict);

            string? actualWrittenValue = null;
            mockStreamWriter
                .Setup(mock => mock.Write(It.IsAny<string?>()))
                .Callback<string?>(value => actualWrittenValue = value)
                .Verifiable();

            var mockConsole = new Mock<IConsole>(MockBehavior.Strict);
            mockConsole
                .SetupGet(mock => mock.Out)
                .Returns(mockStreamWriter.Object)
                .Verifiable();

            var mockEnvironment = new Mock<IEnvironmentWrapper>(MockBehavior.Strict);

            var consoleWriter = new ConsoleWriter(
                mockConsole.Object,
                mockEnvironment.Object,
                suppressErrorOutput,
                enableAnsiProcessing,
                ansiResetType);

            // Act
            await consoleWriter.WriteItemAsync(
                mockItem.Object,
                type,
                expectedIndex,
                mockFormatter.Object,
                emitLineEnding);

            // Assert
            Assert.That(actualWrittenValue, Is.EqualTo(expectedFormattedText));

            mockItem.Verify();
            mockFormatter.Verify();
            mockStreamWriter.Verify();
            mockConsole.Verify();
            mockEnvironment.Verify();
        }

        [Test]
        public async Task WriteItemAsync_AnsiResetAutoWithAnsiDisabledAndOutputRedirected_EmitAnsiResetFalse()
        {
            // Arrange
            var ansiResetType = AnsiResetType.Auto;
            var enableAnsiProcessing = false;
            var expectedEmitAnsiReset = false;

            var type = ContentType.Text;
            var suppressErrorOutput = false;
            var emitLineEnding = false;
            var expectedIndex = 42;
            var expectedId = Guid.NewGuid();
            var expectedTimestamp = DateTimeOffset.Now;

            var valueText = $"{NativeConstants.ANSI_ESCAPE}[38;2;131;95;177m~meow~";
            var expectedFormattedText = "🐱";

            var dataPackage = new DataPackage();
            dataPackage.SetText(valueText);
            var mockItem = new Mock<IClipboardHistoryItemWrapper>(MockBehavior.Strict);
            mockItem.SetupGet(mock => mock.Content).Returns(dataPackage.GetView()).Verifiable();
            mockItem.SetupGet(mock => mock.Id).Returns(expectedId.ToString()).Verifiable();
            mockItem.SetupGet(mock => mock.Timestamp).Returns(expectedTimestamp).Verifiable();

            var mockFormatter = new Mock<IValueFormatter>(MockBehavior.Strict);
            mockFormatter
                .Setup(mock => mock.Format(
                    It.Is<string>(actualValue => actualValue == valueText),
                    It.Is<int?>(actualIndex => actualIndex == expectedIndex),
                    It.Is<string>(actualId => actualId == expectedId.ToString()),
                    It.Is<DateTimeOffset>(actualTimestamp => actualTimestamp == expectedTimestamp),
                    It.Is<bool>(actualEmitAnsiReset => actualEmitAnsiReset == expectedEmitAnsiReset),
                    It.Is<bool>(actualEmitLineEnding => actualEmitLineEnding == emitLineEnding)))
                .Returns(expectedFormattedText)
                .Verifiable();

            var mockStreamWriter = new Mock<IStandardStreamWriter>(MockBehavior.Strict);

            string? actualWrittenValue = null;
            mockStreamWriter
                .Setup(mock => mock.Write(It.IsAny<string?>()))
                .Callback<string?>(value => actualWrittenValue = value)
                .Verifiable();

            var mockConsole = new Mock<IConsole>(MockBehavior.Strict);
            mockConsole
                .SetupGet(mock => mock.Out)
                .Returns(mockStreamWriter.Object)
                .Verifiable();

            mockConsole
                .SetupGet(mock => mock.IsOutputRedirected)
                .Returns(true)
                .Verifiable();

            var mockEnvironment = new Mock<IEnvironmentWrapper>(MockBehavior.Strict);

            var consoleWriter = new ConsoleWriter(
                mockConsole.Object,
                mockEnvironment.Object,
                suppressErrorOutput,
                enableAnsiProcessing,
                ansiResetType);

            // Act
            await consoleWriter.WriteItemAsync(
                mockItem.Object,
                type,
                expectedIndex,
                mockFormatter.Object,
                emitLineEnding);

            // Assert
            Assert.That(actualWrittenValue, Is.EqualTo(expectedFormattedText));

            mockItem.Verify();
            mockFormatter.Verify();
            mockStreamWriter.Verify();
            mockConsole.Verify();
            mockEnvironment.Verify();
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase("8bit")]
        public async Task WriteItemAsync_AnsiResetAutoWithAnsiDisabledAndInvalidColorTerm_EmitAnsiResetFalse(string? invalidColorTermValue)
        {
            // Arrange
            var ansiResetType = AnsiResetType.Auto;
            var enableAnsiProcessing = false;
            var expectedEmitAnsiReset = false;

            var type = ContentType.Text;
            var suppressErrorOutput = false;
            var emitLineEnding = false;
            var expectedIndex = 42;
            var expectedId = Guid.NewGuid();
            var expectedTimestamp = DateTimeOffset.Now;

            var valueText = $"{NativeConstants.ANSI_ESCAPE}[38;2;131;95;177m~meow~";
            var expectedFormattedText = "🐱";

            var dataPackage = new DataPackage();
            dataPackage.SetText(valueText);
            var mockItem = new Mock<IClipboardHistoryItemWrapper>(MockBehavior.Strict);
            mockItem.SetupGet(mock => mock.Content).Returns(dataPackage.GetView()).Verifiable();
            mockItem.SetupGet(mock => mock.Id).Returns(expectedId.ToString()).Verifiable();
            mockItem.SetupGet(mock => mock.Timestamp).Returns(expectedTimestamp).Verifiable();

            var mockFormatter = new Mock<IValueFormatter>(MockBehavior.Strict);
            mockFormatter
                .Setup(mock => mock.Format(
                    It.Is<string>(actualValue => actualValue == valueText),
                    It.Is<int?>(actualIndex => actualIndex == expectedIndex),
                    It.Is<string>(actualId => actualId == expectedId.ToString()),
                    It.Is<DateTimeOffset>(actualTimestamp => actualTimestamp == expectedTimestamp),
                    It.Is<bool>(actualEmitAnsiReset => actualEmitAnsiReset == expectedEmitAnsiReset),
                    It.Is<bool>(actualEmitLineEnding => actualEmitLineEnding == emitLineEnding)))
                .Returns(expectedFormattedText)
                .Verifiable();

            var mockStreamWriter = new Mock<IStandardStreamWriter>(MockBehavior.Strict);

            string? actualWrittenValue = null;
            mockStreamWriter
                .Setup(mock => mock.Write(It.IsAny<string?>()))
                .Callback<string?>(value => actualWrittenValue = value)
                .Verifiable();

            var mockConsole = new Mock<IConsole>(MockBehavior.Strict);
            mockConsole
                .SetupGet(mock => mock.Out)
                .Returns(mockStreamWriter.Object)
                .Verifiable();

            mockConsole
                .SetupGet(mock => mock.IsOutputRedirected)
                .Returns(false)
                .Verifiable();

            var mockEnvironment = new Mock<IEnvironmentWrapper>(MockBehavior.Strict);
            mockEnvironment
                .Setup(mock => mock.GetEnvironmentVariable(It.Is<string>(actualVariableName => actualVariableName == "COLORTERM")))
                .Returns(invalidColorTermValue)
                .Verifiable();

            var consoleWriter = new ConsoleWriter(
                mockConsole.Object,
                mockEnvironment.Object,
                suppressErrorOutput,
                enableAnsiProcessing,
                ansiResetType);

            // Act
            await consoleWriter.WriteItemAsync(
                mockItem.Object,
                type,
                expectedIndex,
                mockFormatter.Object,
                emitLineEnding);

            // Assert
            Assert.That(actualWrittenValue, Is.EqualTo(expectedFormattedText));

            mockItem.Verify();
            mockFormatter.Verify();
            mockStreamWriter.Verify();
            mockConsole.Verify();
            mockEnvironment.Verify();
        }

        [Test]
        [TestCase("24bit")]
        [TestCase("truecolor")]
        public async Task WriteItemAsync_AnsiResetAutoWithAnsiDisabledAndValidColorTerm_EmitAnsiResetTrue(string? validColorTermValue)
        {
            // Arrange
            var ansiResetType = AnsiResetType.Auto;
            var enableAnsiProcessing = false;
            var expectedEmitAnsiReset = true;

            var type = ContentType.Text;
            var suppressErrorOutput = false;
            var emitLineEnding = false;
            var expectedIndex = 42;
            var expectedId = Guid.NewGuid();
            var expectedTimestamp = DateTimeOffset.Now;

            var valueText = $"{NativeConstants.ANSI_ESCAPE}[38;2;131;95;177m~meow~";
            var expectedFormattedText = "🐱";

            var dataPackage = new DataPackage();
            dataPackage.SetText(valueText);
            var mockItem = new Mock<IClipboardHistoryItemWrapper>(MockBehavior.Strict);
            mockItem.SetupGet(mock => mock.Content).Returns(dataPackage.GetView()).Verifiable();
            mockItem.SetupGet(mock => mock.Id).Returns(expectedId.ToString()).Verifiable();
            mockItem.SetupGet(mock => mock.Timestamp).Returns(expectedTimestamp).Verifiable();

            var mockFormatter = new Mock<IValueFormatter>(MockBehavior.Strict);
            mockFormatter
                .Setup(mock => mock.Format(
                    It.Is<string>(actualValue => actualValue == valueText),
                    It.Is<int?>(actualIndex => actualIndex == expectedIndex),
                    It.Is<string>(actualId => actualId == expectedId.ToString()),
                    It.Is<DateTimeOffset>(actualTimestamp => actualTimestamp == expectedTimestamp),
                    It.Is<bool>(actualEmitAnsiReset => actualEmitAnsiReset == expectedEmitAnsiReset),
                    It.Is<bool>(actualEmitLineEnding => actualEmitLineEnding == emitLineEnding)))
                .Returns(expectedFormattedText)
                .Verifiable();

            var mockStreamWriter = new Mock<IStandardStreamWriter>(MockBehavior.Strict);

            string? actualWrittenValue = null;
            mockStreamWriter
                .Setup(mock => mock.Write(It.IsAny<string?>()))
                .Callback<string?>(value => actualWrittenValue = value)
                .Verifiable();

            var mockConsole = new Mock<IConsole>(MockBehavior.Strict);
            mockConsole
                .SetupGet(mock => mock.Out)
                .Returns(mockStreamWriter.Object)
                .Verifiable();

            mockConsole
                .SetupGet(mock => mock.IsOutputRedirected)
                .Returns(false)
                .Verifiable();

            var mockEnvironment = new Mock<IEnvironmentWrapper>(MockBehavior.Strict);
            mockEnvironment
                .Setup(mock => mock.GetEnvironmentVariable(It.Is<string>(actualVariableName => actualVariableName == "COLORTERM")))
                .Returns(validColorTermValue)
                .Verifiable();

            var consoleWriter = new ConsoleWriter(
                mockConsole.Object,
                mockEnvironment.Object,
                suppressErrorOutput,
                enableAnsiProcessing,
                ansiResetType);

            // Act
            await consoleWriter.WriteItemAsync(
                mockItem.Object,
                type,
                expectedIndex,
                mockFormatter.Object,
                emitLineEnding);

            // Assert
            Assert.That(actualWrittenValue, Is.EqualTo(expectedFormattedText));

            mockItem.Verify();
            mockFormatter.Verify();
            mockStreamWriter.Verify();
            mockConsole.Verify();
            mockEnvironment.Verify();
        }
        #endregion WriteItemAsync

        #region WriteValue
        [Test]
        public void WriteValue_NullValue_NoOutput()

        {
            // Arrange
            var ansiResetType = AnsiResetType.On;
            bool enableAnsiProcessing = false;
            var suppressErrorOutput = false;

            var mockFormatter = new Mock<IValueFormatter>(MockBehavior.Strict);
            var mockConsole = new Mock<IConsole>(MockBehavior.Strict);
            var mockEnvironment = new Mock<IEnvironmentWrapper>(MockBehavior.Strict);

            var consoleWriter = new ConsoleWriter(
                mockConsole.Object,
                mockEnvironment.Object,
                suppressErrorOutput,
                enableAnsiProcessing,
                ansiResetType);

            // Act
            consoleWriter.WriteValue(null!, mockFormatter.Object);

            // NOTE: No asserts needed for this test, as it will fail if something is written to the console since
            // no setup was proved for the output stream and the console mock is created with MockBehavior.Strict

            mockConsole.Verify();
            mockEnvironment.Verify();
        }

        [Test]
        public void WriteValue_NonNullFormatter_WritesFormattedValue()
        {
            // Arrange
            var ansiResetType = AnsiResetType.Off;
            bool enableAnsiProcessing = false;
            var suppressErrorOutput = false;

            var valueText = "🦄";
            var expectedFormattedText = "🦄🦄🦄🦄🦄";

            var mockFormatter = new Mock<IValueFormatter>(MockBehavior.Strict);
            mockFormatter
                .Setup(mock => mock.Format(
                    It.Is<string>(actualValue => actualValue == valueText),
                    It.Is<bool>(actualEmitAnsiReset => actualEmitAnsiReset == false),
                    It.Is<bool>(actualEmitLineEnding => actualEmitLineEnding == false)))
                .Returns(expectedFormattedText)
                .Verifiable();

            var mockStreamWriter = new Mock<IStandardStreamWriter>(MockBehavior.Strict);

            string? actualWrittenValue = null;
            mockStreamWriter
                .Setup(mock => mock.Write(It.IsAny<string?>()))
                .Callback<string?>(value => actualWrittenValue = value)
                .Verifiable();

            var mockConsole = new Mock<IConsole>(MockBehavior.Strict);
            mockConsole
                .SetupGet(mock => mock.Out)
                .Returns(mockStreamWriter.Object)
                .Verifiable();

            var mockEnvironment = new Mock<IEnvironmentWrapper>(MockBehavior.Strict);

            var consoleWriter = new ConsoleWriter(
                mockConsole.Object,
                mockEnvironment.Object,
                suppressErrorOutput,
                enableAnsiProcessing,
                ansiResetType);

            // Act
            consoleWriter.WriteValue(valueText, mockFormatter.Object);

            // Assert
            Assert.That(actualWrittenValue, Is.EqualTo(expectedFormattedText));

            mockFormatter.Verify();
            mockStreamWriter.Verify();
            mockConsole.Verify();
            mockEnvironment.Verify();
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void WriteValue_AnsiResetOff_EmitAnsiResetFalse(bool enableAnsiProcessing)
        {
            // Arrange
            var ansiResetType = AnsiResetType.Off;
            var expectedEmitAnsiReset = false;

            var suppressErrorOutput = false;

            var valueText = "🦄";
            var expectedFormattedText = "🦄🦄🦄🦄🦄";

            var mockFormatter = new Mock<IValueFormatter>(MockBehavior.Strict);
            mockFormatter
                .Setup(mock => mock.Format(
                    It.Is<string>(actualValue => actualValue == valueText),
                    It.Is<bool>(actualEmitAnsiReset => actualEmitAnsiReset == expectedEmitAnsiReset),
                    It.Is<bool>(actualEmitLineEnding => actualEmitLineEnding == false)))
                .Returns(expectedFormattedText)
                .Verifiable();

            var mockStreamWriter = new Mock<IStandardStreamWriter>(MockBehavior.Strict);

            string? actualWrittenValue = null;
            mockStreamWriter
                .Setup(mock => mock.Write(It.IsAny<string?>()))
                .Callback<string?>(value => actualWrittenValue = value)
                .Verifiable();

            var mockConsole = new Mock<IConsole>(MockBehavior.Strict);
            mockConsole
                .SetupGet(mock => mock.Out)
                .Returns(mockStreamWriter.Object)
                .Verifiable();

            var mockEnvironment = new Mock<IEnvironmentWrapper>(MockBehavior.Strict);

            var consoleWriter = new ConsoleWriter(
                mockConsole.Object,
                mockEnvironment.Object,
                suppressErrorOutput,
                enableAnsiProcessing,
                ansiResetType);

            // Act
            consoleWriter.WriteValue(valueText, mockFormatter.Object);

            // Assert
            Assert.That(actualWrittenValue, Is.EqualTo(expectedFormattedText));

            mockFormatter.Verify();
            mockStreamWriter.Verify();
            mockConsole.Verify();
            mockEnvironment.Verify();
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void WriteValue_AnsiResetOn_EmitAnsiResetTrue(bool enableAnsiProcessing)
        {
            // Arrange
            var ansiResetType = AnsiResetType.On;
            var expectedEmitAnsiReset = true;

            var suppressErrorOutput = false;

            var valueText = "🦄";
            var expectedFormattedText = "🦄🦄🦄🦄🦄";

            var mockFormatter = new Mock<IValueFormatter>(MockBehavior.Strict);
            mockFormatter
                .Setup(mock => mock.Format(
                    It.Is<string>(actualValue => actualValue == valueText),
                    It.Is<bool>(actualEmitAnsiReset => actualEmitAnsiReset == expectedEmitAnsiReset),
                    It.Is<bool>(actualEmitLineEnding => actualEmitLineEnding == false)))
                .Returns(expectedFormattedText)
                .Verifiable();

            var mockStreamWriter = new Mock<IStandardStreamWriter>(MockBehavior.Strict);

            string? actualWrittenValue = null;
            mockStreamWriter
                .Setup(mock => mock.Write(It.IsAny<string?>()))
                .Callback<string?>(value => actualWrittenValue = value)
                .Verifiable();

            var mockConsole = new Mock<IConsole>(MockBehavior.Strict);
            mockConsole
                .SetupGet(mock => mock.Out)
                .Returns(mockStreamWriter.Object)
                .Verifiable();

            var mockEnvironment = new Mock<IEnvironmentWrapper>(MockBehavior.Strict);

            var consoleWriter = new ConsoleWriter(
                mockConsole.Object,
                mockEnvironment.Object,
                suppressErrorOutput,
                enableAnsiProcessing,
                ansiResetType);

            // Act
            consoleWriter.WriteValue(valueText, mockFormatter.Object);

            // Assert
            Assert.That(actualWrittenValue, Is.EqualTo(expectedFormattedText));

            mockFormatter.Verify();
            mockStreamWriter.Verify();
            mockConsole.Verify();
            mockEnvironment.Verify();
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void WriteValue_AnsiResetAutoWithAnsiEnabled_EmitAnsiResetTrueIfContainsAnsi(bool valueContainsAnsi)
        {
            // Arrange
            var ansiResetType = AnsiResetType.Auto;
            var enableAnsiProcessing = true;
            var expectedEmitAnsiReset = valueContainsAnsi;

            var suppressErrorOutput = false;

            var valueText = "🦄";
            var expectedFormattedText = "🦄🦄🦄🦄🦄";
            if (valueContainsAnsi)
            {
                valueText = $"{NativeConstants.ANSI_ESCAPE}[38;2;131;95;177m{valueText}";
                expectedFormattedText += NativeConstants.ANSI_RESET;
            }

            var mockFormatter = new Mock<IValueFormatter>(MockBehavior.Strict);
            mockFormatter
                .Setup(mock => mock.Format(
                    It.Is<string>(actualValue => actualValue == valueText),
                    It.Is<bool>(actualEmitAnsiReset => actualEmitAnsiReset == expectedEmitAnsiReset),
                    It.Is<bool>(actualEmitLineEnding => actualEmitLineEnding == false)))
                .Returns(expectedFormattedText)
                .Verifiable();

            var mockStreamWriter = new Mock<IStandardStreamWriter>(MockBehavior.Strict);

            string? actualWrittenValue = null;
            mockStreamWriter
                .Setup(mock => mock.Write(It.IsAny<string?>()))
                .Callback<string?>(value => actualWrittenValue = value)
                .Verifiable();

            var mockConsole = new Mock<IConsole>(MockBehavior.Strict);
            mockConsole
                .SetupGet(mock => mock.Out)
                .Returns(mockStreamWriter.Object)
                .Verifiable();

            var mockEnvironment = new Mock<IEnvironmentWrapper>(MockBehavior.Strict);

            var consoleWriter = new ConsoleWriter(
                mockConsole.Object,
                mockEnvironment.Object,
                suppressErrorOutput,
                enableAnsiProcessing,
                ansiResetType);

            // Act
            consoleWriter.WriteValue(valueText, mockFormatter.Object);

            // Assert
            Assert.That(actualWrittenValue, Is.EqualTo(expectedFormattedText));

            mockFormatter.Verify();
            mockStreamWriter.Verify();
            mockConsole.Verify();
            mockEnvironment.Verify();
        }

        [Test]
        public void WriteValue_AnsiResetAutoWithAnsiDisabledAndOutputRedirected_EmitAnsiResetFalse()
        {
            // Arrange
            var ansiResetType = AnsiResetType.Auto;
            var enableAnsiProcessing = false;
            var expectedEmitAnsiReset = false;

            var suppressErrorOutput = false;

            var valueText = $"{NativeConstants.ANSI_ESCAPE}[38;2;131;95;177m🦄";
            var expectedFormattedText = "🦄🦄🦄🦄🦄";

            var mockFormatter = new Mock<IValueFormatter>(MockBehavior.Strict);
            mockFormatter
                .Setup(mock => mock.Format(
                    It.Is<string>(actualValue => actualValue == valueText),
                    It.Is<bool>(actualEmitAnsiReset => actualEmitAnsiReset == expectedEmitAnsiReset),
                    It.Is<bool>(actualEmitLineEnding => actualEmitLineEnding == false)))
                .Returns(expectedFormattedText)
                .Verifiable();

            var mockStreamWriter = new Mock<IStandardStreamWriter>(MockBehavior.Strict);

            string? actualWrittenValue = null;
            mockStreamWriter
                .Setup(mock => mock.Write(It.IsAny<string?>()))
                .Callback<string?>(value => actualWrittenValue = value)
                .Verifiable();

            var mockConsole = new Mock<IConsole>(MockBehavior.Strict);
            mockConsole
                .SetupGet(mock => mock.Out)
                .Returns(mockStreamWriter.Object)
                .Verifiable();

            mockConsole
                .SetupGet(mock => mock.IsOutputRedirected)
                .Returns(true)
                .Verifiable();

            var mockEnvironment = new Mock<IEnvironmentWrapper>(MockBehavior.Strict);

            var consoleWriter = new ConsoleWriter(
                mockConsole.Object,
                mockEnvironment.Object,
                suppressErrorOutput,
                enableAnsiProcessing,
                ansiResetType);

            // Act
            consoleWriter.WriteValue(valueText, mockFormatter.Object);

            // Assert
            Assert.That(actualWrittenValue, Is.EqualTo(expectedFormattedText));

            mockFormatter.Verify();
            mockStreamWriter.Verify();
            mockConsole.Verify();
            mockEnvironment.Verify();
        }

        [Test]
        [TestCase(null)]
        [TestCase("")]
        [TestCase("8bit")]
        public void WriteValue_AnsiResetAutoWithAnsiDisabledAndInvalidColorTerm_EmitAnsiResetFalse(string? invalidColorTermValue)
        {
            // Arrange
            var ansiResetType = AnsiResetType.Auto;
            var enableAnsiProcessing = false;
            var expectedEmitAnsiReset = false;

            var suppressErrorOutput = false;

            var valueText = $"{NativeConstants.ANSI_ESCAPE}[38;2;131;95;177m🦄";
            var expectedFormattedText = "🦄🦄🦄🦄🦄";

            var mockFormatter = new Mock<IValueFormatter>(MockBehavior.Strict);
            mockFormatter
                .Setup(mock => mock.Format(
                    It.Is<string>(actualValue => actualValue == valueText),
                    It.Is<bool>(actualEmitAnsiReset => actualEmitAnsiReset == expectedEmitAnsiReset),
                    It.Is<bool>(actualEmitLineEnding => actualEmitLineEnding == false)))
                .Returns(expectedFormattedText)
                .Verifiable();

            var mockStreamWriter = new Mock<IStandardStreamWriter>(MockBehavior.Strict);

            string? actualWrittenValue = null;
            mockStreamWriter
                .Setup(mock => mock.Write(It.IsAny<string?>()))
                .Callback<string?>(value => actualWrittenValue = value)
                .Verifiable();

            var mockConsole = new Mock<IConsole>(MockBehavior.Strict);
            mockConsole
                .SetupGet(mock => mock.Out)
                .Returns(mockStreamWriter.Object)
                .Verifiable();

            mockConsole
                .SetupGet(mock => mock.IsOutputRedirected)
                .Returns(false)
                .Verifiable();

            var mockEnvironment = new Mock<IEnvironmentWrapper>(MockBehavior.Strict);
            mockEnvironment
                .Setup(mock => mock.GetEnvironmentVariable(It.Is<string>(actualVariableName => actualVariableName == "COLORTERM")))
                .Returns(invalidColorTermValue)
                .Verifiable();

            var consoleWriter = new ConsoleWriter(
                mockConsole.Object,
                mockEnvironment.Object,
                suppressErrorOutput,
                enableAnsiProcessing,
                ansiResetType);

            // Act
            consoleWriter.WriteValue(valueText, mockFormatter.Object);

            // Assert
            Assert.That(actualWrittenValue, Is.EqualTo(expectedFormattedText));

            mockFormatter.Verify();
            mockStreamWriter.Verify();
            mockConsole.Verify();
            mockEnvironment.Verify();
        }

        [Test]
        [TestCase("24bit")]
        [TestCase("truecolor")]
        public void WriteValue_AnsiResetAutoWithAnsiDisabledAndValidColorTerm_EmitAnsiResetTrue(string? validColorTermValue)
        {
            // Arrange
            var ansiResetType = AnsiResetType.Auto;
            var enableAnsiProcessing = false;
            var expectedEmitAnsiReset = true;

            var suppressErrorOutput = false;

            var valueText = $"{NativeConstants.ANSI_ESCAPE}[38;2;131;95;177m🦄";
            var expectedFormattedText = "🦄🦄🦄🦄🦄";

            var mockFormatter = new Mock<IValueFormatter>(MockBehavior.Strict);
            mockFormatter
                .Setup(mock => mock.Format(
                    It.Is<string>(actualValue => actualValue == valueText),
                    It.Is<bool>(actualEmitAnsiReset => actualEmitAnsiReset == expectedEmitAnsiReset),
                    It.Is<bool>(actualEmitLineEnding => actualEmitLineEnding == false)))
                .Returns(expectedFormattedText)
                .Verifiable();

            var mockStreamWriter = new Mock<IStandardStreamWriter>(MockBehavior.Strict);

            string? actualWrittenValue = null;
            mockStreamWriter
                .Setup(mock => mock.Write(It.IsAny<string?>()))
                .Callback<string?>(value => actualWrittenValue = value)
                .Verifiable();

            var mockConsole = new Mock<IConsole>(MockBehavior.Strict);
            mockConsole
                .SetupGet(mock => mock.Out)
                .Returns(mockStreamWriter.Object)
                .Verifiable();

            mockConsole
                .SetupGet(mock => mock.IsOutputRedirected)
                .Returns(false)
                .Verifiable();

            var mockEnvironment = new Mock<IEnvironmentWrapper>(MockBehavior.Strict);
            mockEnvironment
                .Setup(mock => mock.GetEnvironmentVariable(It.Is<string>(actualVariableName => actualVariableName == "COLORTERM")))
                .Returns(validColorTermValue)
                .Verifiable();

            var consoleWriter = new ConsoleWriter(
                mockConsole.Object,
                mockEnvironment.Object,
                suppressErrorOutput,
                enableAnsiProcessing,
                ansiResetType);

            // Act
            consoleWriter.WriteValue(valueText, mockFormatter.Object);

            // Assert
            Assert.That(actualWrittenValue, Is.EqualTo(expectedFormattedText));

            mockFormatter.Verify();
            mockStreamWriter.Verify();
            mockConsole.Verify();
            mockEnvironment.Verify();
        }
        #endregion WriteValue

        #region WriteLine
        [Test]
        [TestCase("")]
        [TestCase("    ")]
        [TestCase("🐢")]
        public void WriteLine_WritesValueWithNewLineToOutStream(string valueToWrite)
        {
            // Arrange
            var expectedWrittenValue = $"{valueToWrite}{Environment.NewLine}";
            var ansiResetType = AnsiResetType.Off;
            bool enableAnsiProcessing = false;
            var suppressErrorOutput = false;

            var mockStreamWriter = new Mock<IStandardStreamWriter>(MockBehavior.Strict);

            string? actualWrittenValue = null;
            mockStreamWriter
                .Setup(mock => mock.Write(It.IsAny<string?>()))
                .Callback<string?>(value => actualWrittenValue = value)
                .Verifiable();

            var mockConsole = new Mock<IConsole>(MockBehavior.Strict);
            mockConsole
                .SetupGet(mock => mock.Out)
                .Returns(mockStreamWriter.Object)
                .Verifiable();

            var mockEnvironment = new Mock<IEnvironmentWrapper>(MockBehavior.Strict);

            var consoleWriter = new ConsoleWriter(
                mockConsole.Object,
                mockEnvironment.Object,
                suppressErrorOutput,
                enableAnsiProcessing,
                ansiResetType);

            // Act
            consoleWriter.WriteLine(valueToWrite);

            // Assert
            Assert.That(actualWrittenValue, Is.EqualTo(expectedWrittenValue));

            mockStreamWriter.Verify();
            mockConsole.Verify();
            mockEnvironment.Verify();
        }
        #endregion WriteLine

        #region WriteErrorLine
        [Test]
        [TestCase("")]
        [TestCase("    ")]
        [TestCase("🐢")]
        public void WriteLine_SuppressErrorOutputFalse_WritesValueWithNewLineToErrorStream(string valueToWrite)
        {
            // Arrange
            var expectedWrittenValue = $"{valueToWrite}{Environment.NewLine}";
            var ansiResetType = AnsiResetType.Off;
            bool enableAnsiProcessing = false;
            var suppressErrorOutput = false;

            var mockStreamWriter = new Mock<IStandardStreamWriter>(MockBehavior.Strict);

            string? actualWrittenValue = null;
            mockStreamWriter
                .Setup(mock => mock.Write(It.IsAny<string?>()))
                .Callback<string?>(value => actualWrittenValue = value)
                .Verifiable();

            var mockConsole = new Mock<IConsole>(MockBehavior.Strict);
            mockConsole
                .SetupGet(mock => mock.Error)
                .Returns(mockStreamWriter.Object)
                .Verifiable();

            var mockEnvironment = new Mock<IEnvironmentWrapper>(MockBehavior.Strict);

            var consoleWriter = new ConsoleWriter(
                mockConsole.Object,
                mockEnvironment.Object,
                suppressErrorOutput,
                enableAnsiProcessing,
                ansiResetType);

            // Act
            consoleWriter.WriteErrorLine(valueToWrite);

            // Assert
            Assert.That(actualWrittenValue, Is.EqualTo(expectedWrittenValue));

            mockStreamWriter.Verify();
            mockConsole.Verify();
            mockEnvironment.Verify();
        }

        [Test]
        public void WriteLine_SuppressErrorOutputTrue_NoOutput()
        {
            // Arrange
            var valueToWrite = "🐢";
            var ansiResetType = AnsiResetType.Off;
            bool enableAnsiProcessing = false;
            var suppressErrorOutput = true;
            var mockConsole = new Mock<IConsole>(MockBehavior.Strict);
            var mockEnvironment = new Mock<IEnvironmentWrapper>(MockBehavior.Strict);

            var consoleWriter = new ConsoleWriter(
                mockConsole.Object,
                mockEnvironment.Object,
                suppressErrorOutput,
                enableAnsiProcessing,
                ansiResetType);

            // Act
            consoleWriter.WriteErrorLine(valueToWrite);

            // NOTE: No asserts needed for this test, as it will fail if something is written to the console since
            // no setup was proved for the error stream and the console mock is created with MockBehavior.Strict

            mockConsole.Verify();
            mockEnvironment.Verify();
        }
        #endregion WriteErrorLine

        #region Helpers
        public static IEnumerable<TestCaseData> ContructorTestCases()
        {
            foreach (var ansiResetType in Enum.GetValues<AnsiResetType>())
            {
                yield return new TestCaseData(true, true, ansiResetType);
                yield return new TestCaseData(true, false, ansiResetType);
                yield return new TestCaseData(false, true, ansiResetType);
                yield return new TestCaseData(false, false, ansiResetType);
            }
        }
        #endregion Helpers
    }
}
