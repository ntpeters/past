using past.ConsoleApp.Commands;
using past.ConsoleApp.Output;
using past.ConsoleApp.Test.TestHelpers;
using past.Core;
using System.CommandLine;

namespace past.ConsoleApp.Test.Commands
{
    public class ListCommandTests
    {
        #region Constructor
        [Test]
        public void Constructor_ValidParameters_Success()
        {
            // Arrange
            var typeOption = new Option<ContentType>("--type");
            var allOption = new Option<bool>("--all");
            var ansiOption = new Option<bool>("--ansi");
            var ansiResetOption = new Option<AnsiResetType>("--ansi-reset");
            var quietOption = new Option<bool>("--quiet");

            var expectedHandler = (IConsoleWriter _, IValueFormatter _, ContentType _, bool _, CancellationToken _) => Task.FromResult(0);

            // Act + Assert
            Assert.DoesNotThrow(() => new ListCommand(
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
            var allOption = new Option<bool>("--all");
            var ansiOption = new Option<bool>("--ansi");
            var ansiResetOption = new Option<AnsiResetType>("--ansi-reset");
            var quietOption = new Option<bool>("--quiet");

            var expectedHandler = (IConsoleWriter _, IValueFormatter _, ContentType _, bool _, CancellationToken _) => Task.FromResult(0);

            // Act + Assert
            Assert.Throws<ArgumentNullException>(() => new ListCommand(
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
            var typeOption = new Option<ContentType>("--type");
            var ansiOption = new Option<bool>("--ansi");
            var ansiResetOption = new Option<AnsiResetType>("--ansi-reset");
            var quietOption = new Option<bool>("--quiet");

            var expectedHandler = (IConsoleWriter _, IValueFormatter _, ContentType _, bool _, CancellationToken _) => Task.FromResult(0);

            // Act + Assert
            Assert.Throws<ArgumentNullException>(() => new ListCommand(
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
            var typeOption = new Option<ContentType>("--type");
            var allOption = new Option<bool>("--all");
            var ansiResetOption = new Option<AnsiResetType>("--ansi-reset");
            var quietOption = new Option<bool>("--quiet");

            var expectedHandler = (IConsoleWriter _, IValueFormatter _, ContentType _, bool _, CancellationToken _) => Task.FromResult(0);

            // Act + Assert
            Assert.Throws<ArgumentNullException>(() => new ListCommand(
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
            var typeOption = new Option<ContentType>("--type");
            var allOption = new Option<bool>("--all");
            var ansiOption = new Option<bool>("--ansi");
            var quietOption = new Option<bool>("--quiet");

            var expectedHandler = (IConsoleWriter _, IValueFormatter _, ContentType _, bool _, CancellationToken _) => Task.FromResult(0);

            // Act + Assert
            Assert.Throws<ArgumentNullException>(() => new ListCommand(
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
            var typeOption = new Option<ContentType>("--type");
            var allOption = new Option<bool>("--all");
            var ansiOption = new Option<bool>("--ansi");
            var ansiResetOption = new Option<AnsiResetType>("--ansi-reset");

            var expectedHandler = (IConsoleWriter _, IValueFormatter _, ContentType _, bool _, CancellationToken _) => Task.FromResult(0);

            // Act + Assert
            Assert.Throws<ArgumentNullException>(() => new ListCommand(
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
            var typeOption = new Option<ContentType>("--type");
            var allOption = new Option<bool>("--all");
            var ansiOption = new Option<bool>("--ansi");
            var ansiResetOption = new Option<AnsiResetType>("--ansi-reset");
            var quietOption = new Option<bool>("--quiet");

            var expectedHandler = (IConsoleWriter _, IValueFormatter _, ContentType _, bool _, CancellationToken _) => Task.FromResult(0);

            // Act + Assert
            Assert.Throws<ArgumentNullException>(() => new ListCommand(
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
        public void ListCommand_HasExpectedNameAndDescription()
        {
            // Arrange
            var expectedCommandName = "list";
            var expctedCommandDescription = "Lists the clipboard history";

            var typeOption = new Option<ContentType>("--type");
            var allOption = new Option<bool>("--all");
            var ansiOption = new Option<bool>("--ansi");
            var ansiResetOption = new Option<AnsiResetType>("--ansi-reset");
            var quietOption = new Option<bool>("--quiet");

            var expectedHandler = (IConsoleWriter _, IValueFormatter _, ContentType _, bool _, CancellationToken _) => Task.FromResult(0);

            var listCommand = new ListCommand(
                typeOption,
                allOption,
                ansiOption,
                ansiResetOption,
                quietOption,
                expectedHandler);

            // Act + Assert
            Assert.That(listCommand.Name, Is.EqualTo(expectedCommandName));
            Assert.That(listCommand.Description, Is.EqualTo(expctedCommandDescription));
        }

        [Test]
        public void ListCommand_HasExpectedHandler()
        {
            // Arrange
            var typeOption = new Option<ContentType>("--type");
            var allOption = new Option<bool>("--all");
            var ansiOption = new Option<bool>("--ansi");
            var ansiResetOption = new Option<AnsiResetType>("--ansi-reset");
            var quietOption = new Option<bool>("--quiet");

            var expectedHandler = (IConsoleWriter _, IValueFormatter _, ContentType _, bool _, CancellationToken _) => Task.FromResult(0);

            var listCommand = new ListCommand(
                typeOption,
                allOption,
                ansiOption,
                ansiResetOption,
                quietOption,
                expectedHandler);

            // Act + Assert
            var actualHandler = listCommand.GetHandler();
            Assert.That(actualHandler, Is.EqualTo(expectedHandler));
        }

        [Test]
        public void ListCommand_HasNoSubcommands()
        {
            // Arrange
            var typeOption = new Option<ContentType>("--type");
            var allOption = new Option<bool>("--all");
            var ansiOption = new Option<bool>("--ansi");
            var ansiResetOption = new Option<AnsiResetType>("--ansi-reset");
            var quietOption = new Option<bool>("--quiet");

            var expectedHandler = (IConsoleWriter _, IValueFormatter _, ContentType _, bool _, CancellationToken _) => Task.FromResult(0);

            var listCommand = new ListCommand(
                typeOption,
                allOption,
                ansiOption,
                ansiResetOption,
                quietOption,
                expectedHandler);

            // Act + Assert
            Assert.That(listCommand.Subcommands, Is.Empty);
        }

        [Test]
        public void ListCommand_HasNoArguments()
        {
            // Arrange
            var typeOption = new Option<ContentType>("--type");
            var allOption = new Option<bool>("--all");
            var ansiOption = new Option<bool>("--ansi");
            var ansiResetOption = new Option<AnsiResetType>("--ansi-reset");
            var quietOption = new Option<bool>("--quiet");

            var expectedHandler = (IConsoleWriter _, IValueFormatter _, ContentType _, bool _, CancellationToken _) => Task.FromResult(0);

            var listCommand = new ListCommand(
                typeOption,
                allOption,
                ansiOption,
                ansiResetOption,
                quietOption,
                expectedHandler);

            // Act + Assert
            Assert.That(listCommand.Arguments, Is.Empty);
        }
        #endregion Command

        #region Options
        [Test]
        public void Options_HasExpectedCount()
        {
            // Arrange
            var typeOption = new Option<ContentType>("--type");
            var allOption = new Option<bool>("--all");
            var ansiOption = new Option<bool>("--ansi");
            var ansiResetOption = new Option<AnsiResetType>("--ansi-reset");
            var quietOption = new Option<bool>("--quiet");

            var expectedHandler = (IConsoleWriter _, IValueFormatter _, ContentType _, bool _, CancellationToken _) => Task.FromResult(0);

            var listCommand = new ListCommand(
                typeOption,
                allOption,
                ansiOption,
                ansiResetOption,
                quietOption,
                expectedHandler);

            // Act + Assert
            Assert.That(listCommand.Options, Has.Exactly(9).Items);
        }

        [Test]
        public void Options_HasAllExpectedProvidedOptions()
        {
            // Arrange
            var typeOption = new Option<ContentType>("--type");
            var allOption = new Option<bool>("--all");
            var ansiOption = new Option<bool>("--ansi");
            var ansiResetOption = new Option<AnsiResetType>("--ansi-reset");
            var quietOption = new Option<bool>("--quiet");

            var expectedHandler = (IConsoleWriter _, IValueFormatter _, ContentType _, bool _, CancellationToken _) => Task.FromResult(0);

            var listCommand = new ListCommand(
                typeOption,
                allOption,
                ansiOption,
                ansiResetOption,
                quietOption,
                expectedHandler);

            // Act + Assert
            Assert.That(listCommand.Options, Contains.Item(typeOption));
            Assert.That(listCommand.Options, Contains.Item(allOption));
            Assert.That(listCommand.Options, Contains.Item(ansiOption));
            Assert.That(listCommand.Options, Contains.Item(ansiResetOption));

            Assert.That(listCommand.Options, Does.Not.Contain(quietOption));
        }

        [Test]
        public void Options_HasNullOption()
        {
            // Arrange
            var expectedNullOptionName = "null";
            var expectedNullOptionDescription = "Use the null byte to separate entries";

            var typeOption = new Option<ContentType>("--type");
            var allOption = new Option<bool>("--all");
            var ansiOption = new Option<bool>("--ansi");
            var ansiResetOption = new Option<AnsiResetType>("--ansi-reset");
            var quietOption = new Option<bool>("--quiet");

            var expectedHandler = (IConsoleWriter _, IValueFormatter _, ContentType _, bool _, CancellationToken _) => Task.FromResult(0);

            var listCommand = new ListCommand(
                typeOption,
                allOption,
                ansiOption,
                ansiResetOption,
                quietOption,
                expectedHandler);

            // Act + Assert
            var nullOptionMatches = listCommand.Options.Where(option =>
                option.Name == expectedNullOptionName &&
                option.Description == expectedNullOptionDescription);
            Assert.That(nullOptionMatches, Has.Exactly(1).Items);
        }

        [Test]
        public void Options_HasIndexOption()
        {
            // Arrange
            var expectedIndexOptionName = "index";
            var expectedIndexOptionDescription = "Print indices with each item";

            var typeOption = new Option<ContentType>("--type");
            var allOption = new Option<bool>("--all");
            var ansiOption = new Option<bool>("--ansi");
            var ansiResetOption = new Option<AnsiResetType>("--ansi-reset");
            var quietOption = new Option<bool>("--quiet");

            var expectedHandler = (IConsoleWriter _, IValueFormatter _, ContentType _, bool _, CancellationToken _) => Task.FromResult(0);

            var listCommand = new ListCommand(
                typeOption,
                allOption,
                ansiOption,
                ansiResetOption,
                quietOption,
                expectedHandler);

            // Act + Assert
            var indexOptionMatches = listCommand.Options.Where(option =>
                option.Name == expectedIndexOptionName &&
                option.Description == expectedIndexOptionDescription);
            Assert.That(indexOptionMatches, Has.Exactly(1).Items);
        }

        [Test]
        public void Options_HasIdOption()
        {
            // Arrange
            var expectedIdOptionName = "id";
            var expectedIdOptionDescription = "Print the ID (GUID) with each item";

            var typeOption = new Option<ContentType>("--type");
            var allOption = new Option<bool>("--all");
            var ansiOption = new Option<bool>("--ansi");
            var ansiResetOption = new Option<AnsiResetType>("--ansi-reset");
            var quietOption = new Option<bool>("--quiet");

            var expectedHandler = (IConsoleWriter _, IValueFormatter _, ContentType _, bool _, CancellationToken _) => Task.FromResult(0);

            var listCommand = new ListCommand(
                typeOption,
                allOption,
                ansiOption,
                ansiResetOption,
                quietOption,
                expectedHandler);

            // Act + Assert
            var idOptionMatches = listCommand.Options.Where(option =>
                option.Name == expectedIdOptionName &&
                option.Description == expectedIdOptionDescription);
            Assert.That(idOptionMatches, Has.Exactly(1).Items);
        }

        [Test]
        public void Options_HasTimeOption()
        {
            // Arrange
            var expectedTimeOptionName = "time";
            var expectedTimeOptionDescription = "Print the date and time that each item was copied";

            var typeOption = new Option<ContentType>("--type");
            var allOption = new Option<bool>("--all");
            var ansiOption = new Option<bool>("--ansi");
            var ansiResetOption = new Option<AnsiResetType>("--ansi-reset");
            var quietOption = new Option<bool>("--quiet");

            var expectedHandler = (IConsoleWriter _, IValueFormatter _, ContentType _, bool _, CancellationToken _) => Task.FromResult(0);

            var listCommand = new ListCommand(
                typeOption,
                allOption,
                ansiOption,
                ansiResetOption,
                quietOption,
                expectedHandler);

            // Act + Assert
            var timeOptionMatches = listCommand.Options.Where(option =>
                option.Name == expectedTimeOptionName &&
                option.Description == expectedTimeOptionDescription);
            Assert.That(timeOptionMatches, Has.Exactly(1).Items);
        }

        [Test]
        public void Options_HasPinnedOption()
        {
            // Arrange
            var expectedPinnedOptionName = "pinned";
            var expectedPinnedOptionDescription = "Print only pinned items";

            var typeOption = new Option<ContentType>("--type");
            var allOption = new Option<bool>("--all");
            var ansiOption = new Option<bool>("--ansi");
            var ansiResetOption = new Option<AnsiResetType>("--ansi-reset");
            var quietOption = new Option<bool>("--quiet");

            var expectedHandler = (IConsoleWriter _, IValueFormatter _, ContentType _, bool _, CancellationToken _) => Task.FromResult(0);

            var listCommand = new ListCommand(
                typeOption,
                allOption,
                ansiOption,
                ansiResetOption,
                quietOption,
                expectedHandler);

            // Act + Assert
            var pinnedOptionMatches = listCommand.Options.Where(option =>
                option.Name == expectedPinnedOptionName &&
                option.Description == expectedPinnedOptionDescription);
            Assert.That(pinnedOptionMatches, Has.Exactly(1).Items);
        }
        #endregion Options
    }
}
