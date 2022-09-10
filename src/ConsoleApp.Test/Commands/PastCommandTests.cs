using Moq;
using past.ConsoleApp.Commands;
using past.ConsoleApp.Test.TestHelpers;
using System.CommandLine;

namespace past.ConsoleApp.Test.Commands
{
    public class PastCommandTests
    {
        #region Constructors
        [Test]
        public void Constructor_Parameterless_Success()
        {
            Assert.DoesNotThrow(() => new PastCommand());
        }

        [Test]
        public void Consructor_NonNullConsoleClipboard_Success()
        {
            var mockConsoleClipboard = new Mock<IConsoleClipboard>(MockBehavior.Strict);
            Assert.DoesNotThrow(() => new PastCommand(mockConsoleClipboard.Object));
        }

        [Test]
        public void Constructor_NullConsoleClipboard_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new PastCommand(null!));
        }
        #endregion Constructors

        #region Command
        [Test]
        public void PastCommand_IsRootCommand()
        {
            var mockConsoleClipboard = new Mock<IConsoleClipboard>(MockBehavior.Strict);
            var pastCommand = new PastCommand(mockConsoleClipboard.Object);
            Assert.That(pastCommand, Is.InstanceOf<RootCommand>());
        }

        [Test]
        public void PastCommand_HasExpectedDescription()
        {
            // Arrange
            var mockConsoleClipboard = new Mock<IConsoleClipboard>(MockBehavior.Strict);
            var pastCommand = new PastCommand(mockConsoleClipboard.Object);

            // Act + Assert
            Assert.That(pastCommand.Description, Is.EqualTo("A CLI for interating with Windows Clipboard History."));
        }

        [Test]
        public void PastCommand_HasExpectedHandler()
        {
            // Arrange
            var mockConsoleClipboard = new Mock<IConsoleClipboard>(MockBehavior.Strict);
            var expectedPastCommandHandler = mockConsoleClipboard.Object.GetCurrentClipboardValueAsync;
            var pastCommand = new PastCommand(mockConsoleClipboard.Object);

            // Act + Assert
            var pastCommandHandler = pastCommand.GetHandler();
            Assert.That(pastCommandHandler, Is.EqualTo(expectedPastCommandHandler));
        }
        #endregion Command

        #region Subcommands
        [Test]
        public void Subcommands_HasExpectedCount()
        {
            // Arrange
            var mockConsoleClipboard = new Mock<IConsoleClipboard>(MockBehavior.Strict);
            var pastCommand = new PastCommand(mockConsoleClipboard.Object);

            // Act + Assert
            Assert.That(pastCommand.Subcommands, Has.Exactly(4).Items);
        }

        [Test]
        public void Subcommands_HasListCommand()
        {
            // Arrange
            var mockConsoleClipboard = new Mock<IConsoleClipboard>(MockBehavior.Strict);
            var expectedListCommandHandler = mockConsoleClipboard.Object.ListClipboardHistoryAsync;
            var pastCommand = new PastCommand(mockConsoleClipboard.Object);

            // Act + Assert
            var listCommandMatches = pastCommand.Subcommands.Where(command => command.Name == "list");
            Assert.That(listCommandMatches, Has.Exactly(1).Items);

            var listCommand = listCommandMatches.First();
            Assert.That(listCommand, Is.InstanceOf<ListCommand>());

            var listCommandHandler = listCommand.GetHandler();
            Assert.That(listCommandHandler, Is.EqualTo(expectedListCommandHandler));
        }

        [Test]
        public void Subcommands_HasGetCommand()
        {
            // Arrange
            var mockConsoleClipboard = new Mock<IConsoleClipboard>(MockBehavior.Strict);
            var expectedGetCommandHandler = mockConsoleClipboard.Object.GetClipboardHistoryItemAsync;
            var pastCommand = new PastCommand(mockConsoleClipboard.Object);

            // Act + Assert
            var getCommandMatches = pastCommand.Subcommands.Where(command => command.Name == "get");
            Assert.That(getCommandMatches, Has.Exactly(1).Items);

            var getCommand = getCommandMatches.First();
            Assert.That(getCommand, Is.InstanceOf<GetCommand>());

            var getCommandHandler = getCommand.GetHandler();
            Assert.That(getCommandHandler, Is.EqualTo(expectedGetCommandHandler));
        }

        [Test]
        public void Subcommands_HasStatusCommand()
        {
            // Arrange
            var mockConsoleClipboard = new Mock<IConsoleClipboard>(MockBehavior.Strict);
            var expectedStatusCommandHandler = mockConsoleClipboard.Object.GetClipboardHistoryStatus;
            var pastCommand = new PastCommand(mockConsoleClipboard.Object);

            // Act + Assert
            var statusCommandMatches = pastCommand.Subcommands.Where(command => command.Name == "status");
            Assert.That(statusCommandMatches, Has.Exactly(1).Items);

            var statusCommand = statusCommandMatches.First();
            Assert.That(statusCommand, Is.InstanceOf<StatusCommand>());

            var statusCommandHandler = statusCommand.GetHandler();
            Assert.That(statusCommandHandler, Is.EqualTo(expectedStatusCommandHandler));
        }

        [Test]
        public void Subcommands_HasHelpCommand()
        {
            // Arrange
            var mockConsoleClipboard = new Mock<IConsoleClipboard>(MockBehavior.Strict);
            var pastCommand = new PastCommand(mockConsoleClipboard.Object);

            // Act + Assert
            var helpCommandMatches = pastCommand.Subcommands.Where(command => command.Name == "help");
            Assert.That(helpCommandMatches, Has.Exactly(1).Items);

            var helpCommand = helpCommandMatches.First();
            Assert.That(helpCommand, Is.InstanceOf<HelpCommand>());
        }
        #endregion Subcommands

        #region Options
        [Test]
        public void Options_HasExpectedCount()
        {
            // Arrange
            var mockConsoleClipboard = new Mock<IConsoleClipboard>(MockBehavior.Strict);
            var pastCommand = new PastCommand(mockConsoleClipboard.Object);

            // Act + Assert
            Assert.That(pastCommand.Options, Has.Exactly(6).Items);
        }

        [Test]
        public void Options_HasTypeOption()
        {
            // Arrange
            var mockConsoleClipboard = new Mock<IConsoleClipboard>(MockBehavior.Strict);
            var pastCommand = new PastCommand(mockConsoleClipboard.Object);

            // Act + Assert
            var typeOptionMatches = pastCommand.Options.Where(option =>
                option.Name == "type");
            Assert.That(typeOptionMatches, Has.Exactly(1).Items);

            var typeOption = typeOptionMatches.First();
            Assert.That(typeOption.Aliases, Has.Exactly(2).Items);
            Assert.That(typeOption.HasAlias("--type"), Is.True);
            Assert.That(typeOption.HasAlias("-t"), Is.True);
            Assert.That(typeOption.Description,
                Is.EqualTo("The type of content to read from the clipboard. (default: Text)"));
        }

        [Test]
        public void Options_HasAllOption()
        {
            // Arrange
            var mockConsoleClipboard = new Mock<IConsoleClipboard>(MockBehavior.Strict);
            var pastCommand = new PastCommand(mockConsoleClipboard.Object);

            // Act + Assert
            var allOptionMatches = pastCommand.Options.Where(option =>
                option.Name == "all");
            Assert.That(allOptionMatches, Has.Exactly(1).Items);

            var allOption = allOptionMatches.First();
            Assert.That(allOption.Aliases, Has.Exactly(1).Items);
            Assert.That(allOption.HasAlias("--all"), Is.True);
            Assert.That(allOption.Description,
                Is.EqualTo("Alias for `--type all`. Overrides the `--type` option if present."));
        }

        [Test]
        public void Options_HasAnsiOption()
        {
            // Arrange
            var mockConsoleClipboard = new Mock<IConsoleClipboard>(MockBehavior.Strict);
            var pastCommand = new PastCommand(mockConsoleClipboard.Object);

            // Act + Assert
            var ansiOptionMatches = pastCommand.Options.Where(option =>
                option.Name == "ansi");
            Assert.That(ansiOptionMatches, Has.Exactly(1).Items);

            var ansiOption = ansiOptionMatches.First();
            Assert.That(ansiOption.Aliases, Has.Exactly(1).Items);
            Assert.That(ansiOption.HasAlias("--ansi"), Is.True);
            Assert.That(ansiOption.Description,
                Is.EqualTo("Enable processing of ANSI control sequences"));
        }

        [Test]
        public void Options_HasAnsiResetOption()
        {
            // Arrange
            var mockConsoleClipboard = new Mock<IConsoleClipboard>(MockBehavior.Strict);
            var pastCommand = new PastCommand(mockConsoleClipboard.Object);

            // Act + Assert
            var ansiResetOptionMatches = pastCommand.Options.Where(option =>
                option.Name == "ansi-reset");
            Assert.That(ansiResetOptionMatches, Has.Exactly(1).Items);

            var ansiResetOption = ansiResetOptionMatches.First();
            Assert.That(ansiResetOption.Aliases, Has.Exactly(1).Items);
            Assert.That(ansiResetOption.HasAlias("--ansi-reset"), Is.True);
            Assert.That(ansiResetOption.Description,
                Is.EqualTo("Controls whether to emit the ANSI reset escape code after printing an item. Auto will only emit ANSI reset when another ANSI escape sequence is detected in that item."));
        }

        [Test]
        public void Options_HasQuietOption()
        {
            // Arrange
            var mockConsoleClipboard = new Mock<IConsoleClipboard>(MockBehavior.Strict);
            var pastCommand = new PastCommand(mockConsoleClipboard.Object);

            // Act + Assert
            var quietOptionMatches = pastCommand.Options.Where(option =>
                option.Name == "quiet");
            Assert.That(quietOptionMatches, Has.Exactly(1).Items);

            var quietOption = quietOptionMatches.First();
            Assert.That(quietOption.GetIsGlobal(), Is.True);
            Assert.That(quietOptionMatches, Has.Exactly(1).Items);
            Assert.That(quietOption.Aliases, Has.Exactly(2).Items);
            Assert.That(quietOption.HasAlias("--quiet"), Is.True);
            Assert.That(quietOption.HasAlias("-q"), Is.True);
            Assert.That(quietOption.Description, Is.EqualTo("Suppresses error output"));
        }

        [Test]
        public void Options_HasDebugOption()
        {
            // Arrange
            var mockConsoleClipboard = new Mock<IConsoleClipboard>(MockBehavior.Strict);
            var pastCommand = new PastCommand(mockConsoleClipboard.Object);

            // Act + Assert
            var debugOptionMatches = pastCommand.Options.Where(option =>
                option.Name == "debug");
            Assert.That(debugOptionMatches, Has.Exactly(1).Items);

            var debugOption = debugOptionMatches.First();
            Assert.That(debugOption.GetIsGlobal(), Is.True);
            Assert.That(debugOption.Aliases, Has.Exactly(1).Items);
            Assert.That(debugOption.HasAlias("--debug"), Is.True);
            Assert.That(debugOption.Description,
                Is.EqualTo("Prints additional diagnostic output." +
                "\n[Debug Builds Only] Halts execution on startup to allow attaching a debugger."));
        }
        #endregion Options
    }
}
