using Moq;
using past.ConsoleApp.Middleware;
using past.ConsoleApp.Wrappers;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;

namespace past.ConsoleApp.Test.Middleware
{
    public class ConsoleModeMiddlewareTests
    {
        #region Constructors
        [Test]
        public void Create_Parameterless_Success()
        {
            Assert.DoesNotThrow(() => new ConsoleModeMiddleware());
        }

        [Test]
        public void Create_ValidParameters_Success()
        {
            // Arrange
            var mockConsoleUtilities = new Mock<IConsoleUtilities>(MockBehavior.Strict);
            var mockAppDomain = new Mock<IAppDomainWrapper>(MockBehavior.Strict);

            // Act + Assert
            Assert.DoesNotThrow(() => new ConsoleModeMiddleware(mockConsoleUtilities.Object, mockAppDomain.Object));
        }

        [Test]
        public void Create_NullConsoleUtilities_ThrowsArgumentNullException()
        {
            // Arrange
            var mockAppDomain = new Mock<IAppDomainWrapper>(MockBehavior.Strict);

            // Act + Assert
            Assert.Throws<ArgumentNullException>(() => new ConsoleModeMiddleware(null!, mockAppDomain.Object));
        }

        [Test]
        public void Create_NullAppDomain_ThrowsArgumentNullException()
        {
            // Arrange
            var mockConsoleUtilities = new Mock<IConsoleUtilities>(MockBehavior.Strict);

            // Act + Assert
            Assert.Throws<ArgumentNullException>(() => new ConsoleModeMiddleware(mockConsoleUtilities.Object, null!));
        }
        #endregion Constructors

        #region ConfigureConsoleMode
        [Test]
        public void ConfigureConsoleMode_EnablesVtProcessing_RestoresConsoleModeOnProcessExit()
        {
            // Arrange
            var mockUtilities = new Mock<IConsoleUtilities>(MockBehavior.Strict);
            mockUtilities
                .Setup(mock => mock.TryEnableVirtualTerminalProcessing(out It.Ref<string?>.IsAny))
                .Returns((out string? error) =>
                {
                    error = null;
                    return true;
                })
                .Verifiable();

            mockUtilities
                .Setup(mock => mock.TryRestoreConsoleMode(out It.Ref<string?>.IsAny))
                .Returns((out string? error) =>
                {
                    error = null;
                    return true;
                })
                .Verifiable();

            var mockAppDomain = new Mock<IAppDomainWrapper>(MockBehavior.Strict);

            var mockConsole = new Mock<IConsole>(MockBehavior.Strict);
            mockConsole
                .SetupGet(mock => mock.IsOutputRedirected)
                .Returns(false);

            var testCommand = new Command("test");
            testCommand.SetHandler((InvocationContext context) =>
            {
                var middleware = new ConsoleModeMiddleware(mockUtilities.Object, mockAppDomain.Object);
                middleware.ConfigureConsoleMode(context);
            });

            // Act
            testCommand.Invoke("test", mockConsole.Object);

            mockAppDomain.Raise(mock => mock.ProcessExit += null, mockAppDomain.Object, EventArgs.Empty);

            // Assert
            mockUtilities.Verify();
            mockConsole.Verify();

            mockAppDomain.VerifyAdd(
                mock => mock.ProcessExit += It.IsAny<EventHandler>(),
                Times.Once);
            mockAppDomain.VerifyRemove(
                mock => mock.ProcessExit -= It.IsAny<EventHandler>(),
                Times.Never);
        }

        [Test]
        public void ConfigureConsoleMode_RestoreConsoleModeFails_WritesError()
        {
            // Arrange
            string nativeError = "💥💥💥";
            string expectedError = $"Failed to restore original console mode: {nativeError}{Environment.NewLine}";

            var mockUtilities = new Mock<IConsoleUtilities>(MockBehavior.Strict);
            mockUtilities
                .Setup(mock => mock.TryEnableVirtualTerminalProcessing(out It.Ref<string?>.IsAny))
                .Returns((out string? error) =>
                {
                    error = null;
                    return true;
                })
                .Verifiable();

            mockUtilities
                .Setup(mock => mock.TryRestoreConsoleMode(out It.Ref<string?>.IsAny))
                .Returns((out string? error) =>
                {
                    error = nativeError;
                    return false;
                })
                .Verifiable();

            var mockAppDomain = new Mock<IAppDomainWrapper>(MockBehavior.Strict);
            var mockStreamWriter = new Mock<IStandardStreamWriter>(MockBehavior.Strict);

            string? actualWrittenValue = null;
            mockStreamWriter
                .Setup(mock => mock.Write(It.IsAny<string?>()))
                .Callback<string?>(value => actualWrittenValue = value)
                .Verifiable();

            var mockConsole = new Mock<IConsole>(MockBehavior.Strict);
            mockConsole
                .SetupGet(mock => mock.IsOutputRedirected)
                .Returns(false);

            mockConsole
                .SetupGet(mock => mock.Error)
                .Returns(mockStreamWriter.Object)
                .Verifiable();

            var testCommand = new Command("test");
            testCommand.SetHandler((InvocationContext context) =>
            {
                var middleware = new ConsoleModeMiddleware(mockUtilities.Object, mockAppDomain.Object);
                middleware.ConfigureConsoleMode(context);
            });

            // Act
            testCommand.Invoke("test", mockConsole.Object);

            mockAppDomain.Raise(mock => mock.ProcessExit += null, mockAppDomain.Object, EventArgs.Empty);

            // Assert
            Assert.That(actualWrittenValue, Is.EqualTo(expectedError));

            mockUtilities.Verify();
            mockConsole.Verify();
            mockStreamWriter.Verify();

            mockAppDomain.VerifyAdd(
                mock => mock.ProcessExit += It.IsAny<EventHandler>(),
                Times.Once);
            mockAppDomain.VerifyRemove(
                mock => mock.ProcessExit -= It.IsAny<EventHandler>(),
                Times.Never);
        }

        [Test]
        public void ConfigureConsoleMode_EnableVtProcessingFails_WritesError()
        {
            // Arrange
            string nativeError = "💥💥💥";
            string expectedError = $"Failed to enable VT processing: {nativeError}{Environment.NewLine}";

            var mockUtilities = new Mock<IConsoleUtilities>(MockBehavior.Strict);
            mockUtilities
                .Setup(mock => mock.TryEnableVirtualTerminalProcessing(out It.Ref<string?>.IsAny))
                .Returns((out string? error) =>
                {
                    error = nativeError;
                    return false;
                })
                .Verifiable();

            var mockAppDomain = new Mock<IAppDomainWrapper>(MockBehavior.Strict);
            var mockStreamWriter = new Mock<IStandardStreamWriter>(MockBehavior.Strict);

            string? actualWrittenValue = null;
            mockStreamWriter
                .Setup(mock => mock.Write(It.IsAny<string?>()))
                .Callback<string?>(value => actualWrittenValue = value)
                .Verifiable();

            var mockConsole = new Mock<IConsole>(MockBehavior.Strict);
            mockConsole
                .SetupGet(mock => mock.IsOutputRedirected)
                .Returns(false);

            mockConsole
                .SetupGet(mock => mock.Error)
                .Returns(mockStreamWriter.Object)
                .Verifiable();

            var testCommand = new Command("test");
            testCommand.SetHandler((InvocationContext context) =>
            {
                var middleware = new ConsoleModeMiddleware(mockUtilities.Object, mockAppDomain.Object);
                middleware.ConfigureConsoleMode(context);
            });

            // Act
            testCommand.Invoke("test", mockConsole.Object);

            mockAppDomain.Raise(mock => mock.ProcessExit += null, mockAppDomain.Object, EventArgs.Empty);

            // Assert
            Assert.That(actualWrittenValue, Is.EqualTo(expectedError));

            mockUtilities.Verify();
            mockConsole.Verify();
            mockStreamWriter.Verify();

            mockAppDomain.VerifyAdd(
                mock => mock.ProcessExit += It.IsAny<EventHandler>(),
                Times.Never);
            mockAppDomain.VerifyRemove(
                mock => mock.ProcessExit -= It.IsAny<EventHandler>(),
                Times.Never);
        }

        [Test]
        public void ConfigureConsoleMode_OutputRedirected_Noop()
        {
            // Arrange
            var mockUtilities = new Mock<IConsoleUtilities>(MockBehavior.Strict);
            var mockAppDomain = new Mock<IAppDomainWrapper>(MockBehavior.Strict);

            var mockConsole = new Mock<IConsole>(MockBehavior.Strict);
            mockConsole
                .SetupGet(mock => mock.IsOutputRedirected)
                .Returns(true);

            var testCommand = new Command("test");
            testCommand.SetHandler((InvocationContext context) =>
            {
                var middleware = new ConsoleModeMiddleware(mockUtilities.Object, mockAppDomain.Object);
                middleware.ConfigureConsoleMode(context);
            });

            // Act
            testCommand.Invoke("test", mockConsole.Object);

            mockAppDomain.Raise(mock => mock.ProcessExit += null, mockAppDomain.Object, EventArgs.Empty);

            // Assert
            mockUtilities.Verify();
            mockConsole.Verify();

            mockAppDomain.VerifyAdd(
                mock => mock.ProcessExit += It.IsAny<EventHandler>(),
                Times.Never);
            mockAppDomain.VerifyRemove(
                mock => mock.ProcessExit -= It.IsAny<EventHandler>(),
                Times.Never);
        }
        #endregion ConfigureConsoleMode
    }
}
