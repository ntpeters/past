using Moq;
using past.ConsoleApp.Wrappers;

namespace past.ConsoleApp.Test
{
    public class ConsoleUtilitiesTests
    {
        #region Constructors
        [Test]
        public void Create_Parmeterless_Success()
        {
            Assert.DoesNotThrow(() => new ConsoleUtilities());
        }

        [Test]
        public void Create_ValidParameters_Success()
        {
            var mockNativeMethods = new Mock<INativeMethodsWrapper>(MockBehavior.Strict);
            Assert.DoesNotThrow(() => new ConsoleUtilities(mockNativeMethods.Object));
        }

        [Test]
        public void Create_NullNativeMethods_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ConsoleUtilities(null!));
        }
        #endregion Constructors

        #region TryEnableVirtualTerminalProcessing
        [Test]
        public void TryEnableVirtualTerminalProcessing_Success_ReturnsTrue()
        {
            // Arrange
            var expectedHandle = new IntPtr(9001);
            var initialConsoleMode = NativeConstants.ENABLE_WRAP_AT_EOL_OUTPUT;
            var expectedConsoleMode = initialConsoleMode | NativeConstants.ENABLE_VIRTUAL_TERMINAL_PROCESSING | NativeConstants.DISABLE_NEWLINE_AUTO_RETURN;

            var mockNativeMethods = new Mock<INativeMethodsWrapper>(MockBehavior.Strict);
            mockNativeMethods
                .Setup(mock => mock.GetStdHandle(It.Is<int>(actualHandleDescriptor => actualHandleDescriptor == NativeConstants.STD_OUTPUT_HANDLE)))
                .Returns(expectedHandle)
                .Verifiable();

            mockNativeMethods
                .Setup(mock => mock.GetConsoleMode(
                    It.Is<IntPtr>(actualHandle => actualHandle == expectedHandle),
                    out It.Ref<uint>.IsAny))
                .Returns((IntPtr handle, ref uint currentConsoleMode) =>
                {
                    currentConsoleMode = initialConsoleMode;
                    return true;
                })
                .Verifiable();

            mockNativeMethods
                .Setup(mock => mock.SetConsoleMode(
                    It.Is<IntPtr>(actualHandle => actualHandle == expectedHandle),
                    It.Is<uint>(actualConsoleMode => actualConsoleMode == expectedConsoleMode)))
                .Returns(true)
                .Verifiable();

            var consoleUtilities = new ConsoleUtilities(mockNativeMethods.Object);

            // Act
            var result = consoleUtilities.TryEnableVirtualTerminalProcessing(out var actualError);

            // Assert
            Assert.That(result, Is.True);
            Assert.That(actualError, Is.Null);

            mockNativeMethods.Verify();
        }

        [Test]
        public void TryEnableVirtualTerminalProcessing_GetHandleFails_ReturnsFalseWithErrorMessage()
        {
            // Arrange
            uint errorCode = 42;
            var errorMessage = "Well that's not good.";
            var expectedErrorMessage = $"Error {errorCode}: {errorMessage}";
            uint expectedFormatFlags = NativeConstants.FORMAT_MESSAGE_ALLOCATE_BUFFER | NativeConstants.FORMAT_MESSAGE_FROM_SYSTEM | NativeConstants.FORMAT_MESSAGE_IGNORE_INSERTS;

            var mockNativeMethods = new Mock<INativeMethodsWrapper>(MockBehavior.Strict);
            mockNativeMethods
                .Setup(mock => mock.GetStdHandle(It.Is<int>(actualHandleDescriptor => actualHandleDescriptor == NativeConstants.STD_OUTPUT_HANDLE)))
                .Returns(NativeConstants.INVALID_HANDLE_VALUE)
                .Verifiable();

            mockNativeMethods
                .Setup(mock => mock.GetLastError())
                .Returns(errorCode)
                .Verifiable();

            mockNativeMethods
                .Setup(mock => mock.FormatMessage(
                    It.Is<uint>(actualFlags => actualFlags == expectedFormatFlags),
                    It.Is<IntPtr>(lpSource => lpSource == IntPtr.Zero),
                    It.Is<uint>(actualErrorCode => actualErrorCode == errorCode),
                    It.Is<uint>(dwLanguageId => dwLanguageId == 0),
                    out It.Ref<string>.IsAny,
                    It.Is<uint>(nSize => nSize == 0),
                    It.Is<IntPtr>(arguments => arguments == IntPtr.Zero)))
                .Returns((uint dwFlags, IntPtr lpSource, uint dwMessageId, uint dwLanguageId, out string lpBuffer, uint nSize, IntPtr arguments) =>
                {
                    lpBuffer = errorMessage;
                    return (uint)errorMessage.Length;
                })
                .Verifiable();

            var consoleUtilities = new ConsoleUtilities(mockNativeMethods.Object);

            // Act
            var result = consoleUtilities.TryEnableVirtualTerminalProcessing(out var actualError);

            // Assert
            Assert.That(result, Is.False);
            Assert.That(actualError, Is.EqualTo(expectedErrorMessage));

            mockNativeMethods.Verify();
        }

        [Test]
        public void TryEnableVirtualTerminalProcessing_GetConsoleModeFails_ReturnsFalseWithErrorMessage()
        {
            // Arrange
            uint errorCode = 42;
            var errorMessage = "Well that's not good.";
            var expectedErrorMessage = $"Error {errorCode}: {errorMessage}";
            var expectedHandle = new IntPtr(9001);
            uint expectedFormatFlags = NativeConstants.FORMAT_MESSAGE_ALLOCATE_BUFFER | NativeConstants.FORMAT_MESSAGE_FROM_SYSTEM | NativeConstants.FORMAT_MESSAGE_IGNORE_INSERTS;

            var mockNativeMethods = new Mock<INativeMethodsWrapper>(MockBehavior.Strict);
            mockNativeMethods
                .Setup(mock => mock.GetStdHandle(It.Is<int>(actualHandleDescriptor => actualHandleDescriptor == NativeConstants.STD_OUTPUT_HANDLE)))
                .Returns(expectedHandle)
                .Verifiable();

            mockNativeMethods
                .Setup(mock => mock.GetConsoleMode(
                    It.Is<IntPtr>(actualHandle => actualHandle == expectedHandle),
                    out It.Ref<uint>.IsAny))
                .Returns((IntPtr handle, ref uint currentConsoleMode) =>
                {
                    currentConsoleMode = 0;
                    return false;
                })
                .Verifiable();

            mockNativeMethods
                .Setup(mock => mock.GetLastError())
                .Returns(errorCode)
                .Verifiable();

            mockNativeMethods
                .Setup(mock => mock.FormatMessage(
                    It.Is<uint>(actualFlags => actualFlags == expectedFormatFlags),
                    It.Is<IntPtr>(lpSource => lpSource == IntPtr.Zero),
                    It.Is<uint>(actualErrorCode => actualErrorCode == errorCode),
                    It.Is<uint>(dwLanguageId => dwLanguageId == 0),
                    out It.Ref<string>.IsAny,
                    It.Is<uint>(nSize => nSize == 0),
                    It.Is<IntPtr>(arguments => arguments == IntPtr.Zero)))
                .Returns((uint dwFlags, IntPtr lpSource, uint dwMessageId, uint dwLanguageId, out string lpBuffer, uint nSize, IntPtr arguments) =>
                {
                    lpBuffer = errorMessage;
                    return (uint)errorMessage.Length;
                })
                .Verifiable();

            var consoleUtilities = new ConsoleUtilities(mockNativeMethods.Object);

            // Act
            var result = consoleUtilities.TryEnableVirtualTerminalProcessing(out var actualError);

            // Assert
            Assert.That(result, Is.False);
            Assert.That(actualError, Is.EqualTo(expectedErrorMessage));

            mockNativeMethods.Verify();
        }

        [Test]
        public void TryEnableVirtualTerminalProcessing_SetConsoleModeFails_ReturnsFalseWithErrorMessage()
        {
            // Arrange
            uint errorCode = 42;
            var errorMessage = "Well that's not good.";
            var expectedErrorMessage = $"Error {errorCode}: {errorMessage}";
            var expectedHandle = new IntPtr(9001);
            var initialConsoleMode = NativeConstants.ENABLE_WRAP_AT_EOL_OUTPUT;
            var expectedConsoleMode = initialConsoleMode | NativeConstants.ENABLE_VIRTUAL_TERMINAL_PROCESSING | NativeConstants.DISABLE_NEWLINE_AUTO_RETURN;
            uint expectedFormatFlags = NativeConstants.FORMAT_MESSAGE_ALLOCATE_BUFFER | NativeConstants.FORMAT_MESSAGE_FROM_SYSTEM | NativeConstants.FORMAT_MESSAGE_IGNORE_INSERTS;

            var mockNativeMethods = new Mock<INativeMethodsWrapper>(MockBehavior.Strict);
            mockNativeMethods
                .Setup(mock => mock.GetStdHandle(It.Is<int>(actualHandleDescriptor => actualHandleDescriptor == NativeConstants.STD_OUTPUT_HANDLE)))
                .Returns(expectedHandle)
                .Verifiable();

            mockNativeMethods
                .Setup(mock => mock.GetConsoleMode(
                    It.Is<IntPtr>(actualHandle => actualHandle == expectedHandle),
                    out It.Ref<uint>.IsAny))
                .Returns((IntPtr handle, ref uint currentConsoleMode) =>
                {
                    currentConsoleMode = initialConsoleMode;
                    return true;
                })
                .Verifiable();

            mockNativeMethods
                .Setup(mock => mock.SetConsoleMode(
                    It.Is<IntPtr>(actualHandle => actualHandle == expectedHandle),
                    It.Is<uint>(actualConsoleMode => actualConsoleMode == expectedConsoleMode)))
                .Returns(false)
                .Verifiable();

            mockNativeMethods
                .Setup(mock => mock.GetLastError())
                .Returns(errorCode)
                .Verifiable();

            mockNativeMethods
                .Setup(mock => mock.FormatMessage(
                    It.Is<uint>(actualFlags => actualFlags == expectedFormatFlags),
                    It.Is<IntPtr>(lpSource => lpSource == IntPtr.Zero),
                    It.Is<uint>(actualErrorCode => actualErrorCode == errorCode),
                    It.Is<uint>(dwLanguageId => dwLanguageId == 0),
                    out It.Ref<string>.IsAny,
                    It.Is<uint>(nSize => nSize == 0),
                    It.Is<IntPtr>(arguments => arguments == IntPtr.Zero)))
                .Returns((uint dwFlags, IntPtr lpSource, uint dwMessageId, uint dwLanguageId, out string lpBuffer, uint nSize, IntPtr arguments) =>
                {
                    lpBuffer = errorMessage;
                    return (uint)errorMessage.Length;
                })
                .Verifiable();

            var consoleUtilities = new ConsoleUtilities(mockNativeMethods.Object);

            // Act
            var result = consoleUtilities.TryEnableVirtualTerminalProcessing(out var actualError);

            // Assert
            Assert.That(result, Is.False);
            Assert.That(actualError, Is.EqualTo(expectedErrorMessage));

            mockNativeMethods.Verify();
        }
        #endregion TryEnableVirtualTerminalProcessing

        #region TryEnableVirtualTerminalInput
        [Test]
        public void TryEnableVirtualTerminalInput_Success_ReturnsTrue()
        {
            // Arrange
            var expectedHandle = new IntPtr(9001);
            uint initialConsoleMode = 0;
            var expectedConsoleMode = initialConsoleMode | NativeConstants.ENABLE_VIRTUAL_TERMINAL_INPUT;

            var mockNativeMethods = new Mock<INativeMethodsWrapper>(MockBehavior.Strict);
            mockNativeMethods
                .Setup(mock => mock.GetStdHandle(It.Is<int>(actualHandleDescriptor => actualHandleDescriptor == NativeConstants.STD_INPUT_HANDLE)))
                .Returns(expectedHandle)
                .Verifiable();

            mockNativeMethods
                .Setup(mock => mock.GetConsoleMode(
                    It.Is<IntPtr>(actualHandle => actualHandle == expectedHandle),
                    out It.Ref<uint>.IsAny))
                .Returns((IntPtr handle, ref uint currentConsoleMode) =>
                {
                    currentConsoleMode = initialConsoleMode;
                    return true;
                })
                .Verifiable();

            mockNativeMethods
                .Setup(mock => mock.SetConsoleMode(
                    It.Is<IntPtr>(actualHandle => actualHandle == expectedHandle),
                    It.Is<uint>(actualConsoleMode => actualConsoleMode == expectedConsoleMode)))
                .Returns(true)
                .Verifiable();

            var consoleUtilities = new ConsoleUtilities(mockNativeMethods.Object);

            // Act
            var result = consoleUtilities.TryEnableVirtualTerminalInput(out var actualError);

            // Assert
            Assert.That(result, Is.True);
            Assert.That(actualError, Is.Null);

            mockNativeMethods.Verify();
        }

        [Test]
        public void TryEnableVirtualTerminalInput_GetHandleFails_ReturnsFalseWithErrorMessage()
        {
            // Arrange
            uint errorCode = 42;
            var errorMessage = "Well that's not good.";
            var expectedErrorMessage = $"Error {errorCode}: {errorMessage}";
            uint expectedFormatFlags = NativeConstants.FORMAT_MESSAGE_ALLOCATE_BUFFER | NativeConstants.FORMAT_MESSAGE_FROM_SYSTEM | NativeConstants.FORMAT_MESSAGE_IGNORE_INSERTS;

            var mockNativeMethods = new Mock<INativeMethodsWrapper>(MockBehavior.Strict);
            mockNativeMethods
                .Setup(mock => mock.GetStdHandle(It.Is<int>(actualHandleDescriptor => actualHandleDescriptor == NativeConstants.STD_INPUT_HANDLE)))
                .Returns(NativeConstants.INVALID_HANDLE_VALUE)
                .Verifiable();

            mockNativeMethods
                .Setup(mock => mock.GetLastError())
                .Returns(errorCode)
                .Verifiable();

            mockNativeMethods
                .Setup(mock => mock.FormatMessage(
                    It.Is<uint>(actualFlags => actualFlags == expectedFormatFlags),
                    It.Is<IntPtr>(lpSource => lpSource == IntPtr.Zero),
                    It.Is<uint>(actualErrorCode => actualErrorCode == errorCode),
                    It.Is<uint>(dwLanguageId => dwLanguageId == 0),
                    out It.Ref<string>.IsAny,
                    It.Is<uint>(nSize => nSize == 0),
                    It.Is<IntPtr>(arguments => arguments == IntPtr.Zero)))
                .Returns((uint dwFlags, IntPtr lpSource, uint dwMessageId, uint dwLanguageId, out string lpBuffer, uint nSize, IntPtr arguments) =>
                {
                    lpBuffer = errorMessage;
                    return (uint)errorMessage.Length;
                })
                .Verifiable();

            var consoleUtilities = new ConsoleUtilities(mockNativeMethods.Object);

            // Act
            var result = consoleUtilities.TryEnableVirtualTerminalInput(out var actualError);

            // Assert
            Assert.That(result, Is.False);
            Assert.That(actualError, Is.EqualTo(expectedErrorMessage));

            mockNativeMethods.Verify();
        }

        [Test]
        public void TryEnableVirtualTerminalInput_GetConsoleModeFails_ReturnsFalseWithErrorMessage()
        {
            // Arrange
            uint errorCode = 42;
            var errorMessage = "Well that's not good.";
            var expectedErrorMessage = $"Error {errorCode}: {errorMessage}";
            var expectedHandle = new IntPtr(9001);
            uint expectedFormatFlags = NativeConstants.FORMAT_MESSAGE_ALLOCATE_BUFFER | NativeConstants.FORMAT_MESSAGE_FROM_SYSTEM | NativeConstants.FORMAT_MESSAGE_IGNORE_INSERTS;

            var mockNativeMethods = new Mock<INativeMethodsWrapper>(MockBehavior.Strict);
            mockNativeMethods
                .Setup(mock => mock.GetStdHandle(It.Is<int>(actualHandleDescriptor => actualHandleDescriptor == NativeConstants.STD_INPUT_HANDLE)))
                .Returns(expectedHandle)
                .Verifiable();

            mockNativeMethods
                .Setup(mock => mock.GetConsoleMode(
                    It.Is<IntPtr>(actualHandle => actualHandle == expectedHandle),
                    out It.Ref<uint>.IsAny))
                .Returns((IntPtr handle, ref uint currentConsoleMode) =>
                {
                    currentConsoleMode = 0;
                    return false;
                })
                .Verifiable();

            mockNativeMethods
                .Setup(mock => mock.GetLastError())
                .Returns(errorCode)
                .Verifiable();

            mockNativeMethods
                .Setup(mock => mock.FormatMessage(
                    It.Is<uint>(actualFlags => actualFlags == expectedFormatFlags),
                    It.Is<IntPtr>(lpSource => lpSource == IntPtr.Zero),
                    It.Is<uint>(actualErrorCode => actualErrorCode == errorCode),
                    It.Is<uint>(dwLanguageId => dwLanguageId == 0),
                    out It.Ref<string>.IsAny,
                    It.Is<uint>(nSize => nSize == 0),
                    It.Is<IntPtr>(arguments => arguments == IntPtr.Zero)))
                .Returns((uint dwFlags, IntPtr lpSource, uint dwMessageId, uint dwLanguageId, out string lpBuffer, uint nSize, IntPtr arguments) =>
                {
                    lpBuffer = errorMessage;
                    return (uint)errorMessage.Length;
                })
                .Verifiable();

            var consoleUtilities = new ConsoleUtilities(mockNativeMethods.Object);

            // Act
            var result = consoleUtilities.TryEnableVirtualTerminalInput(out var actualError);

            // Assert
            Assert.That(result, Is.False);
            Assert.That(actualError, Is.EqualTo(expectedErrorMessage));

            mockNativeMethods.Verify();
        }

        [Test]
        public void TryEnableVirtualTerminalInput_SetConsoleModeFails_ReturnsFalseWithErrorMessage()
        {
            // Arrange
            uint errorCode = 42;
            var errorMessage = "Well that's not good.";
            var expectedErrorMessage = $"Error {errorCode}: {errorMessage}";
            var expectedHandle = new IntPtr(9001);
            uint initialConsoleMode = 0;
            var expectedConsoleMode = initialConsoleMode | NativeConstants.ENABLE_VIRTUAL_TERMINAL_INPUT;
            uint expectedFormatFlags = NativeConstants.FORMAT_MESSAGE_ALLOCATE_BUFFER | NativeConstants.FORMAT_MESSAGE_FROM_SYSTEM | NativeConstants.FORMAT_MESSAGE_IGNORE_INSERTS;

            var mockNativeMethods = new Mock<INativeMethodsWrapper>(MockBehavior.Strict);
            mockNativeMethods
                .Setup(mock => mock.GetStdHandle(It.Is<int>(actualHandleDescriptor => actualHandleDescriptor == NativeConstants.STD_INPUT_HANDLE)))
                .Returns(expectedHandle)
                .Verifiable();

            mockNativeMethods
                .Setup(mock => mock.GetConsoleMode(
                    It.Is<IntPtr>(actualHandle => actualHandle == expectedHandle),
                    out It.Ref<uint>.IsAny))
                .Returns((IntPtr handle, ref uint currentConsoleMode) =>
                {
                    currentConsoleMode = initialConsoleMode;
                    return true;
                })
                .Verifiable();

            mockNativeMethods
                .Setup(mock => mock.SetConsoleMode(
                    It.Is<IntPtr>(actualHandle => actualHandle == expectedHandle),
                    It.Is<uint>(actualConsoleMode => actualConsoleMode == expectedConsoleMode)))
                .Returns(false)
                .Verifiable();

            mockNativeMethods
                .Setup(mock => mock.GetLastError())
                .Returns(errorCode)
                .Verifiable();

            mockNativeMethods
                .Setup(mock => mock.FormatMessage(
                    It.Is<uint>(actualFlags => actualFlags == expectedFormatFlags),
                    It.Is<IntPtr>(lpSource => lpSource == IntPtr.Zero),
                    It.Is<uint>(actualErrorCode => actualErrorCode == errorCode),
                    It.Is<uint>(dwLanguageId => dwLanguageId == 0),
                    out It.Ref<string>.IsAny,
                    It.Is<uint>(nSize => nSize == 0),
                    It.Is<IntPtr>(arguments => arguments == IntPtr.Zero)))
                .Returns((uint dwFlags, IntPtr lpSource, uint dwMessageId, uint dwLanguageId, out string lpBuffer, uint nSize, IntPtr arguments) =>
                {
                    lpBuffer = errorMessage;
                    return (uint)errorMessage.Length;
                })
                .Verifiable();

            var consoleUtilities = new ConsoleUtilities(mockNativeMethods.Object);

            // Act
            var result = consoleUtilities.TryEnableVirtualTerminalInput(out var actualError);

            // Assert
            Assert.That(result, Is.False);
            Assert.That(actualError, Is.EqualTo(expectedErrorMessage));

            mockNativeMethods.Verify();
        }
        #endregion TryEnableVirtualTerminalInput

        #region TryClearConsoleMode
        [Test]
        public void TryClearConsoleMode_Success_ReturnsTrue()
        {
            // Arrange
            var expectedHandle = new IntPtr(9001);
            var expectedInitialConsoleMode = NativeConstants.ENABLE_WRAP_AT_EOL_OUTPUT;
            var expectedConsoleMode = NativeConstants.CLEAR_CONSOLE_MODE;

            var mockNativeMethods = new Mock<INativeMethodsWrapper>(MockBehavior.Strict);
            mockNativeMethods
                .Setup(mock => mock.GetStdHandle(It.Is<int>(actualHandleDescriptor => actualHandleDescriptor == NativeConstants.STD_OUTPUT_HANDLE)))
                .Returns(expectedHandle)
                .Verifiable();

            mockNativeMethods
                .Setup(mock => mock.GetConsoleMode(
                    It.Is<IntPtr>(actualHandle => actualHandle == expectedHandle),
                    out It.Ref<uint>.IsAny))
                .Returns((IntPtr handle, ref uint currentConsoleMode) =>
                {
                    currentConsoleMode = expectedInitialConsoleMode;
                    return true;
                })
                .Verifiable();

            mockNativeMethods
                .Setup(mock => mock.SetConsoleMode(
                    It.Is<IntPtr>(actualHandle => actualHandle == expectedHandle),
                    It.Is<uint>(actualConsoleMode => actualConsoleMode == expectedConsoleMode)))
                .Returns(true)
                .Verifiable();

            var consoleUtilities = new ConsoleUtilities(mockNativeMethods.Object);

            // Act
            var result = consoleUtilities.TryClearConsoleMode(out var actualInitialConsoleMode, out var actualError);

            // Assert
            Assert.That(result, Is.True);
            Assert.That(actualInitialConsoleMode, Is.EqualTo(expectedInitialConsoleMode));
            Assert.That(actualError, Is.Null);

            mockNativeMethods.Verify();
        }

        [Test]
        public void TryClearConsoleMode_GetHandleFails_ReturnsFalseWithErrorMessage()
        {
            // Arrange
            uint errorCode = 42;
            var errorMessage = "Well that's not good.";
            var expectedErrorMessage = $"Error {errorCode}: {errorMessage}";
            uint expectedFormatFlags = NativeConstants.FORMAT_MESSAGE_ALLOCATE_BUFFER | NativeConstants.FORMAT_MESSAGE_FROM_SYSTEM | NativeConstants.FORMAT_MESSAGE_IGNORE_INSERTS;

            var mockNativeMethods = new Mock<INativeMethodsWrapper>(MockBehavior.Strict);
            mockNativeMethods
                .Setup(mock => mock.GetStdHandle(It.Is<int>(actualHandleDescriptor => actualHandleDescriptor == NativeConstants.STD_OUTPUT_HANDLE)))
                .Returns(NativeConstants.INVALID_HANDLE_VALUE)
                .Verifiable();

            mockNativeMethods
                .Setup(mock => mock.GetLastError())
                .Returns(errorCode)
                .Verifiable();

            mockNativeMethods
                .Setup(mock => mock.FormatMessage(
                    It.Is<uint>(actualFlags => actualFlags == expectedFormatFlags),
                    It.Is<IntPtr>(lpSource => lpSource == IntPtr.Zero),
                    It.Is<uint>(actualErrorCode => actualErrorCode == errorCode),
                    It.Is<uint>(dwLanguageId => dwLanguageId == 0),
                    out It.Ref<string>.IsAny,
                    It.Is<uint>(nSize => nSize == 0),
                    It.Is<IntPtr>(arguments => arguments == IntPtr.Zero)))
                .Returns((uint dwFlags, IntPtr lpSource, uint dwMessageId, uint dwLanguageId, out string lpBuffer, uint nSize, IntPtr arguments) =>
                {
                    lpBuffer = errorMessage;
                    return (uint)errorMessage.Length;
                })
                .Verifiable();

            var consoleUtilities = new ConsoleUtilities(mockNativeMethods.Object);

            // Act
            var result = consoleUtilities.TryClearConsoleMode(out var actualInitialConsoleMode, out var actualError);

            // Assert
            Assert.That(result, Is.False);
            Assert.That(actualInitialConsoleMode, Is.Null);
            Assert.That(actualError, Is.EqualTo(expectedErrorMessage));

            mockNativeMethods.Verify();
        }

        [Test]
        public void TryClearConsoleMode_GetConsoleModeFails_ReturnsFalseWithErrorMessage()
        {
            // Arrange
            uint errorCode = 42;
            var errorMessage = "Well that's not good.";
            var expectedErrorMessage = $"Error {errorCode}: {errorMessage}";
            var expectedHandle = new IntPtr(9001);
            uint expectedFormatFlags = NativeConstants.FORMAT_MESSAGE_ALLOCATE_BUFFER | NativeConstants.FORMAT_MESSAGE_FROM_SYSTEM | NativeConstants.FORMAT_MESSAGE_IGNORE_INSERTS;

            var mockNativeMethods = new Mock<INativeMethodsWrapper>(MockBehavior.Strict);
            mockNativeMethods
                .Setup(mock => mock.GetStdHandle(It.Is<int>(actualHandleDescriptor => actualHandleDescriptor == NativeConstants.STD_OUTPUT_HANDLE)))
                .Returns(expectedHandle)
                .Verifiable();

            mockNativeMethods
                .Setup(mock => mock.GetConsoleMode(
                    It.Is<IntPtr>(actualHandle => actualHandle == expectedHandle),
                    out It.Ref<uint>.IsAny))
                .Returns((IntPtr handle, ref uint currentConsoleMode) =>
                {
                    currentConsoleMode = 0;
                    return false;
                })
                .Verifiable();

            mockNativeMethods
                .Setup(mock => mock.GetLastError())
                .Returns(errorCode)
                .Verifiable();

            mockNativeMethods
                .Setup(mock => mock.FormatMessage(
                    It.Is<uint>(actualFlags => actualFlags == expectedFormatFlags),
                    It.Is<IntPtr>(lpSource => lpSource == IntPtr.Zero),
                    It.Is<uint>(actualErrorCode => actualErrorCode == errorCode),
                    It.Is<uint>(dwLanguageId => dwLanguageId == 0),
                    out It.Ref<string>.IsAny,
                    It.Is<uint>(nSize => nSize == 0),
                    It.Is<IntPtr>(arguments => arguments == IntPtr.Zero)))
                .Returns((uint dwFlags, IntPtr lpSource, uint dwMessageId, uint dwLanguageId, out string lpBuffer, uint nSize, IntPtr arguments) =>
                {
                    lpBuffer = errorMessage;
                    return (uint)errorMessage.Length;
                })
                .Verifiable();

            var consoleUtilities = new ConsoleUtilities(mockNativeMethods.Object);

            // Act
            var result = consoleUtilities.TryClearConsoleMode(out var actualInitialConsoleMode, out var actualError);

            // Assert
            Assert.That(result, Is.False);
            Assert.That(actualInitialConsoleMode, Is.Null);
            Assert.That(actualError, Is.EqualTo(expectedErrorMessage));

            mockNativeMethods.Verify();
        }

        [Test]
        public void TryClearConsoleMode_SetConsoleModeFails_ReturnsFalseWithErrorMessage()
        {
            // Arrange
            uint errorCode = 42;
            var errorMessage = "Well that's not good.";
            var expectedErrorMessage = $"Error {errorCode}: {errorMessage}";
            var expectedHandle = new IntPtr(9001);
            var initialConsoleMode = NativeConstants.ENABLE_WRAP_AT_EOL_OUTPUT;
            var expectedConsoleMode = NativeConstants.CLEAR_CONSOLE_MODE;
            uint expectedFormatFlags = NativeConstants.FORMAT_MESSAGE_ALLOCATE_BUFFER | NativeConstants.FORMAT_MESSAGE_FROM_SYSTEM | NativeConstants.FORMAT_MESSAGE_IGNORE_INSERTS;

            var mockNativeMethods = new Mock<INativeMethodsWrapper>(MockBehavior.Strict);
            mockNativeMethods
                .Setup(mock => mock.GetStdHandle(It.Is<int>(actualHandleDescriptor => actualHandleDescriptor == NativeConstants.STD_OUTPUT_HANDLE)))
                .Returns(expectedHandle)
                .Verifiable();

            mockNativeMethods
                .Setup(mock => mock.GetConsoleMode(
                    It.Is<IntPtr>(actualHandle => actualHandle == expectedHandle),
                    out It.Ref<uint>.IsAny))
                .Returns((IntPtr handle, ref uint currentConsoleMode) =>
                {
                    currentConsoleMode = initialConsoleMode;
                    return true;
                })
                .Verifiable();

            mockNativeMethods
                .Setup(mock => mock.SetConsoleMode(
                    It.Is<IntPtr>(actualHandle => actualHandle == expectedHandle),
                    It.Is<uint>(actualConsoleMode => actualConsoleMode == expectedConsoleMode)))
                .Returns(false)
                .Verifiable();

            mockNativeMethods
                .Setup(mock => mock.GetLastError())
                .Returns(errorCode)
                .Verifiable();

            mockNativeMethods
                .Setup(mock => mock.FormatMessage(
                    It.Is<uint>(actualFlags => actualFlags == expectedFormatFlags),
                    It.Is<IntPtr>(lpSource => lpSource == IntPtr.Zero),
                    It.Is<uint>(actualErrorCode => actualErrorCode == errorCode),
                    It.Is<uint>(dwLanguageId => dwLanguageId == 0),
                    out It.Ref<string>.IsAny,
                    It.Is<uint>(nSize => nSize == 0),
                    It.Is<IntPtr>(arguments => arguments == IntPtr.Zero)))
                .Returns((uint dwFlags, IntPtr lpSource, uint dwMessageId, uint dwLanguageId, out string lpBuffer, uint nSize, IntPtr arguments) =>
                {
                    lpBuffer = errorMessage;
                    return (uint)errorMessage.Length;
                })
                .Verifiable();

            var consoleUtilities = new ConsoleUtilities(mockNativeMethods.Object);

            // Act
            var result = consoleUtilities.TryClearConsoleMode(out var actualInitialConsoleMode, out var actualError);

            // Assert
            Assert.That(result, Is.False);
            Assert.That(actualInitialConsoleMode, Is.Null);
            Assert.That(actualError, Is.EqualTo(expectedErrorMessage));

            mockNativeMethods.Verify();
        }
        #endregion TryClearConsoleMode
    }
}
