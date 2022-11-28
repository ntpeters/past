using past.ConsoleApp.Commands;
using past.ConsoleApp.Output;
using past.ConsoleApp.Test.TestHelpers;
using System.CommandLine;
using System.CommandLine.Invocation;

namespace past.ConsoleApp.Test.Commands
{
    public class StatusCommandTests
    {
        #region Constructor
        [Test]
        public void Constructor_ValidParameters_Success()
        {
            // Arrange
            var quietOption = new Option<bool>("--quiet");

            var expectedHandler = (IConsoleWriter _, InvocationContext _) => { };

            // Act + Assert
            Assert.DoesNotThrow(() => new StatusCommand(
                quietOption,
                expectedHandler));
        }

        [Test]
        public void Constructor_NullQuietOption_ThrowsArgumentNullException()
        {
            // Arrange
            var expectedHandler = (IConsoleWriter _, InvocationContext _) => { };

            // Act + Assert
            Assert.Throws<ArgumentNullException>(() => new StatusCommand(
                null!,
                expectedHandler));
        }

        [Test]
        public void Constructor_NullHandler_ThrowsArgumentNullException()
        {
            // Arrange
            var quietOption = new Option<bool>("--quiet");

            // Act + Assert
            Assert.Throws<ArgumentNullException>(() => new StatusCommand(
                quietOption,
                null!));
        }
        #endregion Constructor

        #region Command
        [Test]
        public void StatusCommand_HasExpectedNameAndDescription()
        {
            // Arrange
            var expectedCommandName = "status";
            var expctedCommandDescription = "Gets the status of the clipboard history settings on this device";

            var quietOption = new Option<bool>("--quiet");

            var expectedHandler = (IConsoleWriter _, InvocationContext _) => { };

            var statusCommand = new StatusCommand(
                quietOption,
                expectedHandler);

            // Act + Assert
            Assert.That(statusCommand.Name, Is.EqualTo(expectedCommandName));
            Assert.That(statusCommand.Description, Is.EqualTo(expctedCommandDescription));
        }

        [Test]
        public void StatusCommand_HasExpectedHandler()
        {
            // Arrange
            var quietOption = new Option<bool>("--quiet");

            var expectedHandler = (IConsoleWriter _, InvocationContext _) => { };

            var statusCommand = new StatusCommand(
                quietOption,
                expectedHandler);

            // Act + Assert
            var actualHandler = statusCommand.GetHandler();
            Assert.That(actualHandler, Is.EqualTo(expectedHandler));
        }

        [Test]
        public void StatusCommand_HasNoSubcommands()
        {
            // Arrange
            var quietOption = new Option<bool>("--quiet");

            var expectedHandler = (IConsoleWriter _, InvocationContext _) => { };

            var statusCommand = new StatusCommand(
                quietOption,
                expectedHandler);

            // Act + Assert
            Assert.That(statusCommand.Subcommands, Is.Empty);
        }

        [Test]
        public void StatusCommand_HasNoArguments()
        {
            // Arrange
            var quietOption = new Option<bool>("--quiet");

            var expectedHandler = (IConsoleWriter _, InvocationContext _) => { };

            var statusCommand = new StatusCommand(
                quietOption,
                expectedHandler);

            // Act + Assert
            Assert.That(statusCommand.Arguments, Is.Empty);
        }

        [Test]
        public void StatusCommand_HasNoOptions()
        {
            // Arrange
            var quietOption = new Option<bool>("--quiet");

            var expectedHandler = (IConsoleWriter _, InvocationContext _) => { };

            var statusCommand = new StatusCommand(
                quietOption,
                expectedHandler);

            // Act + Assert
            Assert.That(statusCommand.Options, Is.Empty);
        }
        #endregion Command
    }
}
