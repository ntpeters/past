using past.ConsoleApp.Commands;
using past.ConsoleApp.Test.TestHelpers;
using past.Core;
using System.CommandLine;

namespace past.ConsoleApp.Test.Commands
{
    public class GetCommandTests
    {
        #region Constructor
        [Test]
        public void Constructor_ValidParameters_Success()
        {
            // Arrange
            var identifierArgument = new Argument<string>();

            var typeOption = new Option<ContentType>("--type");
            var allOption = new Option<bool>("--all");
            var ansiOption = new Option<bool>("--ansi");
            var ansiResetOption = new Option<AnsiResetType>("--ansi-reset");
            var quietOption = new Option<bool>("--quiet");

            var expectedHandler = (IConsoleWriter _, IValueFormatter _, ClipboardItemIdentifier _, ContentType _, bool _, CancellationToken _) => Task.FromResult(0);

            // Act + Assert
            Assert.DoesNotThrow(() => new GetCommand(
                identifierArgument,
                typeOption,
                allOption,
                ansiOption,
                ansiResetOption,
                quietOption,
                expectedHandler));
        }

        [Test]
        public void Constructor_NullIdentifierArgument_ThrowsArgumentNullException()
        {
            // Arrange
            var typeOption = new Option<ContentType>("--type");
            var allOption = new Option<bool>("--all");
            var ansiOption = new Option<bool>("--ansi");
            var ansiResetOption = new Option<AnsiResetType>("--ansi-reset");
            var quietOption = new Option<bool>("--quiet");

            var expectedHandler = (IConsoleWriter _, IValueFormatter _, ClipboardItemIdentifier _, ContentType _, bool _, CancellationToken _) => Task.FromResult(0);

            // Act + Assert
            Assert.Throws<ArgumentNullException>(() => new GetCommand(
                null!,
                typeOption,
                allOption,
                ansiOption,
                ansiResetOption,
                quietOption,
                expectedHandler));
        }

        [Test]
        public void Constructor_NullTypeOption_ThrowsArgumentNullException()
        {
            // Arrange
            var identifierArgument = new Argument<string>();

            var allOption = new Option<bool>("--all");
            var ansiOption = new Option<bool>("--ansi");
            var ansiResetOption = new Option<AnsiResetType>("--ansi-reset");
            var quietOption = new Option<bool>("--quiet");

            var expectedHandler = (IConsoleWriter _, IValueFormatter _, ClipboardItemIdentifier _, ContentType _, bool _, CancellationToken _) => Task.FromResult(0);

            // Act + Assert
            Assert.Throws<ArgumentNullException>(() => new GetCommand(
                identifierArgument,
                null!,
                allOption,
                ansiOption,
                ansiResetOption,
                quietOption,
                expectedHandler));
        }

        [Test]
        public void Constructor_NullAllOption_ThrowsArgumentNullException()
        {
            // Arrange
            var identifierArgument = new Argument<string>();

            var typeOption = new Option<ContentType>("--type");
            var ansiOption = new Option<bool>("--ansi");
            var ansiResetOption = new Option<AnsiResetType>("--ansi-reset");
            var quietOption = new Option<bool>("--quiet");

            var expectedHandler = (IConsoleWriter _, IValueFormatter _, ClipboardItemIdentifier _, ContentType _, bool _, CancellationToken _) => Task.FromResult(0);

            // Act + Assert
            Assert.Throws<ArgumentNullException>(() => new GetCommand(
                identifierArgument,
                typeOption,
                null!,
                ansiOption,
                ansiResetOption,
                quietOption,
                expectedHandler));
        }

        [Test]
        public void Constructor_NullAnsiOption_ThrowsArgumentNullException()
        {
            // Arrange
            var identifierArgument = new Argument<string>();

            var typeOption = new Option<ContentType>("--type");
            var allOption = new Option<bool>("--all");
            var ansiResetOption = new Option<AnsiResetType>("--ansi-reset");
            var quietOption = new Option<bool>("--quiet");

            var expectedHandler = (IConsoleWriter _, IValueFormatter _, ClipboardItemIdentifier _, ContentType _, bool _, CancellationToken _) => Task.FromResult(0);

            // Act + Assert
            Assert.Throws<ArgumentNullException>(() => new GetCommand(
                identifierArgument,
                typeOption,
                allOption,
                null!,
                ansiResetOption,
                quietOption,
                expectedHandler));
        }

        [Test]
        public void Constructor_NullAnsiResetOption_ThrowsArgumentNullException()
        {
            // Arrange
            var identifierArgument = new Argument<string>();

            var typeOption = new Option<ContentType>("--type");
            var allOption = new Option<bool>("--all");
            var ansiOption = new Option<bool>("--ansi");
            var quietOption = new Option<bool>("--quiet");

            var expectedHandler = (IConsoleWriter _, IValueFormatter _, ClipboardItemIdentifier _, ContentType _, bool _, CancellationToken _) => Task.FromResult(0);

            // Act + Assert
            Assert.Throws<ArgumentNullException>(() => new GetCommand(
                identifierArgument,
                typeOption,
                allOption,
                ansiOption,
                null!,
                quietOption,
                expectedHandler));
        }

        [Test]
        public void Constructor_NullQuietOption_ThrowsArgumentNullException()
        {
            // Arrange
            var identifierArgument = new Argument<string>();

            var typeOption = new Option<ContentType>("--type");
            var allOption = new Option<bool>("--all");
            var ansiOption = new Option<bool>("--ansi");
            var ansiResetOption = new Option<AnsiResetType>("--ansi-reset");

            var expectedHandler = (IConsoleWriter _, IValueFormatter _, ClipboardItemIdentifier _, ContentType _, bool _, CancellationToken _) => Task.FromResult(0);

            // Act + Assert
            Assert.Throws<ArgumentNullException>(() => new GetCommand(
                identifierArgument,
                typeOption,
                allOption,
                ansiOption,
                ansiResetOption,
                null!,
                expectedHandler));
        }

        [Test]
        public void Constructor_NullHandler_ThrowsArgumentNullException()
        {
            // Arrange
            var identifierArgument = new Argument<string>();

            var typeOption = new Option<ContentType>("--type");
            var allOption = new Option<bool>("--all");
            var ansiOption = new Option<bool>("--ansi");
            var ansiResetOption = new Option<AnsiResetType>("--ansi-reset");
            var quietOption = new Option<bool>("--quiet");

            // Act + Assert
            Assert.Throws<ArgumentNullException>(() => new GetCommand(
                identifierArgument,
                typeOption,
                allOption,
                ansiOption,
                ansiResetOption,
                quietOption,
                null!));
        }
        #endregion Constructor

        #region Command
        [Test]
        public void GetCommand_HasExpectedNameAndDescription()
        {
            // Arrange
            var expectedCommandName = "get";
            var expctedCommandDescription = "Gets the item at the specified index from clipboard history";

            var identifierArgument = new Argument<string>();

            var typeOption = new Option<ContentType>("--type");
            var allOption = new Option<bool>("--all");
            var ansiOption = new Option<bool>("--ansi");
            var ansiResetOption = new Option<AnsiResetType>("--ansi-reset");
            var quietOption = new Option<bool>("--quiet");

            var expectedHandler = (IConsoleWriter _, IValueFormatter _, ClipboardItemIdentifier _, ContentType _, bool _, CancellationToken _) => Task.FromResult(0);

            var getCommand = new GetCommand(
                identifierArgument,
                typeOption,
                allOption,
                ansiOption,
                ansiResetOption,
                quietOption,
                expectedHandler);

            // Act + Assert
            Assert.That(getCommand.Name, Is.EqualTo(expectedCommandName));
            Assert.That(getCommand.Description, Is.EqualTo(expctedCommandDescription));
        }

        [Test]
        public void GetCommand_HasExpectedHandler()
        {
            // Arrange
            var identifierArgument = new Argument<string>();

            var typeOption = new Option<ContentType>("--type");
            var allOption = new Option<bool>("--all");
            var ansiOption = new Option<bool>("--ansi");
            var ansiResetOption = new Option<AnsiResetType>("--ansi-reset");
            var quietOption = new Option<bool>("--quiet");

            var expectedHandler = (IConsoleWriter _, IValueFormatter _, ClipboardItemIdentifier _, ContentType _, bool _, CancellationToken _) => Task.FromResult(0);

            var getCommand = new GetCommand(
                identifierArgument,
                typeOption,
                allOption,
                ansiOption,
                ansiResetOption,
                quietOption,
                expectedHandler);

            // Act + Assert
            var actualHandler = getCommand.GetHandler();
            Assert.That(actualHandler, Is.EqualTo(expectedHandler));
        }

        [Test]
        public void GetCommand_HasNoSubcommands()
        {
            // Arrange
            var identifierArgument = new Argument<string>();

            var typeOption = new Option<ContentType>("--type");
            var allOption = new Option<bool>("--all");
            var ansiOption = new Option<bool>("--ansi");
            var ansiResetOption = new Option<AnsiResetType>("--ansi-reset");
            var quietOption = new Option<bool>("--quiet");

            var expectedHandler = (IConsoleWriter _, IValueFormatter _, ClipboardItemIdentifier _, ContentType _, bool _, CancellationToken _) => Task.FromResult(0);

            var getCommand = new GetCommand(
                identifierArgument,
                typeOption,
                allOption,
                ansiOption,
                ansiResetOption,
                quietOption,
                expectedHandler);

            // Act + Assert
            Assert.That(getCommand.Subcommands, Is.Empty);
        }
        #endregion Command

        #region Arguments
        [Test]
        public void Arguments_HasExpectedCount()
        {
            // Arrange
            var identifierArgument = new Argument<string>();

            var typeOption = new Option<ContentType>("--type");
            var allOption = new Option<bool>("--all");
            var ansiOption = new Option<bool>("--ansi");
            var ansiResetOption = new Option<AnsiResetType>("--ansi-reset");
            var quietOption = new Option<bool>("--quiet");

            var expectedHandler = (IConsoleWriter _, IValueFormatter _, ClipboardItemIdentifier _, ContentType _, bool _, CancellationToken _) => Task.FromResult(0);

            var getCommand = new GetCommand(
                identifierArgument,
                typeOption,
                allOption,
                ansiOption,
                ansiResetOption,
                quietOption,
                expectedHandler);

            // Act + Assert
            Assert.That(getCommand.Arguments, Has.Exactly(1).Items);
        }

        [Test]
        public void Arguments_HasIdentifierArgument()
        {
            // Arrange
            var identifierArgument = new Argument<string>();

            var typeOption = new Option<ContentType>("--type");
            var allOption = new Option<bool>("--all");
            var ansiOption = new Option<bool>("--ansi");
            var ansiResetOption = new Option<AnsiResetType>("--ansi-reset");
            var quietOption = new Option<bool>("--quiet");

            var expectedHandler = (IConsoleWriter _, IValueFormatter _, ClipboardItemIdentifier _, ContentType _, bool _, CancellationToken _) => Task.FromResult(0);

            var getCommand = new GetCommand(
                identifierArgument,
                typeOption,
                allOption,
                ansiOption,
                ansiResetOption,
                quietOption,
                expectedHandler);

            // Act + Assert
            Assert.That(getCommand.Arguments, Contains.Item(identifierArgument));
        }
        #endregion Arguments

        #region Options
        [Test]
        public void Options_HasExpectedCount()
        {
            // Arrange
            var identifierArgument = new Argument<string>();

            var typeOption = new Option<ContentType>("--type");
            var allOption = new Option<bool>("--all");
            var ansiOption = new Option<bool>("--ansi");
            var ansiResetOption = new Option<AnsiResetType>("--ansi-reset");
            var quietOption = new Option<bool>("--quiet");

            var expectedHandler = (IConsoleWriter _, IValueFormatter _, ClipboardItemIdentifier _, ContentType _, bool _, CancellationToken _) => Task.FromResult(0);

            var getCommand = new GetCommand(
                identifierArgument,
                typeOption,
                allOption,
                ansiOption,
                ansiResetOption,
                quietOption,
                expectedHandler);

            // Act + Assert
            Assert.That(getCommand.Options, Has.Exactly(5).Items);
        }

        [Test]
        public void Options_HasAllExpectedProvidedOptions()
        {
            // Arrange
            var identifierArgument = new Argument<string>();

            var typeOption = new Option<ContentType>("--type");
            var allOption = new Option<bool>("--all");
            var ansiOption = new Option<bool>("--ansi");
            var ansiResetOption = new Option<AnsiResetType>("--ansi-reset");
            var quietOption = new Option<bool>("--quiet");

            var expectedHandler = (IConsoleWriter _, IValueFormatter _, ClipboardItemIdentifier _, ContentType _, bool _, CancellationToken _) => Task.FromResult(0);

            var getCommand = new GetCommand(
                identifierArgument,
                typeOption,
                allOption,
                ansiOption,
                ansiResetOption,
                quietOption,
                expectedHandler);

            // Act + Assert
            Assert.That(getCommand.Options, Contains.Item(typeOption));
            Assert.That(getCommand.Options, Contains.Item(allOption));
            Assert.That(getCommand.Options, Contains.Item(ansiOption));
            Assert.That(getCommand.Options, Contains.Item(ansiResetOption));

            Assert.That(getCommand.Options, Does.Not.Contain(quietOption));
        }

        [Test]
        public void Options_HasSetCurrentOption()
        {
            // Arrange
            var expectedSetCurrentOptionName = "set-current";
            var expectedSetCurrentOptionDescription = "Sets the current clipboard contents to the returned history item";

            var identifierArgument = new Argument<string>();

            var typeOption = new Option<ContentType>("--type");
            var allOption = new Option<bool>("--all");
            var ansiOption = new Option<bool>("--ansi");
            var ansiResetOption = new Option<AnsiResetType>("--ansi-reset");
            var quietOption = new Option<bool>("--quiet");

            var expectedHandler = (IConsoleWriter _, IValueFormatter _, ClipboardItemIdentifier _, ContentType _, bool _, CancellationToken _) => Task.FromResult(0);

            var getCommand = new GetCommand(
                identifierArgument,
                typeOption,
                allOption,
                ansiOption,
                ansiResetOption,
                quietOption,
                expectedHandler);

            // Act + Assert
            var setCurrentOptionMatches = getCommand.Options.Where(option =>
                option.Name == expectedSetCurrentOptionName &&
                option.Description == expectedSetCurrentOptionDescription);
            Assert.That(setCurrentOptionMatches, Has.Exactly(1).Items);
        }
        #endregion Options
    }
}
