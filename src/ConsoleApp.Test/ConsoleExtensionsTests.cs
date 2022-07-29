using Moq;
using past.ConsoleApp.Extensions;
using System.CommandLine;
using System.CommandLine.IO;

namespace past.ConsoleApp.Test
{
    public class ConsoleExtensionsTests
    {
        #region WriteError
        [Test]
        public void WriteError_SuppressOutputDefaultValue_ReturnsTrueAndWritesValueToErrorStream()
        {
            // Arrange
            var expectedValue = "Hello there!";

            string? actualValue = null;
            var mockErrorStream = new Mock<IStandardStreamWriter>(MockBehavior.Strict);
            mockErrorStream
                .Setup(mock => mock.Write(It.IsAny<string>()))
                .Callback<string>(value => actualValue = value);

            var mockConsole = new Mock<IConsole>(MockBehavior.Strict);
            mockConsole
                .SetupGet(mock => mock.Error)
                .Returns(mockErrorStream.Object);

            // Act
            var result = mockConsole.Object.WriteError(expectedValue);

            // Assert
            Assert.That(result , Is.True);
            Assert.That(actualValue, Is.EqualTo(expectedValue));
        }

        [Test]
        public void WriteError_SuppressOutputFalse_ReturnsTrueAndWritesValueToErrorStream()
        {
            // Arrange
            var expectedValue = "Hello there!";

            string? actualValue = null;
            var mockErrorStream = new Mock<IStandardStreamWriter>(MockBehavior.Strict);
            mockErrorStream
                .Setup(mock => mock.Write(It.IsAny<string>()))
                .Callback<string>(value => actualValue = value);

            var mockConsole = new Mock<IConsole>(MockBehavior.Strict);
            mockConsole
                .SetupGet(mock => mock.Error)
                .Returns(mockErrorStream.Object);

            // Act
            var result = mockConsole.Object.WriteError(expectedValue, false);

            // Assert
            Assert.That(result, Is.True);
            Assert.That(actualValue, Is.EqualTo(expectedValue));
        }

        [Test]
        public void WriteError_SuppressOutputTrue_ReturnsFalseAndWritesNothingErrorStream()
        {
            // Arrange
            var inputValue = "Hello there!";
            var mockConsole = new Mock<IConsole>(MockBehavior.Strict);

            // Act
            var result = mockConsole.Object.WriteError(inputValue, true);

            // Assert
            Assert.That(result, Is.False);
        }
        #endregion WriteError

        #region WriteErrorLine
        [Test]
        public void WriteErrorLine_SuppressOutputDefaultValue_ReturnsTrueAndWritesValueToErrorStream()
        {
            // Arrange
            var inputValue = "Hello there!";
            var expectedValue = $"{inputValue}{Environment.NewLine}";

            string? actualValue = null;
            var mockErrorStream = new Mock<IStandardStreamWriter>(MockBehavior.Strict);
            mockErrorStream
                .Setup(mock => mock.Write(It.IsAny<string>()))
                .Callback<string>(value => actualValue = value);

            var mockConsole = new Mock<IConsole>(MockBehavior.Strict);
            mockConsole
                .SetupGet(mock => mock.Error)
                .Returns(mockErrorStream.Object);

            // Act
            var result = mockConsole.Object.WriteErrorLine(inputValue);

            // Assert
            Assert.That(result, Is.True);
            Assert.That(actualValue, Is.EqualTo(expectedValue));
        }

        [Test]
        public void WriteErrorLine_SuppressOutputFalse_ReturnsTrueAndWritesValueToErrorStream()
        {
            // Arrange
            var inputValue = "Hello there!";
            var expectedValue = $"{inputValue}{Environment.NewLine}";

            string? actualValue = null;
            var mockErrorStream = new Mock<IStandardStreamWriter>(MockBehavior.Strict);
            mockErrorStream
                .Setup(mock => mock.Write(It.IsAny<string>()))
                .Callback<string>(value => actualValue = value);

            var mockConsole = new Mock<IConsole>(MockBehavior.Strict);
            mockConsole
                .SetupGet(mock => mock.Error)
                .Returns(mockErrorStream.Object);

            // Act
            var result = mockConsole.Object.WriteErrorLine(inputValue, false);

            // Assert
            Assert.That(result, Is.True);
            Assert.That(actualValue, Is.EqualTo(expectedValue));
        }

        [Test]
        public void WriteErrorLine_SuppressOutputTrue_ReturnsFalseAndWritesNothingErrorStream()
        {
            // Arrange
            var inputValue = "Hello there!";
            var mockConsole = new Mock<IConsole>(MockBehavior.Strict);

            // Act
            var result = mockConsole.Object.WriteErrorLine(inputValue, true);

            // Assert
            Assert.That(result, Is.False);
        }
        #endregion WriteErrorLine
    }
}
