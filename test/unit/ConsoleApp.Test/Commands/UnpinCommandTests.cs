using past.ConsoleApp.Commands;
using past.ConsoleApp.Output;
using past.ConsoleApp.Test.TestHelpers;
using past.Core.Models;
using System.CommandLine;

namespace past.ConsoleApp.Test.Commands
{
    public class UnpinCommandTests
    {
        #region Constructor
        [Test]
        public void Constructor_ValidParameters_Success()
        {
            // Arrange
            var identifierArgument = new Argument<string>();
            var quietOption = new Option<bool>("--quiet");

            var expectedHandler = (IConsoleWriter _, ClipboardItemIdentifier _, CancellationToken _) => Task.FromResult(0);

            // Act + Assert
            Assert.DoesNotThrow(() => new UnpinCommand(
                identifierArgument,
                quietOption,
                expectedHandler));
        }

        [Test]
        public void Constructor_NullIdentifierArgument_ThrowsArgumentNullException()
        {
            // Arrange
            var quietOption = new Option<bool>("--quiet");

            var expectedHandler = (IConsoleWriter _, ClipboardItemIdentifier _, CancellationToken _) => Task.FromResult(0);

            // Act + Assert
            Assert.Throws<ArgumentNullException>(() => new UnpinCommand(
                null!,
                quietOption,
                expectedHandler));
        }

        [Test]
        public void Constructor_NullQuietOption_ThrowsArgumentNullException()
        {
            // Arrange
            var identifierArgument = new Argument<string>();

            var expectedHandler = (IConsoleWriter _, ClipboardItemIdentifier _, CancellationToken _) => Task.FromResult(0);

            // Act + Assert
            Assert.Throws<ArgumentNullException>(() => new UnpinCommand(
                identifierArgument,
                null!,
                expectedHandler));
        }

        [Test]
        public void Constructor_NullHandler_ThrowsArgumentNullException()
        {
            // Arrange
            var identifierArgument = new Argument<string>();
            var quietOption = new Option<bool>("--quiet");

            // Act + Assert
            Assert.Throws<ArgumentNullException>(() => new UnpinCommand(
                identifierArgument,
                quietOption,
                null!));
        }
        #endregion Constructor

        #region Command
        [Test]
        public void UnpinCommand_HasExpectedNameAndDescription()
        {
            // Arrange
            var expectedCommandName = "unpin";
            var expctedCommandDescription = "[Experimental] Unpins the specified pinned item from clipboard history";

            var identifierArgument = new Argument<string>();
            var quietOption = new Option<bool>("--quiet");

            var expectedHandler = (IConsoleWriter _, ClipboardItemIdentifier _, CancellationToken _) => Task.FromResult(0);

            var unpinCommand = new UnpinCommand(
                identifierArgument,
                quietOption,
                expectedHandler);

            // Act + Assert
            Assert.That(unpinCommand.Name, Is.EqualTo(expectedCommandName));
            Assert.That(unpinCommand.Description, Is.EqualTo(expctedCommandDescription));
        }

        [Test]
        public void UnpinCommand_HasExpectedHandler()
        {
            // Arrange
            var identifierArgument = new Argument<string>();
            var quietOption = new Option<bool>("--quiet");

            var expectedHandler = (IConsoleWriter _, ClipboardItemIdentifier _, CancellationToken _) => Task.FromResult(0);

            var unpinCommand = new UnpinCommand(
                identifierArgument,
                quietOption,
                expectedHandler);

            // Act + Assert
            var actualHandler = unpinCommand.GetHandler();
            Assert.That(actualHandler, Is.EqualTo(expectedHandler));
        }

        [Test]
        public void UnpinCommand_HasNoSubcommands()
        {
            // Arrange
            var identifierArgument = new Argument<string>();
            var quietOption = new Option<bool>("--quiet");

            var expectedHandler = (IConsoleWriter _, ClipboardItemIdentifier _, CancellationToken _) => Task.FromResult(0);

            var unpinCommand = new UnpinCommand(
                identifierArgument,
                quietOption,
                expectedHandler);

            // Act + Assert
            Assert.That(unpinCommand.Subcommands, Is.Empty);
        }
        #endregion Command

        #region Arguments
        [Test]
        public void Arguments_HasExpectedCount()
        {
            // Arrange
            var identifierArgument = new Argument<string>();
            var quietOption = new Option<bool>("--quiet");

            var expectedHandler = (IConsoleWriter _, ClipboardItemIdentifier _, CancellationToken _) => Task.FromResult(0);

            var unpinCommand = new UnpinCommand(
                identifierArgument,
                quietOption,
                expectedHandler);

            // Act + Assert
            Assert.That(unpinCommand.Arguments, Has.Exactly(1).Items);
        }

        [Test]
        public void Arguments_HasIdentifierArgument()
        {
            // Arrange
            var identifierArgument = new Argument<string>();
            var quietOption = new Option<bool>("--quiet");

            var expectedHandler = (IConsoleWriter _, ClipboardItemIdentifier _, CancellationToken _) => Task.FromResult(0);

            var unpinCommand = new UnpinCommand(
                identifierArgument,
                quietOption,
                expectedHandler);

            // Act + Assert
            Assert.That(unpinCommand.Arguments, Contains.Item(identifierArgument));
        }
        #endregion Arguments

        #region Options
        [Test]
        public void Options_HasExpectedCount()
        {
            // Arrange
            var identifierArgument = new Argument<string>();
            var quietOption = new Option<bool>("--quiet");

            var expectedHandler = (IConsoleWriter _, ClipboardItemIdentifier _, CancellationToken _) => Task.FromResult(0);

            var unpinCommand = new UnpinCommand(
                identifierArgument,
                quietOption,
                expectedHandler);

            // Act + Assert
            Assert.That(unpinCommand.Options, Is.Empty);
        }

        [Test]
        public void Options_HasAllExpectedProvidedOptions()
        {
            // Arrange
            var identifierArgument = new Argument<string>();
            var quietOption = new Option<bool>("--quiet");

            var expectedHandler = (IConsoleWriter _, ClipboardItemIdentifier _, CancellationToken _) => Task.FromResult(0);

            var unpinCommand = new UnpinCommand(
                identifierArgument,
                quietOption,
                expectedHandler);

            // Act + Assert
            Assert.That(unpinCommand.Options, Does.Not.Contain(quietOption));
        }
        #endregion Options
    }
}
