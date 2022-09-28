using Moq;
using past.ConsoleApp.Commands;
using past.ConsoleApp.Middleware;
using past.ConsoleApp.Output;
using past.ConsoleApp.Test.TestHelpers;
using past.Core;
using past.Core.Models;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.Reflection;
using System.Text;

namespace past.ConsoleApp.Test
{
    public class ProgramTests
    {
        #region Constants
        private const string PastCommandShortHelp = @"past 1.0.0
A CLI for interacting with Windows Clipboard History.

Description:
  Gets the current contents of the clipboard, when no subcommand is specified.

Usage:
  past [command] [options]

Options:
  -t, --type <All|Image|Text>  The type of content to read from the clipboard. (default: Text) [default: Text]
  --all                        Alias for `--type all`. Overrides the `--type` option if present.
  --ansi                       Enable processing of ANSI control sequences
  --ansi-reset <Auto|Off|On>   Controls whether to emit the ANSI reset escape code after printing an item. Auto will only emit ANSI reset when another ANSI escape sequence is detected in that item. [default: Auto]
  -q, --quiet                  Suppresses error output
  --debug                      Prints additional diagnostic output.
                               [Debug Builds Only] Halts execution on startup to allow attaching a debugger.
  -V, --version                Show version information
  -?, -h, --help               Show help and usage information. Use with a subcommand to show help specific to that command.

Commands:
  list            Lists the clipboard history
  get <index|id>  Gets the item at the specified index from clipboard history
  status          Gets the status of the clipboard history settings on this device

Note: Use `--help` to show the full help including exit codes.

";
        private const string PastCommandLongHelp = @"past 1.0.0
A CLI for interacting with Windows Clipboard History.

Description:
  Gets the current contents of the clipboard, when no subcommand is specified.

Usage:
  past [command] [options]

Options:
  -t, --type <All|Image|Text>  The type of content to read from the clipboard. (default: Text) [default: Text]
  --all                        Alias for `--type all`. Overrides the `--type` option if present.
  --ansi                       Enable processing of ANSI control sequences
  --ansi-reset <Auto|Off|On>   Controls whether to emit the ANSI reset escape code after printing an item. Auto will only emit ANSI reset when another ANSI escape sequence is detected in that item. [default: Auto]
  -q, --quiet                  Suppresses error output
  --debug                      Prints additional diagnostic output.
                               [Debug Builds Only] Halts execution on startup to allow attaching a debugger.
  -V, --version                Show version information
  -?, -h, --help               Show help and usage information. Use with a subcommand to show help specific to that command.

Commands:
  list            Lists the clipboard history
  get <index|id>  Gets the item at the specified index from clipboard history
  status          Gets the status of the clipboard history settings on this device

Exit Codes:
  Success          0
  ParseError       -1
  UnexpectedError  -99

";

        private const string GetCommandShortHelp = @"past get

Description:
  Gets the item at the specified index from clipboard history

Usage:
  past get <identifier> [options]

Arguments:
  <index|id>  The index or ID of the item to get from clipboard history

Options:
  -t, --type <All|Image|Text>  The type of content to read from the clipboard. (default: Text) [default: Text]
  --all                        Alias for `--type all`. Overrides the `--type` option if present.
  --ansi                       Enable processing of ANSI control sequences
  --ansi-reset <Auto|Off|On>   Controls whether to emit the ANSI reset escape code after printing an item. Auto will only emit ANSI reset when another ANSI escape sequence is detected in that item. [default: Auto]
  --set-current                Sets the current clipboard contents to the returned history item
  -q, --quiet                  Suppresses error output
  --debug                      Prints additional diagnostic output.
                               [Debug Builds Only] Halts execution on startup to allow attaching a debugger.
  -?, -h, --help               Show help and usage information


Note: Use `--help` to show the full help including exit codes.

";
        private const string GetCommandLongHelp = @"past get

Description:
  Gets the item at the specified index from clipboard history

Usage:
  past get <identifier> [options]

Arguments:
  <index|id>  The index or ID of the item to get from clipboard history

Options:
  -t, --type <All|Image|Text>  The type of content to read from the clipboard. (default: Text) [default: Text]
  --all                        Alias for `--type all`. Overrides the `--type` option if present.
  --ansi                       Enable processing of ANSI control sequences
  --ansi-reset <Auto|Off|On>   Controls whether to emit the ANSI reset escape code after printing an item. Auto will only emit ANSI reset when another ANSI escape sequence is detected in that item. [default: Auto]
  --set-current                Sets the current clipboard contents to the returned history item
  -q, --quiet                  Suppresses error output
  --debug                      Prints additional diagnostic output.
                               [Debug Builds Only] Halts execution on startup to allow attaching a debugger.
  -?, -h, --help               Show help and usage information


Exit Codes:
  Success                   0
  ParseError                -1
  NotFound                  -2
  IncompatibleContentType   -3
  ClipboardHistoryDisabled  -4
  AccessDenied              -5
  UnexpectedError           -99

";

        private const string ListCommandShortHelp = @"past list

Description:
  Lists the clipboard history

Usage:
  past list [options]

Options:
  -t, --type <All|Image|Text>  The type of content to read from the clipboard. (default: Text) [default: Text]
  --all                        Alias for `--type all`. Overrides the `--type` option if present.
  --ansi                       Enable processing of ANSI control sequences
  --ansi-reset <Auto|Off|On>   Controls whether to emit the ANSI reset escape code after printing an item. Auto will only emit ANSI reset when another ANSI escape sequence is detected in that item. [default: Auto]
  --null                       Use the null byte to separate entries
  --index                      Print indices with each item
  --id                         Print the ID (GUID) with each item
  --time                       Print the date and time that each item was copied
  --pinned                     Print only pinned items
  -q, --quiet                  Suppresses error output
  --debug                      Prints additional diagnostic output.
                               [Debug Builds Only] Halts execution on startup to allow attaching a debugger.
  -?, -h, --help               Show help and usage information

Note: Use `--help` to show the full help including exit codes.

";
        private const string ListCommandLongHelp = @"past list

Description:
  Lists the clipboard history

Usage:
  past list [options]

Options:
  -t, --type <All|Image|Text>  The type of content to read from the clipboard. (default: Text) [default: Text]
  --all                        Alias for `--type all`. Overrides the `--type` option if present.
  --ansi                       Enable processing of ANSI control sequences
  --ansi-reset <Auto|Off|On>   Controls whether to emit the ANSI reset escape code after printing an item. Auto will only emit ANSI reset when another ANSI escape sequence is detected in that item. [default: Auto]
  --null                       Use the null byte to separate entries
  --index                      Print indices with each item
  --id                         Print the ID (GUID) with each item
  --time                       Print the date and time that each item was copied
  --pinned                     Print only pinned items
  -q, --quiet                  Suppresses error output
  --debug                      Prints additional diagnostic output.
                               [Debug Builds Only] Halts execution on startup to allow attaching a debugger.
  -?, -h, --help               Show help and usage information

Exit Codes:
  Success                   The number of items returned
  ParseError                -1
  NotFound                  -2
  ClipboardHistoryDisabled  -4
  AccessDenied              -5
  UnexpectedError           -99

";

        private const string StatusCommandShortHelp = @"past status

Description:
  Gets the status of the clipboard history settings on this device

Usage:
  past status [options]

Options:
  -q, --quiet     Suppresses error output
  --debug         Prints additional diagnostic output.
                  [Debug Builds Only] Halts execution on startup to allow attaching a debugger.
  -?, -h, --help  Show help and usage information

Note: Use `--help` to show the full help including exit codes.

";
        private const string StatusCommandLongHelp = @"past status

Description:
  Gets the status of the clipboard history settings on this device

Usage:
  past status [options]

Options:
  -q, --quiet     Suppresses error output
  --debug         Prints additional diagnostic output.
                  [Debug Builds Only] Halts execution on startup to allow attaching a debugger.
  -?, -h, --help  Show help and usage information

Exit Codes:
  History Enabled, Roaming Enabled    3
  History Disabled, Roaming Enabled   2
  History Enabled, Roaming Disabled   1
  History Disabled, Roaming Disabled  0
  ParseError                          -1
  UnexpectedError                     -99

";
        #endregion Constants

        #region Root Command
        [Test]
        public async Task MainInternal_RootCommand_NoOptions_ParsesWithDefaultValues()
        {
            // Arrange
            var args = new string[] { };
            var expectedExitCode = (int)ErrorCode.Success;

            var mockConsoleModeMiddleware = new Mock<IConsoleModeMiddleware>();
            var mockConsoleClipboard = new Mock<IConsoleClipboard>(MockBehavior.Strict);

            IConsoleWriter? actualConsoleWriter = null;
            IValueFormatter? actualValueFormatter = null;
            ContentType? actualContentType = null;
            mockConsoleClipboard
                .Setup(mock => mock.GetCurrentClipboardValueAsync(
                    It.IsAny<IConsoleWriter>(),
                    It.IsAny<IValueFormatter>(),
                    It.IsAny<ContentType>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((IConsoleWriter consoleWriter, IValueFormatter valueFormatter, ContentType contentType, CancellationToken _) =>
                {
                    actualConsoleWriter = consoleWriter;
                    actualValueFormatter = valueFormatter;
                    actualContentType = contentType;
                    return expectedExitCode;
                });

            var pastCommand = new PastCommand(mockConsoleClipboard.Object);

            var standardOut = new StringBuilder();
            var standardError = new StringBuilder();
            var mockConsole = CreateMockConsole(standardOut, standardError);

            // Act
            var actualExitCode = await Program.MainInternal(args, pastCommand, mockConsoleModeMiddleware.Object, mockConsole.Object);

            // Assert
            Assert.That(actualExitCode, Is.EqualTo(expectedExitCode));

            Assert.That(actualConsoleWriter, Is.Not.Null);
            Assert.That(actualConsoleWriter.SuppressErrorOutput, Is.False);
            Assert.That(actualConsoleWriter.EnableAnsiProcessing, Is.False);
            Assert.That(actualConsoleWriter.AnsiResetType, Is.EqualTo(AnsiResetType.Auto));

            Assert.That(actualValueFormatter, Is.Not.Null);
            Assert.That(actualValueFormatter.NullLineEnding, Is.False);
            Assert.That(actualValueFormatter.IncludeIndex, Is.False);
            Assert.That(actualValueFormatter.IncludeId, Is.False);
            Assert.That(actualValueFormatter.IncludeTimestamp, Is.False);

            Assert.That(actualContentType, Is.EqualTo(ContentType.Text));

            Assert.That(standardOut.ToString(), Is.Empty);
            Assert.That(standardError.ToString(), Is.Empty);

            mockConsoleModeMiddleware.Verify(
                mock => mock.ConfigureConsoleMode(It.IsAny<InvocationContext>()),
                Times.Never);
        }

        [Test]
        public async Task MainInternal_RootCommand_ValidTypeFlag_ParsesCorrectType([EnumValueSource] ContentType expectedContentType)
        {
            // Arrange
            var args = new string[] { "--type", expectedContentType.ToString() };
            var expectedExitCode = (int)ErrorCode.Success;

            var mockConsoleModeMiddleware = new Mock<IConsoleModeMiddleware>();
            var mockConsoleClipboard = new Mock<IConsoleClipboard>(MockBehavior.Strict);

            IConsoleWriter? actualConsoleWriter = null;
            IValueFormatter? actualValueFormatter = null;
            ContentType? actualContentType = null;
            mockConsoleClipboard
                .Setup(mock => mock.GetCurrentClipboardValueAsync(
                    It.IsAny<IConsoleWriter>(),
                    It.IsAny<IValueFormatter>(),
                    It.IsAny<ContentType>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((IConsoleWriter consoleWriter, IValueFormatter valueFormatter, ContentType contentType, CancellationToken _) =>
                {
                    actualConsoleWriter = consoleWriter;
                    actualValueFormatter = valueFormatter;
                    actualContentType = contentType;
                    return expectedExitCode;
                });

            var pastCommand = new PastCommand(mockConsoleClipboard.Object);

            var standardOut = new StringBuilder();
            var standardError = new StringBuilder();
            var mockConsole = CreateMockConsole(standardOut, standardError);

            // Act
            var actualExitCode = await Program.MainInternal(args, pastCommand, mockConsoleModeMiddleware.Object, mockConsole.Object);

            // Assert
            Assert.That(actualExitCode, Is.EqualTo(expectedExitCode));

            Assert.That(actualConsoleWriter, Is.Not.Null);
            Assert.That(actualConsoleWriter.SuppressErrorOutput, Is.False);
            Assert.That(actualConsoleWriter.EnableAnsiProcessing, Is.False);
            Assert.That(actualConsoleWriter.AnsiResetType, Is.EqualTo(AnsiResetType.Auto));

            Assert.That(actualValueFormatter, Is.Not.Null);
            Assert.That(actualValueFormatter.NullLineEnding, Is.False);
            Assert.That(actualValueFormatter.IncludeIndex, Is.False);
            Assert.That(actualValueFormatter.IncludeId, Is.False);
            Assert.That(actualValueFormatter.IncludeTimestamp, Is.False);

            Assert.That(actualContentType, Is.EqualTo(expectedContentType));

            Assert.That(standardOut.ToString(), Is.Empty);
            Assert.That(standardError.ToString(), Is.Empty);

            mockConsoleModeMiddleware.Verify(
                mock => mock.ConfigureConsoleMode(It.IsAny<InvocationContext>()),
                Times.Never);
        }

        [Test]
        public async Task MainInternal_RootCommand_InvalidTypeFlag_ReturnsParseError()
        {
            // Arrange
            var args = new string[] { "--type", "foobar" };
            var expectedExitCode = (int)ErrorCode.ParseError;
            var expectedErrorOutput = $"Invalid type specified. Valid values are: Text,Image,All{Environment.NewLine}{Environment.NewLine}";

            var mockConsoleClipboard = new Mock<IConsoleClipboard>(MockBehavior.Strict);
            var pastCommand = new PastCommand(mockConsoleClipboard.Object);

            var standardOut = new StringBuilder();
            var standardError = new StringBuilder();
            var mockConsole = CreateMockConsole(standardOut, standardError);

            // Act
            var actualExitCode = await Program.MainInternal(args, pastCommand, null, mockConsole.Object);

            // Assert
            Assert.That(actualExitCode, Is.EqualTo(expectedExitCode));

            Assert.That(standardOut.ToString(), Is.EqualTo(PastCommandShortHelp));
            Assert.That(standardError.ToString(), Is.EqualTo(expectedErrorOutput));
        }

        [Test]
        public async Task MainInternal_RootCommand_ValidAnsiResetFlag_ParsesCorrectType([EnumValueSource] AnsiResetType expectedAnsiResetType)
        {
            // Arrange
            var args = new string[] { "--ansi-reset", expectedAnsiResetType.ToString() };
            var expectedExitCode = (int)ErrorCode.Success;

            var mockConsoleModeMiddleware = new Mock<IConsoleModeMiddleware>();
            var mockConsoleClipboard = new Mock<IConsoleClipboard>(MockBehavior.Strict);

            IConsoleWriter? actualConsoleWriter = null;
            IValueFormatter? actualValueFormatter = null;
            ContentType? actualContentType = null;
            mockConsoleClipboard
                .Setup(mock => mock.GetCurrentClipboardValueAsync(
                    It.IsAny<IConsoleWriter>(),
                    It.IsAny<IValueFormatter>(),
                    It.IsAny<ContentType>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((IConsoleWriter consoleWriter, IValueFormatter valueFormatter, ContentType contentType, CancellationToken _) =>
                {
                    actualConsoleWriter = consoleWriter;
                    actualValueFormatter = valueFormatter;
                    actualContentType = contentType;
                    return expectedExitCode;
                });

            var pastCommand = new PastCommand(mockConsoleClipboard.Object);

            var standardOut = new StringBuilder();
            var standardError = new StringBuilder();
            var mockConsole = CreateMockConsole(standardOut, standardError);

            // Act
            var actualExitCode = await Program.MainInternal(args, pastCommand, mockConsoleModeMiddleware.Object, mockConsole.Object);

            // Assert
            Assert.That(actualExitCode, Is.EqualTo(expectedExitCode));

            Assert.That(actualConsoleWriter, Is.Not.Null);
            Assert.That(actualConsoleWriter.SuppressErrorOutput, Is.False);
            Assert.That(actualConsoleWriter.EnableAnsiProcessing, Is.False);
            Assert.That(actualConsoleWriter.AnsiResetType, Is.EqualTo(expectedAnsiResetType));

            Assert.That(actualValueFormatter, Is.Not.Null);
            Assert.That(actualValueFormatter.NullLineEnding, Is.False);
            Assert.That(actualValueFormatter.IncludeIndex, Is.False);
            Assert.That(actualValueFormatter.IncludeId, Is.False);
            Assert.That(actualValueFormatter.IncludeTimestamp, Is.False);

            Assert.That(actualContentType, Is.EqualTo(ContentType.Text));

            Assert.That(standardOut.ToString(), Is.Empty);
            Assert.That(standardError.ToString(), Is.Empty);

            mockConsoleModeMiddleware.Verify(
                mock => mock.ConfigureConsoleMode(It.IsAny<InvocationContext>()),
                Times.Never);
        }

        [Test]
        public async Task MainInternal_RootCommand_InvalidAnsiResetFlag_ReturnsParseError()
        {
            // Arrange
            var args = new string[] { "--ansi-reset", "foobar" };
            var expectedExitCode = (int)ErrorCode.ParseError;
            var expectedErrorOutput = $"Invalid type specified. Valid values are: Auto,On,Off{Environment.NewLine}{Environment.NewLine}";

            var mockConsoleClipboard = new Mock<IConsoleClipboard>(MockBehavior.Strict);
            var pastCommand = new PastCommand(mockConsoleClipboard.Object);

            var standardOut = new StringBuilder();
            var standardError = new StringBuilder();
            var mockConsole = CreateMockConsole(standardOut, standardError);

            // Act
            var actualExitCode = await Program.MainInternal(args, pastCommand, null, mockConsole.Object);

            // Assert
            Assert.That(actualExitCode, Is.EqualTo(expectedExitCode));

            Assert.That(standardOut.ToString(), Is.EqualTo(PastCommandShortHelp));
            Assert.That(standardError.ToString(), Is.EqualTo(expectedErrorOutput));
        }

        [Test]
        public async Task MainInternal_RootCommand_AllFlag_ParsesToAllContentType()
        {
            // Arrange
            var args = new string[] { "--all" };
            var expectedContentType = ContentType.All;
            var expectedExitCode = (int)ErrorCode.Success;

            var mockConsoleModeMiddleware = new Mock<IConsoleModeMiddleware>();
            var mockConsoleClipboard = new Mock<IConsoleClipboard>(MockBehavior.Strict);

            IConsoleWriter? actualConsoleWriter = null;
            IValueFormatter? actualValueFormatter = null;
            ContentType? actualContentType = null;
            mockConsoleClipboard
                .Setup(mock => mock.GetCurrentClipboardValueAsync(
                    It.IsAny<IConsoleWriter>(),
                    It.IsAny<IValueFormatter>(),
                    It.IsAny<ContentType>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((IConsoleWriter consoleWriter, IValueFormatter valueFormatter, ContentType contentType, CancellationToken _) =>
                {
                    actualConsoleWriter = consoleWriter;
                    actualValueFormatter = valueFormatter;
                    actualContentType = contentType;
                    return expectedExitCode;
                });

            var pastCommand = new PastCommand(mockConsoleClipboard.Object);

            var standardOut = new StringBuilder();
            var standardError = new StringBuilder();
            var mockConsole = CreateMockConsole(standardOut, standardError);

            // Act
            var actualExitCode = await Program.MainInternal(args, pastCommand, mockConsoleModeMiddleware.Object, mockConsole.Object);

            // Assert
            Assert.That(actualExitCode, Is.EqualTo(expectedExitCode));

            Assert.That(actualConsoleWriter, Is.Not.Null);
            Assert.That(actualConsoleWriter.SuppressErrorOutput, Is.False);
            Assert.That(actualConsoleWriter.EnableAnsiProcessing, Is.False);
            Assert.That(actualConsoleWriter.AnsiResetType, Is.EqualTo(AnsiResetType.Auto));

            Assert.That(actualValueFormatter, Is.Not.Null);
            Assert.That(actualValueFormatter.NullLineEnding, Is.False);
            Assert.That(actualValueFormatter.IncludeIndex, Is.False);
            Assert.That(actualValueFormatter.IncludeId, Is.False);
            Assert.That(actualValueFormatter.IncludeTimestamp, Is.False);

            Assert.That(actualContentType, Is.EqualTo(expectedContentType));

            Assert.That(standardOut.ToString(), Is.Empty);
            Assert.That(standardError.ToString(), Is.Empty);

            mockConsoleModeMiddleware.Verify(
                mock => mock.ConfigureConsoleMode(It.IsAny<InvocationContext>()),
                Times.Never);
        }

        [Test]
        public async Task MainInternal_RootCommand_AnsiFlag_EnableAnsiProcessingTrue()
        {
            // Arrange
            var args = new string[] { "--ansi" };
            var expectedExitCode = (int)ErrorCode.Success;

            var mockConsoleModeMiddleware = new Mock<IConsoleModeMiddleware>();
            var mockConsoleClipboard = new Mock<IConsoleClipboard>(MockBehavior.Strict);

            IConsoleWriter? actualConsoleWriter = null;
            IValueFormatter? actualValueFormatter = null;
            ContentType? actualContentType = null;
            mockConsoleClipboard
                .Setup(mock => mock.GetCurrentClipboardValueAsync(
                    It.IsAny<IConsoleWriter>(),
                    It.IsAny<IValueFormatter>(),
                    It.IsAny<ContentType>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((IConsoleWriter consoleWriter, IValueFormatter valueFormatter, ContentType contentType, CancellationToken _) =>
                {
                    actualConsoleWriter = consoleWriter;
                    actualValueFormatter = valueFormatter;
                    actualContentType = contentType;
                    return expectedExitCode;
                });

            var pastCommand = new PastCommand(mockConsoleClipboard.Object);

            var standardOut = new StringBuilder();
            var standardError = new StringBuilder();
            var mockConsole = CreateMockConsole(standardOut, standardError);

            // Act
            var actualExitCode = await Program.MainInternal(args, pastCommand, mockConsoleModeMiddleware.Object, mockConsole.Object);

            // Assert
            Assert.That(actualExitCode, Is.EqualTo(expectedExitCode));

            Assert.That(actualConsoleWriter, Is.Not.Null);
            Assert.That(actualConsoleWriter.SuppressErrorOutput, Is.False);
            Assert.That(actualConsoleWriter.EnableAnsiProcessing, Is.True);
            Assert.That(actualConsoleWriter.AnsiResetType, Is.EqualTo(AnsiResetType.Auto));

            Assert.That(actualValueFormatter, Is.Not.Null);
            Assert.That(actualValueFormatter.NullLineEnding, Is.False);
            Assert.That(actualValueFormatter.IncludeIndex, Is.False);
            Assert.That(actualValueFormatter.IncludeId, Is.False);
            Assert.That(actualValueFormatter.IncludeTimestamp, Is.False);

            Assert.That(actualContentType, Is.EqualTo(ContentType.Text));

            Assert.That(standardOut.ToString(), Is.Empty);
            Assert.That(standardError.ToString(), Is.Empty);

            mockConsoleModeMiddleware.Verify(
                mock => mock.ConfigureConsoleMode(It.IsAny<InvocationContext>()),
                Times.Once);
        }

        [Test]
        [TestCase("--quiet")]
        [TestCase("-q")]
        public async Task MainInternal_RootCommand_QuietFlag_SuppressErrorOutputTrue(string quietFlag)
        {
            // Arrange
            var args = new string[] { quietFlag };
            var expectedExitCode = (int)ErrorCode.Success;

            var mockConsoleModeMiddleware = new Mock<IConsoleModeMiddleware>();
            var mockConsoleClipboard = new Mock<IConsoleClipboard>(MockBehavior.Strict);

            IConsoleWriter? actualConsoleWriter = null;
            IValueFormatter? actualValueFormatter = null;
            ContentType? actualContentType = null;
            mockConsoleClipboard
                .Setup(mock => mock.GetCurrentClipboardValueAsync(
                    It.IsAny<IConsoleWriter>(),
                    It.IsAny<IValueFormatter>(),
                    It.IsAny<ContentType>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((IConsoleWriter consoleWriter, IValueFormatter valueFormatter, ContentType contentType, CancellationToken _) =>
                {
                    actualConsoleWriter = consoleWriter;
                    actualValueFormatter = valueFormatter;
                    actualContentType = contentType;
                    return expectedExitCode;
                });

            var pastCommand = new PastCommand(mockConsoleClipboard.Object);

            var standardOut = new StringBuilder();
            var standardError = new StringBuilder();
            var mockConsole = CreateMockConsole(standardOut, standardError);

            // Act
            var actualExitCode = await Program.MainInternal(args, pastCommand, mockConsoleModeMiddleware.Object, mockConsole.Object);

            // Assert
            Assert.That(actualExitCode, Is.EqualTo(expectedExitCode));

            Assert.That(actualConsoleWriter, Is.Not.Null);
            Assert.That(actualConsoleWriter.SuppressErrorOutput, Is.True);
            Assert.That(actualConsoleWriter.EnableAnsiProcessing, Is.False);
            Assert.That(actualConsoleWriter.AnsiResetType, Is.EqualTo(AnsiResetType.Auto));

            Assert.That(actualValueFormatter, Is.Not.Null);
            Assert.That(actualValueFormatter.NullLineEnding, Is.False);
            Assert.That(actualValueFormatter.IncludeIndex, Is.False);
            Assert.That(actualValueFormatter.IncludeId, Is.False);
            Assert.That(actualValueFormatter.IncludeTimestamp, Is.False);

            Assert.That(actualContentType, Is.EqualTo(ContentType.Text));

            Assert.That(standardOut.ToString(), Is.Empty);
            Assert.That(standardError.ToString(), Is.Empty);

            mockConsoleModeMiddleware.Verify(
                mock => mock.ConfigureConsoleMode(It.IsAny<InvocationContext>()),
                Times.Never);
        }

        [Test]
        public async Task MainInternal_RootCommand_VersionFlag_OutputsExpectedVersion()
        {
            // Arrange
            var args = new string[] { "--version" };
            var expectedExitCode = (int)ErrorCode.Success;
            var expectedOutput = $"{GetExpectedVersion()}{Environment.NewLine}";

            var mockConsoleClipboard = new Mock<IConsoleClipboard>(MockBehavior.Strict);
            var pastCommand = new PastCommand(mockConsoleClipboard.Object);

            var standardOut = new StringBuilder();
            var standardError = new StringBuilder();
            var mockConsole = CreateMockConsole(standardOut, standardError);

            // Act
            var actualExitCode = await Program.MainInternal(args, pastCommand, null, mockConsole.Object);

            // Assert
            Assert.That(actualExitCode, Is.EqualTo(expectedExitCode));

            Assert.That(standardOut.ToString(), Is.EqualTo(expectedOutput));
            Assert.That(standardError.ToString(), Is.Empty);
        }

        [Test]
        [TestCase("-h")]
        [TestCase("-?")]
        public async Task MainInternal_RootCommand_HelpShortFlag_OutputsExpectedHelp(string helpFlag)
        {
            // Arrange
            var args = new string[] { helpFlag };
            var expectedExitCode = (int)ErrorCode.Success;

            var mockConsoleClipboard = new Mock<IConsoleClipboard>(MockBehavior.Strict);
            var pastCommand = new PastCommand(mockConsoleClipboard.Object);

            var standardOut = new StringBuilder();
            var standardError = new StringBuilder();
            var mockConsole = CreateMockConsole(standardOut, standardError);

            // Act
            var actualExitCode = await Program.MainInternal(args, pastCommand, null, mockConsole.Object);

            // Assert
            Assert.That(actualExitCode, Is.EqualTo(expectedExitCode));

            Assert.That(standardOut.ToString(), Is.EqualTo(PastCommandShortHelp));
            Assert.That(standardError.ToString(), Is.Empty);
        }

        [Test]
        public async Task MainInternal_RootCommand_HelpLongFlag_OutputsExpectedHelp()
        {
            // Arrange
            var args = new string[] { "--help" };
            var expectedExitCode = (int)ErrorCode.Success;

            var mockConsoleClipboard = new Mock<IConsoleClipboard>(MockBehavior.Strict);
            var pastCommand = new PastCommand(mockConsoleClipboard.Object);

            var standardOut = new StringBuilder();
            var standardError = new StringBuilder();
            var mockConsole = CreateMockConsole(standardOut, standardError);

            // Act
            var actualExitCode = await Program.MainInternal(args, pastCommand, null, mockConsole.Object);

            // Assert
            Assert.That(actualExitCode, Is.EqualTo(expectedExitCode));

            Assert.That(standardOut.ToString(), Is.EqualTo(PastCommandLongHelp));
            Assert.That(standardError.ToString(), Is.Empty);
        }

        [Test]
        public async Task MainInternal_RootCommand_ThrowsException_ReturnsUnexpectedError()
        {
            // Arrange
            var args = new string[] { };
            var expectedExitCode = (int)ErrorCode.UnexpectedError;
            var expectedException = new Exception("Uh-oh! :O");

            var mockConsoleClipboard = new Mock<IConsoleClipboard>(MockBehavior.Strict);
            mockConsoleClipboard
                .Setup(mock => mock.GetCurrentClipboardValueAsync(
                    It.IsAny<IConsoleWriter>(),
                    It.IsAny<IValueFormatter>(),
                    It.IsAny<ContentType>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(expectedException);

            var pastCommand = new PastCommand(mockConsoleClipboard.Object);

            var standardOut = new StringBuilder();
            var standardError = new StringBuilder();
            var mockConsole = CreateMockConsole(standardOut, standardError);

            // Act
            var actualExitCode = await Program.MainInternal(args, pastCommand, null, mockConsole.Object);

            // Assert
            Assert.That(actualExitCode, Is.EqualTo(expectedExitCode));

            Assert.That(standardOut.ToString(), Is.Empty);
            Assert.That(standardError.ToString(), Is.EqualTo($"Unhandled exception: {expectedException}{Environment.NewLine}"));
        }
        #endregion Root Command

        #region Get Command
        [Test]
        [TestCase("5")]
        [TestCase("bc65714b-1c69-448b-967f-7925c6970db5")]
        public async Task MainInternal_GetCommand_ValidArgument_ParsesWithDefaultValues(string rawIdentifier)
        {
            // Arrange
            var args = new string[] { "get", rawIdentifier };
            var expectedExitCode = (int)ErrorCode.Success;

            Assert.That(ClipboardItemIdentifier.TryParse(rawIdentifier, out var expectedIdentifier), Is.True);

            var mockConsoleModeMiddleware = new Mock<IConsoleModeMiddleware>();
            var mockConsoleClipboard = new Mock<IConsoleClipboard>(MockBehavior.Strict);

            IConsoleWriter? actualConsoleWriter = null;
            IValueFormatter? actualValueFormatter = null;
            ClipboardItemIdentifier? actualIdentifier = null;
            ContentType? actualContentType = null;
            bool? actualSetCurrent = null;
            mockConsoleClipboard
                .Setup(mock => mock.GetClipboardHistoryItemAsync(
                    It.IsAny<IConsoleWriter>(),
                    It.IsAny<IValueFormatter>(),
                    It.IsAny<ClipboardItemIdentifier>(),
                    It.IsAny<ContentType>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((IConsoleWriter consoleWriter, IValueFormatter valueFormatter, ClipboardItemIdentifier identifier, ContentType contentType, bool setCurrent, CancellationToken _) =>
                {
                    actualConsoleWriter = consoleWriter;
                    actualValueFormatter = valueFormatter;
                    actualIdentifier = identifier;
                    actualContentType = contentType;
                    actualSetCurrent = setCurrent;
                    return expectedExitCode;
                });

            var pastCommand = new PastCommand(mockConsoleClipboard.Object);

            var standardOut = new StringBuilder();
            var standardError = new StringBuilder();
            var mockConsole = CreateMockConsole(standardOut, standardError);

            // Act
            var actualExitCode = await Program.MainInternal(args, pastCommand, mockConsoleModeMiddleware.Object, mockConsole.Object);

            // Assert
            Assert.That(actualExitCode, Is.EqualTo(expectedExitCode));

            Assert.That(actualConsoleWriter, Is.Not.Null);
            Assert.That(actualConsoleWriter.SuppressErrorOutput, Is.False);
            Assert.That(actualConsoleWriter.EnableAnsiProcessing, Is.False);
            Assert.That(actualConsoleWriter.AnsiResetType, Is.EqualTo(AnsiResetType.Auto));

            Assert.That(actualValueFormatter, Is.Not.Null);
            Assert.That(actualValueFormatter.NullLineEnding, Is.False);
            Assert.That(actualValueFormatter.IncludeIndex, Is.False);
            Assert.That(actualValueFormatter.IncludeId, Is.False);
            Assert.That(actualValueFormatter.IncludeTimestamp, Is.False);

            Assert.That(actualIdentifier, Is.EqualTo(expectedIdentifier));
            Assert.That(actualContentType, Is.EqualTo(ContentType.Text));
            Assert.That(actualSetCurrent, Is.False);

            Assert.That(standardOut.ToString(), Is.Empty);
            Assert.That(standardError.ToString(), Is.Empty);

            mockConsoleModeMiddleware.Verify(
                mock => mock.ConfigureConsoleMode(It.IsAny<InvocationContext>()),
                Times.Never);
        }

        [Test]
        public async Task MainInternal_GetCommand_InvalidArgument_ReturnsParseError()
        {
            // Arrange
            var args = new string[] { "get", "foobar" };
            var expectedExitCode = (int)ErrorCode.ParseError;
            var expectedErrorOutput = $"Invalid identifier specified. Identifier must be either a positive integer or a GUID.{Environment.NewLine}{Environment.NewLine}";

            var mockConsoleClipboard = new Mock<IConsoleClipboard>(MockBehavior.Strict);
            var pastCommand = new PastCommand(mockConsoleClipboard.Object);

            var standardOut = new StringBuilder();
            var standardError = new StringBuilder();
            var mockConsole = CreateMockConsole(standardOut, standardError);

            // Act
            var actualExitCode = await Program.MainInternal(args, pastCommand, null, mockConsole.Object);

            // Assert
            Assert.That(actualExitCode, Is.EqualTo(expectedExitCode));

            Assert.That(standardOut.ToString(), Is.EqualTo(GetCommandShortHelp));
            Assert.That(standardError.ToString(), Is.EqualTo(expectedErrorOutput));
        }

        [Test]
        public async Task MainInternal_GetCommand_ValidTypeFlag_ParsesCorrectType([EnumValueSource] ContentType expectedContentType)
        {
            // Arrange
            ClipboardItemIdentifier expectedIdentifier = 0;
            var args = new string[] { "get", expectedIdentifier.ToString(), "--type", expectedContentType.ToString() };
            var expectedExitCode = (int)ErrorCode.Success;

            var mockConsoleModeMiddleware = new Mock<IConsoleModeMiddleware>();
            var mockConsoleClipboard = new Mock<IConsoleClipboard>(MockBehavior.Strict);

            IConsoleWriter? actualConsoleWriter = null;
            IValueFormatter? actualValueFormatter = null;
            ClipboardItemIdentifier? actualIdentifier = null;
            ContentType? actualContentType = null;
            bool? actualSetCurrent = null;
            mockConsoleClipboard
                .Setup(mock => mock.GetClipboardHistoryItemAsync(
                    It.IsAny<IConsoleWriter>(),
                    It.IsAny<IValueFormatter>(),
                    It.IsAny<ClipboardItemIdentifier>(),
                    It.IsAny<ContentType>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((IConsoleWriter consoleWriter, IValueFormatter valueFormatter, ClipboardItemIdentifier identifier, ContentType contentType, bool setCurrent, CancellationToken _) =>
                {
                    actualConsoleWriter = consoleWriter;
                    actualValueFormatter = valueFormatter;
                    actualIdentifier = identifier;
                    actualContentType = contentType;
                    actualSetCurrent = setCurrent;
                    return expectedExitCode;
                });

            var pastCommand = new PastCommand(mockConsoleClipboard.Object);

            var standardOut = new StringBuilder();
            var standardError = new StringBuilder();
            var mockConsole = CreateMockConsole(standardOut, standardError);

            // Act
            var actualExitCode = await Program.MainInternal(args, pastCommand, mockConsoleModeMiddleware.Object, mockConsole.Object);

            // Assert
            Assert.That(actualExitCode, Is.EqualTo(expectedExitCode));

            Assert.That(actualConsoleWriter, Is.Not.Null);
            Assert.That(actualConsoleWriter.SuppressErrorOutput, Is.False);
            Assert.That(actualConsoleWriter.EnableAnsiProcessing, Is.False);
            Assert.That(actualConsoleWriter.AnsiResetType, Is.EqualTo(AnsiResetType.Auto));

            Assert.That(actualValueFormatter, Is.Not.Null);
            Assert.That(actualValueFormatter.NullLineEnding, Is.False);
            Assert.That(actualValueFormatter.IncludeIndex, Is.False);
            Assert.That(actualValueFormatter.IncludeId, Is.False);
            Assert.That(actualValueFormatter.IncludeTimestamp, Is.False);

            Assert.That(actualIdentifier, Is.EqualTo(expectedIdentifier));
            Assert.That(actualContentType, Is.EqualTo(expectedContentType));
            Assert.That(actualSetCurrent, Is.False);

            Assert.That(standardOut.ToString(), Is.Empty);
            Assert.That(standardError.ToString(), Is.Empty);

            mockConsoleModeMiddleware.Verify(
                mock => mock.ConfigureConsoleMode(It.IsAny<InvocationContext>()),
                Times.Never);
        }

        [Test]
        public async Task MainInternal_GetCommand_InvalidTypeFlag_ReturnsParseError()
        {
            // Arrange
            ClipboardItemIdentifier expectedIdentifier = 0;
            var args = new string[] { "get", expectedIdentifier.ToString(), "--type", "foobar" };
            var expectedExitCode = (int)ErrorCode.ParseError;
            var expectedErrorOutput = $"Invalid type specified. Valid values are: Text,Image,All{Environment.NewLine}{Environment.NewLine}";

            var mockConsoleClipboard = new Mock<IConsoleClipboard>(MockBehavior.Strict);
            var pastCommand = new PastCommand(mockConsoleClipboard.Object);

            var standardOut = new StringBuilder();
            var standardError = new StringBuilder();
            var mockConsole = CreateMockConsole(standardOut, standardError);

            // Act
            var actualExitCode = await Program.MainInternal(args, pastCommand, null, mockConsole.Object);

            // Assert
            Assert.That(actualExitCode, Is.EqualTo(expectedExitCode));

            Assert.That(standardOut.ToString(), Is.EqualTo(GetCommandShortHelp));
            Assert.That(standardError.ToString(), Is.EqualTo(expectedErrorOutput));
        }

        [Test]
        public async Task MainInternal_GetCommand_ValidAnsiResetFlag_ParsesCorrectType([EnumValueSource] AnsiResetType expectedAnsiResetType)
        {
            // Arrange
            ClipboardItemIdentifier expectedIdentifier = 0;
            var args = new string[] { "get", expectedIdentifier.ToString(), "--ansi-reset", expectedAnsiResetType.ToString() };
            var expectedExitCode = (int)ErrorCode.Success;

            var mockConsoleModeMiddleware = new Mock<IConsoleModeMiddleware>();
            var mockConsoleClipboard = new Mock<IConsoleClipboard>(MockBehavior.Strict);

            IConsoleWriter? actualConsoleWriter = null;
            IValueFormatter? actualValueFormatter = null;
            ClipboardItemIdentifier? actualIdentifier = null;
            ContentType? actualContentType = null;
            bool? actualSetCurrent = null;
            mockConsoleClipboard
                .Setup(mock => mock.GetClipboardHistoryItemAsync(
                    It.IsAny<IConsoleWriter>(),
                    It.IsAny<IValueFormatter>(),
                    It.IsAny<ClipboardItemIdentifier>(),
                    It.IsAny<ContentType>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((IConsoleWriter consoleWriter, IValueFormatter valueFormatter, ClipboardItemIdentifier identifier, ContentType contentType, bool setCurrent, CancellationToken _) =>
                {
                    actualConsoleWriter = consoleWriter;
                    actualValueFormatter = valueFormatter;
                    actualIdentifier = identifier;
                    actualContentType = contentType;
                    actualSetCurrent = setCurrent;
                    return expectedExitCode;
                });

            var pastCommand = new PastCommand(mockConsoleClipboard.Object);

            var standardOut = new StringBuilder();
            var standardError = new StringBuilder();
            var mockConsole = CreateMockConsole(standardOut, standardError);

            // Act
            var actualExitCode = await Program.MainInternal(args, pastCommand, mockConsoleModeMiddleware.Object, mockConsole.Object);

            // Assert
            Assert.That(actualExitCode, Is.EqualTo(expectedExitCode));

            Assert.That(actualConsoleWriter, Is.Not.Null);
            Assert.That(actualConsoleWriter.SuppressErrorOutput, Is.False);
            Assert.That(actualConsoleWriter.EnableAnsiProcessing, Is.False);
            Assert.That(actualConsoleWriter.AnsiResetType, Is.EqualTo(expectedAnsiResetType));

            Assert.That(actualValueFormatter, Is.Not.Null);
            Assert.That(actualValueFormatter.NullLineEnding, Is.False);
            Assert.That(actualValueFormatter.IncludeIndex, Is.False);
            Assert.That(actualValueFormatter.IncludeId, Is.False);
            Assert.That(actualValueFormatter.IncludeTimestamp, Is.False);

            Assert.That(actualIdentifier, Is.EqualTo(expectedIdentifier));
            Assert.That(actualContentType, Is.EqualTo(ContentType.Text));
            Assert.That(actualSetCurrent, Is.False);

            Assert.That(standardOut.ToString(), Is.Empty);
            Assert.That(standardError.ToString(), Is.Empty);

            mockConsoleModeMiddleware.Verify(
                mock => mock.ConfigureConsoleMode(It.IsAny<InvocationContext>()),
                Times.Never);
        }

        [Test]
        public async Task MainInternal_GetCommand_InvalidAnsiResetFlag_ReturnsParseError()
        {
            // Arrange
            ClipboardItemIdentifier expectedIdentifier = 0;
            var args = new string[] { "get", expectedIdentifier.ToString(), "--ansi-reset", "foobar" };
            var expectedExitCode = (int)ErrorCode.ParseError;
            var expectedErrorOutput = $"Invalid type specified. Valid values are: Auto,On,Off{Environment.NewLine}{Environment.NewLine}";

            var mockConsoleClipboard = new Mock<IConsoleClipboard>(MockBehavior.Strict);
            var pastCommand = new PastCommand(mockConsoleClipboard.Object);

            var standardOut = new StringBuilder();
            var standardError = new StringBuilder();
            var mockConsole = CreateMockConsole(standardOut, standardError);

            // Act
            var actualExitCode = await Program.MainInternal(args, pastCommand, null, mockConsole.Object);

            // Assert
            Assert.That(actualExitCode, Is.EqualTo(expectedExitCode));

            Assert.That(standardOut.ToString(), Is.EqualTo(GetCommandShortHelp));
            Assert.That(standardError.ToString(), Is.EqualTo(expectedErrorOutput));
        }

        [Test]
        public async Task MainInternal_GetCommand_AllFlag_ParsesToAllContentType()
        {
            // Arrange
            ClipboardItemIdentifier expectedIdentifier = 0;
            var args = new string[] { "get", expectedIdentifier.ToString(), "--all" };
            var expectedContentType = ContentType.All;
            var expectedExitCode = (int)ErrorCode.Success;

            var mockConsoleModeMiddleware = new Mock<IConsoleModeMiddleware>();
            var mockConsoleClipboard = new Mock<IConsoleClipboard>(MockBehavior.Strict);

            IConsoleWriter? actualConsoleWriter = null;
            IValueFormatter? actualValueFormatter = null;
            ClipboardItemIdentifier? actualIdentifier = null;
            ContentType? actualContentType = null;
            bool? actualSetCurrent = null;
            mockConsoleClipboard
                .Setup(mock => mock.GetClipboardHistoryItemAsync(
                    It.IsAny<IConsoleWriter>(),
                    It.IsAny<IValueFormatter>(),
                    It.IsAny<ClipboardItemIdentifier>(),
                    It.IsAny<ContentType>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((IConsoleWriter consoleWriter, IValueFormatter valueFormatter, ClipboardItemIdentifier identifier, ContentType contentType, bool setCurrent, CancellationToken _) =>
                {
                    actualConsoleWriter = consoleWriter;
                    actualValueFormatter = valueFormatter;
                    actualIdentifier = identifier;
                    actualContentType = contentType;
                    actualSetCurrent = setCurrent;
                    return expectedExitCode;
                });

            var pastCommand = new PastCommand(mockConsoleClipboard.Object);

            var standardOut = new StringBuilder();
            var standardError = new StringBuilder();
            var mockConsole = CreateMockConsole(standardOut, standardError);

            // Act
            var actualExitCode = await Program.MainInternal(args, pastCommand, mockConsoleModeMiddleware.Object, mockConsole.Object);

            // Assert
            Assert.That(actualExitCode, Is.EqualTo(expectedExitCode));

            Assert.That(actualConsoleWriter, Is.Not.Null);
            Assert.That(actualConsoleWriter.SuppressErrorOutput, Is.False);
            Assert.That(actualConsoleWriter.EnableAnsiProcessing, Is.False);
            Assert.That(actualConsoleWriter.AnsiResetType, Is.EqualTo(AnsiResetType.Auto));

            Assert.That(actualValueFormatter, Is.Not.Null);
            Assert.That(actualValueFormatter.NullLineEnding, Is.False);
            Assert.That(actualValueFormatter.IncludeIndex, Is.False);
            Assert.That(actualValueFormatter.IncludeId, Is.False);
            Assert.That(actualValueFormatter.IncludeTimestamp, Is.False);

            Assert.That(actualIdentifier, Is.EqualTo(expectedIdentifier));
            Assert.That(actualContentType, Is.EqualTo(expectedContentType));
            Assert.That(actualSetCurrent, Is.False);

            Assert.That(standardOut.ToString(), Is.Empty);
            Assert.That(standardError.ToString(), Is.Empty);

            mockConsoleModeMiddleware.Verify(
                mock => mock.ConfigureConsoleMode(It.IsAny<InvocationContext>()),
                Times.Never);
        }

        [Test]
        public async Task MainInternal_GetCommand_AnsiFlag_EnableAnsiProcessingTrue()
        {
            // Arrange
            ClipboardItemIdentifier expectedIdentifier = 0;
            var args = new string[] { "get", expectedIdentifier.ToString(), "--ansi" };
            var expectedExitCode = (int)ErrorCode.Success;

            var mockConsoleModeMiddleware = new Mock<IConsoleModeMiddleware>();
            var mockConsoleClipboard = new Mock<IConsoleClipboard>(MockBehavior.Strict);

            IConsoleWriter? actualConsoleWriter = null;
            IValueFormatter? actualValueFormatter = null;
            ClipboardItemIdentifier? actualIdentifier = null;
            ContentType? actualContentType = null;
            bool? actualSetCurrent = null;
            mockConsoleClipboard
                .Setup(mock => mock.GetClipboardHistoryItemAsync(
                    It.IsAny<IConsoleWriter>(),
                    It.IsAny<IValueFormatter>(),
                    It.IsAny<ClipboardItemIdentifier>(),
                    It.IsAny<ContentType>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((IConsoleWriter consoleWriter, IValueFormatter valueFormatter, ClipboardItemIdentifier identifier, ContentType contentType, bool setCurrent, CancellationToken _) =>
                {
                    actualConsoleWriter = consoleWriter;
                    actualValueFormatter = valueFormatter;
                    actualIdentifier = identifier;
                    actualContentType = contentType;
                    actualSetCurrent = setCurrent;
                    return expectedExitCode;
                });

            var pastCommand = new PastCommand(mockConsoleClipboard.Object);

            var standardOut = new StringBuilder();
            var standardError = new StringBuilder();
            var mockConsole = CreateMockConsole(standardOut, standardError);

            // Act
            var actualExitCode = await Program.MainInternal(args, pastCommand, mockConsoleModeMiddleware.Object, mockConsole.Object);

            // Assert
            Assert.That(actualExitCode, Is.EqualTo(expectedExitCode));

            Assert.That(actualConsoleWriter, Is.Not.Null);
            Assert.That(actualConsoleWriter.SuppressErrorOutput, Is.False);
            Assert.That(actualConsoleWriter.EnableAnsiProcessing, Is.True);
            Assert.That(actualConsoleWriter.AnsiResetType, Is.EqualTo(AnsiResetType.Auto));

            Assert.That(actualValueFormatter, Is.Not.Null);
            Assert.That(actualValueFormatter.NullLineEnding, Is.False);
            Assert.That(actualValueFormatter.IncludeIndex, Is.False);
            Assert.That(actualValueFormatter.IncludeId, Is.False);
            Assert.That(actualValueFormatter.IncludeTimestamp, Is.False);

            Assert.That(actualIdentifier, Is.EqualTo(expectedIdentifier));
            Assert.That(actualContentType, Is.EqualTo(ContentType.Text));
            Assert.That(actualSetCurrent, Is.False);

            Assert.That(standardOut.ToString(), Is.Empty);
            Assert.That(standardError.ToString(), Is.Empty);

            mockConsoleModeMiddleware.Verify(
                mock => mock.ConfigureConsoleMode(It.IsAny<InvocationContext>()),
                Times.Once);
        }

        [Test]
        public async Task MainInternal_GetCommand_SetCurrentFlag_SetCurrentTrue()
        {
            // Arrange
            ClipboardItemIdentifier expectedIdentifier = 0;
            var args = new string[] { "get", expectedIdentifier.ToString(), "--set-current" };
            var expectedExitCode = (int)ErrorCode.Success;

            var mockConsoleModeMiddleware = new Mock<IConsoleModeMiddleware>();
            var mockConsoleClipboard = new Mock<IConsoleClipboard>(MockBehavior.Strict);

            IConsoleWriter? actualConsoleWriter = null;
            IValueFormatter? actualValueFormatter = null;
            ClipboardItemIdentifier? actualIdentifier = null;
            ContentType? actualContentType = null;
            bool? actualSetCurrent = null;
            mockConsoleClipboard
                .Setup(mock => mock.GetClipboardHistoryItemAsync(
                    It.IsAny<IConsoleWriter>(),
                    It.IsAny<IValueFormatter>(),
                    It.IsAny<ClipboardItemIdentifier>(),
                    It.IsAny<ContentType>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((IConsoleWriter consoleWriter, IValueFormatter valueFormatter, ClipboardItemIdentifier identifier, ContentType contentType, bool setCurrent, CancellationToken _) =>
                {
                    actualConsoleWriter = consoleWriter;
                    actualValueFormatter = valueFormatter;
                    actualIdentifier = identifier;
                    actualContentType = contentType;
                    actualSetCurrent = setCurrent;
                    return expectedExitCode;
                });

            var pastCommand = new PastCommand(mockConsoleClipboard.Object);

            var standardOut = new StringBuilder();
            var standardError = new StringBuilder();
            var mockConsole = CreateMockConsole(standardOut, standardError);

            // Act
            var actualExitCode = await Program.MainInternal(args, pastCommand, mockConsoleModeMiddleware.Object, mockConsole.Object);

            // Assert
            Assert.That(actualExitCode, Is.EqualTo(expectedExitCode));

            Assert.That(actualConsoleWriter, Is.Not.Null);
            Assert.That(actualConsoleWriter.SuppressErrorOutput, Is.False);
            Assert.That(actualConsoleWriter.EnableAnsiProcessing, Is.False);
            Assert.That(actualConsoleWriter.AnsiResetType, Is.EqualTo(AnsiResetType.Auto));

            Assert.That(actualValueFormatter, Is.Not.Null);
            Assert.That(actualValueFormatter.NullLineEnding, Is.False);
            Assert.That(actualValueFormatter.IncludeIndex, Is.False);
            Assert.That(actualValueFormatter.IncludeId, Is.False);
            Assert.That(actualValueFormatter.IncludeTimestamp, Is.False);

            Assert.That(actualIdentifier, Is.EqualTo(expectedIdentifier));
            Assert.That(actualContentType, Is.EqualTo(ContentType.Text));
            Assert.That(actualSetCurrent, Is.True);

            Assert.That(standardOut.ToString(), Is.Empty);
            Assert.That(standardError.ToString(), Is.Empty);

            mockConsoleModeMiddleware.Verify(
                mock => mock.ConfigureConsoleMode(It.IsAny<InvocationContext>()),
                Times.Never);
        }

        [Test]
        [TestCase("--quiet")]
        [TestCase("-q")]
        public async Task MainInternal_GetCommand_QuietFlag_SuppressErrorOutputTrue(string quietFlag)
        {
            // Arrange
            ClipboardItemIdentifier expectedIdentifier = 0;
            var args = new string[] { "get", expectedIdentifier.ToString(), quietFlag };
            var expectedExitCode = (int)ErrorCode.Success;

            var mockConsoleModeMiddleware = new Mock<IConsoleModeMiddleware>();
            var mockConsoleClipboard = new Mock<IConsoleClipboard>(MockBehavior.Strict);

            IConsoleWriter? actualConsoleWriter = null;
            IValueFormatter? actualValueFormatter = null;
            ClipboardItemIdentifier? actualIdentifier = null;
            ContentType? actualContentType = null;
            bool? actualSetCurrent = null;
            mockConsoleClipboard
                .Setup(mock => mock.GetClipboardHistoryItemAsync(
                    It.IsAny<IConsoleWriter>(),
                    It.IsAny<IValueFormatter>(),
                    It.IsAny<ClipboardItemIdentifier>(),
                    It.IsAny<ContentType>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((IConsoleWriter consoleWriter, IValueFormatter valueFormatter, ClipboardItemIdentifier identifier, ContentType contentType, bool setCurrent, CancellationToken _) =>
                {
                    actualConsoleWriter = consoleWriter;
                    actualValueFormatter = valueFormatter;
                    actualIdentifier = identifier;
                    actualContentType = contentType;
                    actualSetCurrent = setCurrent;
                    return expectedExitCode;
                });

            var pastCommand = new PastCommand(mockConsoleClipboard.Object);

            var standardOut = new StringBuilder();
            var standardError = new StringBuilder();
            var mockConsole = CreateMockConsole(standardOut, standardError);

            // Act
            var actualExitCode = await Program.MainInternal(args, pastCommand, mockConsoleModeMiddleware.Object, mockConsole.Object);

            // Assert
            Assert.That(actualExitCode, Is.EqualTo(expectedExitCode));

            Assert.That(actualConsoleWriter, Is.Not.Null);
            Assert.That(actualConsoleWriter.SuppressErrorOutput, Is.True);
            Assert.That(actualConsoleWriter.EnableAnsiProcessing, Is.False);
            Assert.That(actualConsoleWriter.AnsiResetType, Is.EqualTo(AnsiResetType.Auto));

            Assert.That(actualValueFormatter, Is.Not.Null);
            Assert.That(actualValueFormatter.NullLineEnding, Is.False);
            Assert.That(actualValueFormatter.IncludeIndex, Is.False);
            Assert.That(actualValueFormatter.IncludeId, Is.False);
            Assert.That(actualValueFormatter.IncludeTimestamp, Is.False);

            Assert.That(actualIdentifier, Is.EqualTo(expectedIdentifier));
            Assert.That(actualContentType, Is.EqualTo(ContentType.Text));
            Assert.That(actualSetCurrent, Is.False);

            Assert.That(standardOut.ToString(), Is.Empty);
            Assert.That(standardError.ToString(), Is.Empty);

            mockConsoleModeMiddleware.Verify(
                mock => mock.ConfigureConsoleMode(It.IsAny<InvocationContext>()),
                Times.Never);
        }

        [Test]
        [TestCase("-h")]
        [TestCase("-?")]
        public async Task MainInternal_GetCommand_HelpShortFlag_OutputsExpectedHelp(string helpFlag)
        {
            // Arrange
            var args = new string[] { "get", helpFlag };
            var expectedExitCode = (int)ErrorCode.Success;

            var mockConsoleClipboard = new Mock<IConsoleClipboard>(MockBehavior.Strict);
            var pastCommand = new PastCommand(mockConsoleClipboard.Object);

            var standardOut = new StringBuilder();
            var standardError = new StringBuilder();
            var mockConsole = CreateMockConsole(standardOut, standardError);

            // Act
            var actualExitCode = await Program.MainInternal(args, pastCommand, null, mockConsole.Object);

            // Assert
            Assert.That(actualExitCode, Is.EqualTo(expectedExitCode));

            Assert.That(standardOut.ToString(), Is.EqualTo(GetCommandShortHelp));
            Assert.That(standardError.ToString(), Is.Empty);
        }

        [Test]
        public async Task MainInternal_GetCommand_HelpLongFlag_OutputsExpectedHelp()
        {
            // Arrange
            var args = new string[] { "get", "--help" };
            var expectedExitCode = (int)ErrorCode.Success;

            var mockConsoleClipboard = new Mock<IConsoleClipboard>(MockBehavior.Strict);
            var pastCommand = new PastCommand(mockConsoleClipboard.Object);

            var standardOut = new StringBuilder();
            var standardError = new StringBuilder();
            var mockConsole = CreateMockConsole(standardOut, standardError);

            // Act
            var actualExitCode = await Program.MainInternal(args, pastCommand, null, mockConsole.Object);

            // Assert
            Assert.That(actualExitCode, Is.EqualTo(expectedExitCode));

            Assert.That(standardOut.ToString(), Is.EqualTo(GetCommandLongHelp));
            Assert.That(standardError.ToString(), Is.Empty);
        }

        [Test]
        public async Task MainInternal_GetCommand_ThrowsException_ReturnsUnexpectedError()
        {
            // Arrange
            var args = new string[] { "get", "0" };
            var expectedExitCode = (int)ErrorCode.UnexpectedError;
            var expectedException = new Exception("Uh-oh! :O");

            var mockConsoleClipboard = new Mock<IConsoleClipboard>(MockBehavior.Strict);
            mockConsoleClipboard
                .Setup(mock => mock.GetClipboardHistoryItemAsync(
                    It.IsAny<IConsoleWriter>(),
                    It.IsAny<IValueFormatter>(),
                    It.IsAny<ClipboardItemIdentifier>(),
                    It.IsAny<ContentType>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()))
                .Throws(expectedException);

            var pastCommand = new PastCommand(mockConsoleClipboard.Object);

            var standardOut = new StringBuilder();
            var standardError = new StringBuilder();
            var mockConsole = CreateMockConsole(standardOut, standardError);

            // Act
            var actualExitCode = await Program.MainInternal(args, pastCommand, null, mockConsole.Object);

            // Assert
            Assert.That(actualExitCode, Is.EqualTo(expectedExitCode));

            Assert.That(standardOut.ToString(), Is.Empty);
            Assert.That(standardError.ToString(), Is.EqualTo($"Unhandled exception: {expectedException}{Environment.NewLine}"));
        }
        #endregion Get Command

        #region List Command
        [Test]
        public async Task MainInternal_ListCommand_NoOptions_ParsesWithDefaultValues()
        {
            // Arrange
            var args = new string[] { "list" };
            var expectedExitCode = (int)ErrorCode.Success;

            var mockConsoleModeMiddleware = new Mock<IConsoleModeMiddleware>();
            var mockConsoleClipboard = new Mock<IConsoleClipboard>(MockBehavior.Strict);

            IConsoleWriter? actualConsoleWriter = null;
            IValueFormatter? actualValueFormatter = null;
            ContentType? actualContentType = null;
            bool? actualPinned = null;
            mockConsoleClipboard
                .Setup(mock => mock.ListClipboardHistoryAsync(
                    It.IsAny<IConsoleWriter>(),
                    It.IsAny<IValueFormatter>(),
                    It.IsAny<ContentType>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((IConsoleWriter consoleWriter, IValueFormatter valueFormatter, ContentType contentType, bool setPinned, CancellationToken _) =>
                {
                    actualConsoleWriter = consoleWriter;
                    actualValueFormatter = valueFormatter;
                    actualContentType = contentType;
                    actualPinned = setPinned;
                    return expectedExitCode;
                });

            var pastCommand = new PastCommand(mockConsoleClipboard.Object);

            var standardOut = new StringBuilder();
            var standardError = new StringBuilder();
            var mockConsole = CreateMockConsole(standardOut, standardError);

            // Act
            var actualExitCode = await Program.MainInternal(args, pastCommand, mockConsoleModeMiddleware.Object, mockConsole.Object);

            // Assert
            Assert.That(actualExitCode, Is.EqualTo(expectedExitCode));

            Assert.That(actualConsoleWriter, Is.Not.Null);
            Assert.That(actualConsoleWriter.SuppressErrorOutput, Is.False);
            Assert.That(actualConsoleWriter.EnableAnsiProcessing, Is.False);
            Assert.That(actualConsoleWriter.AnsiResetType, Is.EqualTo(AnsiResetType.Auto));

            Assert.That(actualValueFormatter, Is.Not.Null);
            Assert.That(actualValueFormatter.NullLineEnding, Is.False);
            Assert.That(actualValueFormatter.IncludeIndex, Is.False);
            Assert.That(actualValueFormatter.IncludeId, Is.False);
            Assert.That(actualValueFormatter.IncludeTimestamp, Is.False);

            Assert.That(actualContentType, Is.EqualTo(ContentType.Text));
            Assert.That(actualPinned, Is.False);

            Assert.That(standardOut.ToString(), Is.Empty);
            Assert.That(standardError.ToString(), Is.Empty);

            mockConsoleModeMiddleware.Verify(
                mock => mock.ConfigureConsoleMode(It.IsAny<InvocationContext>()),
                Times.Never);
        }

        [Test]
        public async Task MainInternal_ListCommand_ValidTypeFlag_ParsesCorrectType([EnumValueSource] ContentType expectedContentType)
        {
            // Arrange
            var args = new string[] { "list", "--type", expectedContentType.ToString() };
            var expectedExitCode = (int)ErrorCode.Success;

            var mockConsoleModeMiddleware = new Mock<IConsoleModeMiddleware>();
            var mockConsoleClipboard = new Mock<IConsoleClipboard>(MockBehavior.Strict);

            IConsoleWriter? actualConsoleWriter = null;
            IValueFormatter? actualValueFormatter = null;
            ContentType? actualContentType = null;
            bool? actualPinned = null;
            mockConsoleClipboard
                .Setup(mock => mock.ListClipboardHistoryAsync(
                    It.IsAny<IConsoleWriter>(),
                    It.IsAny<IValueFormatter>(),
                    It.IsAny<ContentType>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((IConsoleWriter consoleWriter, IValueFormatter valueFormatter, ContentType contentType, bool setPinned, CancellationToken _) =>
                {
                    actualConsoleWriter = consoleWriter;
                    actualValueFormatter = valueFormatter;
                    actualContentType = contentType;
                    actualPinned = setPinned;
                    return expectedExitCode;
                });

            var pastCommand = new PastCommand(mockConsoleClipboard.Object);

            var standardOut = new StringBuilder();
            var standardError = new StringBuilder();
            var mockConsole = CreateMockConsole(standardOut, standardError);

            // Act
            var actualExitCode = await Program.MainInternal(args, pastCommand, mockConsoleModeMiddleware.Object, mockConsole.Object);

            // Assert
            Assert.That(actualExitCode, Is.EqualTo(expectedExitCode));

            Assert.That(actualConsoleWriter, Is.Not.Null);
            Assert.That(actualConsoleWriter.SuppressErrorOutput, Is.False);
            Assert.That(actualConsoleWriter.EnableAnsiProcessing, Is.False);
            Assert.That(actualConsoleWriter.AnsiResetType, Is.EqualTo(AnsiResetType.Auto));

            Assert.That(actualValueFormatter, Is.Not.Null);
            Assert.That(actualValueFormatter.NullLineEnding, Is.False);
            Assert.That(actualValueFormatter.IncludeIndex, Is.False);
            Assert.That(actualValueFormatter.IncludeId, Is.False);
            Assert.That(actualValueFormatter.IncludeTimestamp, Is.False);

            Assert.That(actualContentType, Is.EqualTo(expectedContentType));
            Assert.That(actualPinned, Is.False);

            Assert.That(standardOut.ToString(), Is.Empty);
            Assert.That(standardError.ToString(), Is.Empty);

            mockConsoleModeMiddleware.Verify(
                mock => mock.ConfigureConsoleMode(It.IsAny<InvocationContext>()),
                Times.Never);
        }

        [Test]
        public async Task MainInternal_ListCommand_InvalidTypeFlag_ReturnsParseError()
        {
            // Arrange
            var args = new string[] { "list", "--type", "foobar" };
            var expectedExitCode = (int)ErrorCode.ParseError;
            var expectedErrorOutput = $"Invalid type specified. Valid values are: Text,Image,All{Environment.NewLine}{Environment.NewLine}";

            var mockConsoleClipboard = new Mock<IConsoleClipboard>(MockBehavior.Strict);
            var pastCommand = new PastCommand(mockConsoleClipboard.Object);

            var standardOut = new StringBuilder();
            var standardError = new StringBuilder();
            var mockConsole = CreateMockConsole(standardOut, standardError);

            // Act
            var actualExitCode = await Program.MainInternal(args, pastCommand, null, mockConsole.Object);

            // Assert
            Assert.That(actualExitCode, Is.EqualTo(expectedExitCode));

            Assert.That(standardOut.ToString(), Is.EqualTo(ListCommandShortHelp));
            Assert.That(standardError.ToString(), Is.EqualTo(expectedErrorOutput));
        }

        [Test]
        public async Task MainInternal_ListCommand_ValidAnsiResetFlag_ParsesCorrectType([EnumValueSource] AnsiResetType expectedAnsiResetType)
        {
            // Arrange
            var args = new string[] { "list", "--ansi-reset", expectedAnsiResetType.ToString() };
            var expectedExitCode = (int)ErrorCode.Success;

            var mockConsoleModeMiddleware = new Mock<IConsoleModeMiddleware>();
            var mockConsoleClipboard = new Mock<IConsoleClipboard>(MockBehavior.Strict);

            IConsoleWriter? actualConsoleWriter = null;
            IValueFormatter? actualValueFormatter = null;
            ContentType? actualContentType = null;
            bool? actualPinned = null;
            mockConsoleClipboard
                .Setup(mock => mock.ListClipboardHistoryAsync(
                    It.IsAny<IConsoleWriter>(),
                    It.IsAny<IValueFormatter>(),
                    It.IsAny<ContentType>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((IConsoleWriter consoleWriter, IValueFormatter valueFormatter, ContentType contentType, bool setPinned, CancellationToken _) =>
                {
                    actualConsoleWriter = consoleWriter;
                    actualValueFormatter = valueFormatter;
                    actualContentType = contentType;
                    actualPinned = setPinned;
                    return expectedExitCode;
                });

            var pastCommand = new PastCommand(mockConsoleClipboard.Object);

            var standardOut = new StringBuilder();
            var standardError = new StringBuilder();
            var mockConsole = CreateMockConsole(standardOut, standardError);

            // Act
            var actualExitCode = await Program.MainInternal(args, pastCommand, mockConsoleModeMiddleware.Object, mockConsole.Object);

            // Assert
            Assert.That(actualExitCode, Is.EqualTo(expectedExitCode));

            Assert.That(actualConsoleWriter, Is.Not.Null);
            Assert.That(actualConsoleWriter.SuppressErrorOutput, Is.False);
            Assert.That(actualConsoleWriter.EnableAnsiProcessing, Is.False);
            Assert.That(actualConsoleWriter.AnsiResetType, Is.EqualTo(expectedAnsiResetType));

            Assert.That(actualValueFormatter, Is.Not.Null);
            Assert.That(actualValueFormatter.NullLineEnding, Is.False);
            Assert.That(actualValueFormatter.IncludeIndex, Is.False);
            Assert.That(actualValueFormatter.IncludeId, Is.False);
            Assert.That(actualValueFormatter.IncludeTimestamp, Is.False);

            Assert.That(actualContentType, Is.EqualTo(ContentType.Text));
            Assert.That(actualPinned, Is.False);

            Assert.That(standardOut.ToString(), Is.Empty);
            Assert.That(standardError.ToString(), Is.Empty);

            mockConsoleModeMiddleware.Verify(
                mock => mock.ConfigureConsoleMode(It.IsAny<InvocationContext>()),
                Times.Never);
        }

        [Test]
        public async Task MainInternal_ListCommand_InvalidAnsiResetFlag_ReturnsParseError()
        {
            // Arrange
            var args = new string[] { "list", "--ansi-reset", "foobar" };
            var expectedExitCode = (int)ErrorCode.ParseError;
            var expectedErrorOutput = $"Invalid type specified. Valid values are: Auto,On,Off{Environment.NewLine}{Environment.NewLine}";

            var mockConsoleClipboard = new Mock<IConsoleClipboard>(MockBehavior.Strict);
            var pastCommand = new PastCommand(mockConsoleClipboard.Object);

            var standardOut = new StringBuilder();
            var standardError = new StringBuilder();
            var mockConsole = CreateMockConsole(standardOut, standardError);

            // Act
            var actualExitCode = await Program.MainInternal(args, pastCommand, null, mockConsole.Object);

            // Assert
            Assert.That(actualExitCode, Is.EqualTo(expectedExitCode));

            Assert.That(standardOut.ToString(), Is.EqualTo(ListCommandShortHelp));
            Assert.That(standardError.ToString(), Is.EqualTo(expectedErrorOutput));
        }

        [Test]
        public async Task MainInternal_ListCommand_AllFlag_ParsesToAllContentType()
        {
            // Arrange
            var args = new string[] { "list", "--all" };
            var expectedContentType = ContentType.All;
            var expectedExitCode = (int)ErrorCode.Success;

            var mockConsoleModeMiddleware = new Mock<IConsoleModeMiddleware>();
            var mockConsoleClipboard = new Mock<IConsoleClipboard>(MockBehavior.Strict);

            IConsoleWriter? actualConsoleWriter = null;
            IValueFormatter? actualValueFormatter = null;
            ContentType? actualContentType = null;
            bool? actualPinned = null;
            mockConsoleClipboard
                .Setup(mock => mock.ListClipboardHistoryAsync(
                    It.IsAny<IConsoleWriter>(),
                    It.IsAny<IValueFormatter>(),
                    It.IsAny<ContentType>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((IConsoleWriter consoleWriter, IValueFormatter valueFormatter, ContentType contentType, bool setPinned, CancellationToken _) =>
                {
                    actualConsoleWriter = consoleWriter;
                    actualValueFormatter = valueFormatter;
                    actualContentType = contentType;
                    actualPinned = setPinned;
                    return expectedExitCode;
                });

            var pastCommand = new PastCommand(mockConsoleClipboard.Object);

            var standardOut = new StringBuilder();
            var standardError = new StringBuilder();
            var mockConsole = CreateMockConsole(standardOut, standardError);

            // Act
            var actualExitCode = await Program.MainInternal(args, pastCommand, mockConsoleModeMiddleware.Object, mockConsole.Object);

            // Assert
            Assert.That(actualExitCode, Is.EqualTo(expectedExitCode));

            Assert.That(actualConsoleWriter, Is.Not.Null);
            Assert.That(actualConsoleWriter.SuppressErrorOutput, Is.False);
            Assert.That(actualConsoleWriter.EnableAnsiProcessing, Is.False);
            Assert.That(actualConsoleWriter.AnsiResetType, Is.EqualTo(AnsiResetType.Auto));

            Assert.That(actualValueFormatter, Is.Not.Null);
            Assert.That(actualValueFormatter.NullLineEnding, Is.False);
            Assert.That(actualValueFormatter.IncludeIndex, Is.False);
            Assert.That(actualValueFormatter.IncludeId, Is.False);
            Assert.That(actualValueFormatter.IncludeTimestamp, Is.False);

            Assert.That(actualContentType, Is.EqualTo(expectedContentType));
            Assert.That(actualPinned, Is.False);

            Assert.That(standardOut.ToString(), Is.Empty);
            Assert.That(standardError.ToString(), Is.Empty);

            mockConsoleModeMiddleware.Verify(
                mock => mock.ConfigureConsoleMode(It.IsAny<InvocationContext>()),
                Times.Never);
        }

        [Test]
        public async Task MainInternal_ListCommand_AnsiFlag_EnableAnsiProcessingTrue()
        {
            // Arrange
            var args = new string[] { "list", "--ansi" };
            var expectedExitCode = (int)ErrorCode.Success;

            var mockConsoleModeMiddleware = new Mock<IConsoleModeMiddleware>();
            var mockConsoleClipboard = new Mock<IConsoleClipboard>(MockBehavior.Strict);

            IConsoleWriter? actualConsoleWriter = null;
            IValueFormatter? actualValueFormatter = null;
            ContentType? actualContentType = null;
            bool? actualPinned = null;
            mockConsoleClipboard
                .Setup(mock => mock.ListClipboardHistoryAsync(
                    It.IsAny<IConsoleWriter>(),
                    It.IsAny<IValueFormatter>(),
                    It.IsAny<ContentType>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((IConsoleWriter consoleWriter, IValueFormatter valueFormatter, ContentType contentType, bool setPinned, CancellationToken _) =>
                {
                    actualConsoleWriter = consoleWriter;
                    actualValueFormatter = valueFormatter;
                    actualContentType = contentType;
                    actualPinned = setPinned;
                    return expectedExitCode;
                });

            var pastCommand = new PastCommand(mockConsoleClipboard.Object);

            var standardOut = new StringBuilder();
            var standardError = new StringBuilder();
            var mockConsole = CreateMockConsole(standardOut, standardError);

            // Act
            var actualExitCode = await Program.MainInternal(args, pastCommand, mockConsoleModeMiddleware.Object, mockConsole.Object);

            // Assert
            Assert.That(actualExitCode, Is.EqualTo(expectedExitCode));

            Assert.That(actualConsoleWriter, Is.Not.Null);
            Assert.That(actualConsoleWriter.SuppressErrorOutput, Is.False);
            Assert.That(actualConsoleWriter.EnableAnsiProcessing, Is.True);
            Assert.That(actualConsoleWriter.AnsiResetType, Is.EqualTo(AnsiResetType.Auto));

            Assert.That(actualValueFormatter, Is.Not.Null);
            Assert.That(actualValueFormatter.NullLineEnding, Is.False);
            Assert.That(actualValueFormatter.IncludeIndex, Is.False);
            Assert.That(actualValueFormatter.IncludeId, Is.False);
            Assert.That(actualValueFormatter.IncludeTimestamp, Is.False);

            Assert.That(actualContentType, Is.EqualTo(ContentType.Text));
            Assert.That(actualPinned, Is.False);

            Assert.That(standardOut.ToString(), Is.Empty);
            Assert.That(standardError.ToString(), Is.Empty);

            mockConsoleModeMiddleware.Verify(
                mock => mock.ConfigureConsoleMode(It.IsAny<InvocationContext>()),
                Times.Once);
        }

        [Test]
        public async Task MainInternal_ListCommand_PinnedFlag_PinnedTrue()
        {
            // Arrange
            var args = new string[] { "list", "--pinned" };
            var expectedExitCode = (int)ErrorCode.Success;

            var mockConsoleModeMiddleware = new Mock<IConsoleModeMiddleware>();
            var mockConsoleClipboard = new Mock<IConsoleClipboard>(MockBehavior.Strict);

            IConsoleWriter? actualConsoleWriter = null;
            IValueFormatter? actualValueFormatter = null;
            ContentType? actualContentType = null;
            bool? actualPinned = null;
            mockConsoleClipboard
                .Setup(mock => mock.ListClipboardHistoryAsync(
                    It.IsAny<IConsoleWriter>(),
                    It.IsAny<IValueFormatter>(),
                    It.IsAny<ContentType>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((IConsoleWriter consoleWriter, IValueFormatter valueFormatter, ContentType contentType, bool setPinned, CancellationToken _) =>
                {
                    actualConsoleWriter = consoleWriter;
                    actualValueFormatter = valueFormatter;
                    actualContentType = contentType;
                    actualPinned = setPinned;
                    return expectedExitCode;
                });

            var pastCommand = new PastCommand(mockConsoleClipboard.Object);

            var standardOut = new StringBuilder();
            var standardError = new StringBuilder();
            var mockConsole = CreateMockConsole(standardOut, standardError);

            // Act
            var actualExitCode = await Program.MainInternal(args, pastCommand, mockConsoleModeMiddleware.Object, mockConsole.Object);

            // Assert
            Assert.That(actualExitCode, Is.EqualTo(expectedExitCode));

            Assert.That(actualConsoleWriter, Is.Not.Null);
            Assert.That(actualConsoleWriter.SuppressErrorOutput, Is.False);
            Assert.That(actualConsoleWriter.EnableAnsiProcessing, Is.False);
            Assert.That(actualConsoleWriter.AnsiResetType, Is.EqualTo(AnsiResetType.Auto));

            Assert.That(actualValueFormatter, Is.Not.Null);
            Assert.That(actualValueFormatter.NullLineEnding, Is.False);
            Assert.That(actualValueFormatter.IncludeIndex, Is.False);
            Assert.That(actualValueFormatter.IncludeId, Is.False);
            Assert.That(actualValueFormatter.IncludeTimestamp, Is.False);

            Assert.That(actualContentType, Is.EqualTo(ContentType.Text));
            Assert.That(actualPinned, Is.True);

            Assert.That(standardOut.ToString(), Is.Empty);
            Assert.That(standardError.ToString(), Is.Empty);

            mockConsoleModeMiddleware.Verify(
                mock => mock.ConfigureConsoleMode(It.IsAny<InvocationContext>()),
                Times.Never);
        }

        [Test]
        public async Task MainInternal_ListCommand_NullFlag_NullLineEndingTrue()
        {
            // Arrange
            var args = new string[] { "list", "--null" };
            var expectedExitCode = (int)ErrorCode.Success;

            var mockConsoleModeMiddleware = new Mock<IConsoleModeMiddleware>();
            var mockConsoleClipboard = new Mock<IConsoleClipboard>(MockBehavior.Strict);

            IConsoleWriter? actualConsoleWriter = null;
            IValueFormatter? actualValueFormatter = null;
            ContentType? actualContentType = null;
            bool? actualPinned = null;
            mockConsoleClipboard
                .Setup(mock => mock.ListClipboardHistoryAsync(
                    It.IsAny<IConsoleWriter>(),
                    It.IsAny<IValueFormatter>(),
                    It.IsAny<ContentType>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((IConsoleWriter consoleWriter, IValueFormatter valueFormatter, ContentType contentType, bool setPinned, CancellationToken _) =>
                {
                    actualConsoleWriter = consoleWriter;
                    actualValueFormatter = valueFormatter;
                    actualContentType = contentType;
                    actualPinned = setPinned;
                    return expectedExitCode;
                });

            var pastCommand = new PastCommand(mockConsoleClipboard.Object);

            var standardOut = new StringBuilder();
            var standardError = new StringBuilder();
            var mockConsole = CreateMockConsole(standardOut, standardError);

            // Act
            var actualExitCode = await Program.MainInternal(args, pastCommand, mockConsoleModeMiddleware.Object, mockConsole.Object);

            // Assert
            Assert.That(actualExitCode, Is.EqualTo(expectedExitCode));

            Assert.That(actualConsoleWriter, Is.Not.Null);
            Assert.That(actualConsoleWriter.SuppressErrorOutput, Is.False);
            Assert.That(actualConsoleWriter.EnableAnsiProcessing, Is.False);
            Assert.That(actualConsoleWriter.AnsiResetType, Is.EqualTo(AnsiResetType.Auto));

            Assert.That(actualValueFormatter, Is.Not.Null);
            Assert.That(actualValueFormatter.NullLineEnding, Is.True);
            Assert.That(actualValueFormatter.IncludeIndex, Is.False);
            Assert.That(actualValueFormatter.IncludeId, Is.False);
            Assert.That(actualValueFormatter.IncludeTimestamp, Is.False);

            Assert.That(actualContentType, Is.EqualTo(ContentType.Text));
            Assert.That(actualPinned, Is.False);

            Assert.That(standardOut.ToString(), Is.Empty);
            Assert.That(standardError.ToString(), Is.Empty);

            mockConsoleModeMiddleware.Verify(
                mock => mock.ConfigureConsoleMode(It.IsAny<InvocationContext>()),
                Times.Never);
        }

        [Test]
        public async Task MainInternal_ListCommand_IndexFlag_IncludeIndexTrue()
        {
            // Arrange
            var args = new string[] { "list", "--index" };
            var expectedExitCode = (int)ErrorCode.Success;

            var mockConsoleModeMiddleware = new Mock<IConsoleModeMiddleware>();
            var mockConsoleClipboard = new Mock<IConsoleClipboard>(MockBehavior.Strict);

            IConsoleWriter? actualConsoleWriter = null;
            IValueFormatter? actualValueFormatter = null;
            ContentType? actualContentType = null;
            bool? actualPinned = null;
            mockConsoleClipboard
                .Setup(mock => mock.ListClipboardHistoryAsync(
                    It.IsAny<IConsoleWriter>(),
                    It.IsAny<IValueFormatter>(),
                    It.IsAny<ContentType>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((IConsoleWriter consoleWriter, IValueFormatter valueFormatter, ContentType contentType, bool setPinned, CancellationToken _) =>
                {
                    actualConsoleWriter = consoleWriter;
                    actualValueFormatter = valueFormatter;
                    actualContentType = contentType;
                    actualPinned = setPinned;
                    return expectedExitCode;
                });

            var pastCommand = new PastCommand(mockConsoleClipboard.Object);

            var standardOut = new StringBuilder();
            var standardError = new StringBuilder();
            var mockConsole = CreateMockConsole(standardOut, standardError);

            // Act
            var actualExitCode = await Program.MainInternal(args, pastCommand, mockConsoleModeMiddleware.Object, mockConsole.Object);

            // Assert
            Assert.That(actualExitCode, Is.EqualTo(expectedExitCode));

            Assert.That(actualConsoleWriter, Is.Not.Null);
            Assert.That(actualConsoleWriter.SuppressErrorOutput, Is.False);
            Assert.That(actualConsoleWriter.EnableAnsiProcessing, Is.False);
            Assert.That(actualConsoleWriter.AnsiResetType, Is.EqualTo(AnsiResetType.Auto));

            Assert.That(actualValueFormatter, Is.Not.Null);
            Assert.That(actualValueFormatter.NullLineEnding, Is.False);
            Assert.That(actualValueFormatter.IncludeIndex, Is.True);
            Assert.That(actualValueFormatter.IncludeId, Is.False);
            Assert.That(actualValueFormatter.IncludeTimestamp, Is.False);

            Assert.That(actualContentType, Is.EqualTo(ContentType.Text));
            Assert.That(actualPinned, Is.False);

            Assert.That(standardOut.ToString(), Is.Empty);
            Assert.That(standardError.ToString(), Is.Empty);

            mockConsoleModeMiddleware.Verify(
                mock => mock.ConfigureConsoleMode(It.IsAny<InvocationContext>()),
                Times.Never);
        }

        [Test]
        public async Task MainInternal_ListCommand_IdFlag_IncludeIdTrue()
        {
            // Arrange
            var args = new string[] { "list", "--id" };
            var expectedExitCode = (int)ErrorCode.Success;

            var mockConsoleModeMiddleware = new Mock<IConsoleModeMiddleware>();
            var mockConsoleClipboard = new Mock<IConsoleClipboard>(MockBehavior.Strict);

            IConsoleWriter? actualConsoleWriter = null;
            IValueFormatter? actualValueFormatter = null;
            ContentType? actualContentType = null;
            bool? actualPinned = null;
            mockConsoleClipboard
                .Setup(mock => mock.ListClipboardHistoryAsync(
                    It.IsAny<IConsoleWriter>(),
                    It.IsAny<IValueFormatter>(),
                    It.IsAny<ContentType>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((IConsoleWriter consoleWriter, IValueFormatter valueFormatter, ContentType contentType, bool setPinned, CancellationToken _) =>
                {
                    actualConsoleWriter = consoleWriter;
                    actualValueFormatter = valueFormatter;
                    actualContentType = contentType;
                    actualPinned = setPinned;
                    return expectedExitCode;
                });

            var pastCommand = new PastCommand(mockConsoleClipboard.Object);

            var standardOut = new StringBuilder();
            var standardError = new StringBuilder();
            var mockConsole = CreateMockConsole(standardOut, standardError);

            // Act
            var actualExitCode = await Program.MainInternal(args, pastCommand, mockConsoleModeMiddleware.Object, mockConsole.Object);

            // Assert
            Assert.That(actualExitCode, Is.EqualTo(expectedExitCode));

            Assert.That(actualConsoleWriter, Is.Not.Null);
            Assert.That(actualConsoleWriter.SuppressErrorOutput, Is.False);
            Assert.That(actualConsoleWriter.EnableAnsiProcessing, Is.False);
            Assert.That(actualConsoleWriter.AnsiResetType, Is.EqualTo(AnsiResetType.Auto));

            Assert.That(actualValueFormatter, Is.Not.Null);
            Assert.That(actualValueFormatter.NullLineEnding, Is.False);
            Assert.That(actualValueFormatter.IncludeIndex, Is.False);
            Assert.That(actualValueFormatter.IncludeId, Is.True);
            Assert.That(actualValueFormatter.IncludeTimestamp, Is.False);

            Assert.That(actualContentType, Is.EqualTo(ContentType.Text));
            Assert.That(actualPinned, Is.False);

            Assert.That(standardOut.ToString(), Is.Empty);
            Assert.That(standardError.ToString(), Is.Empty);

            mockConsoleModeMiddleware.Verify(
                mock => mock.ConfigureConsoleMode(It.IsAny<InvocationContext>()),
                Times.Never);
        }

        [Test]
        public async Task MainInternal_ListCommand_TimeFlag_IncludeTimestampTrue()
        {
            // Arrange
            var args = new string[] { "list", "--time" };
            var expectedExitCode = (int)ErrorCode.Success;

            var mockConsoleModeMiddleware = new Mock<IConsoleModeMiddleware>();
            var mockConsoleClipboard = new Mock<IConsoleClipboard>(MockBehavior.Strict);

            IConsoleWriter? actualConsoleWriter = null;
            IValueFormatter? actualValueFormatter = null;
            ContentType? actualContentType = null;
            bool? actualPinned = null;
            mockConsoleClipboard
                .Setup(mock => mock.ListClipboardHistoryAsync(
                    It.IsAny<IConsoleWriter>(),
                    It.IsAny<IValueFormatter>(),
                    It.IsAny<ContentType>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((IConsoleWriter consoleWriter, IValueFormatter valueFormatter, ContentType contentType, bool setPinned, CancellationToken _) =>
                {
                    actualConsoleWriter = consoleWriter;
                    actualValueFormatter = valueFormatter;
                    actualContentType = contentType;
                    actualPinned = setPinned;
                    return expectedExitCode;
                });

            var pastCommand = new PastCommand(mockConsoleClipboard.Object);

            var standardOut = new StringBuilder();
            var standardError = new StringBuilder();
            var mockConsole = CreateMockConsole(standardOut, standardError);

            // Act
            var actualExitCode = await Program.MainInternal(args, pastCommand, mockConsoleModeMiddleware.Object, mockConsole.Object);

            // Assert
            Assert.That(actualExitCode, Is.EqualTo(expectedExitCode));

            Assert.That(actualConsoleWriter, Is.Not.Null);
            Assert.That(actualConsoleWriter.SuppressErrorOutput, Is.False);
            Assert.That(actualConsoleWriter.EnableAnsiProcessing, Is.False);
            Assert.That(actualConsoleWriter.AnsiResetType, Is.EqualTo(AnsiResetType.Auto));

            Assert.That(actualValueFormatter, Is.Not.Null);
            Assert.That(actualValueFormatter.NullLineEnding, Is.False);
            Assert.That(actualValueFormatter.IncludeIndex, Is.False);
            Assert.That(actualValueFormatter.IncludeId, Is.False);
            Assert.That(actualValueFormatter.IncludeTimestamp, Is.True);

            Assert.That(actualContentType, Is.EqualTo(ContentType.Text));
            Assert.That(actualPinned, Is.False);

            Assert.That(standardOut.ToString(), Is.Empty);
            Assert.That(standardError.ToString(), Is.Empty);

            mockConsoleModeMiddleware.Verify(
                mock => mock.ConfigureConsoleMode(It.IsAny<InvocationContext>()),
                Times.Never);
        }

        [Test]
        [TestCase("--quiet")]
        [TestCase("-q")]
        public async Task MainInternal_ListCommand_QuietFlag_SuppressErrorOutputTrue(string quietFlag)
        {
            // Arrange
            var args = new string[] { "list", quietFlag };
            var expectedExitCode = (int)ErrorCode.Success;

            var mockConsoleModeMiddleware = new Mock<IConsoleModeMiddleware>();
            var mockConsoleClipboard = new Mock<IConsoleClipboard>(MockBehavior.Strict);

            IConsoleWriter? actualConsoleWriter = null;
            IValueFormatter? actualValueFormatter = null;
            ContentType? actualContentType = null;
            bool? actualPinned = null;
            mockConsoleClipboard
                .Setup(mock => mock.ListClipboardHistoryAsync(
                    It.IsAny<IConsoleWriter>(),
                    It.IsAny<IValueFormatter>(),
                    It.IsAny<ContentType>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync((IConsoleWriter consoleWriter, IValueFormatter valueFormatter, ContentType contentType, bool setPinned, CancellationToken _) =>
                {
                    actualConsoleWriter = consoleWriter;
                    actualValueFormatter = valueFormatter;
                    actualContentType = contentType;
                    actualPinned = setPinned;
                    return expectedExitCode;
                });

            var pastCommand = new PastCommand(mockConsoleClipboard.Object);

            var standardOut = new StringBuilder();
            var standardError = new StringBuilder();
            var mockConsole = CreateMockConsole(standardOut, standardError);

            // Act
            var actualExitCode = await Program.MainInternal(args, pastCommand, mockConsoleModeMiddleware.Object, mockConsole.Object);

            // Assert
            Assert.That(actualExitCode, Is.EqualTo(expectedExitCode));

            Assert.That(actualConsoleWriter, Is.Not.Null);
            Assert.That(actualConsoleWriter.SuppressErrorOutput, Is.True);
            Assert.That(actualConsoleWriter.EnableAnsiProcessing, Is.False);
            Assert.That(actualConsoleWriter.AnsiResetType, Is.EqualTo(AnsiResetType.Auto));

            Assert.That(actualValueFormatter, Is.Not.Null);
            Assert.That(actualValueFormatter.NullLineEnding, Is.False);
            Assert.That(actualValueFormatter.IncludeIndex, Is.False);
            Assert.That(actualValueFormatter.IncludeId, Is.False);
            Assert.That(actualValueFormatter.IncludeTimestamp, Is.False);

            Assert.That(actualContentType, Is.EqualTo(ContentType.Text));
            Assert.That(actualPinned, Is.False);

            Assert.That(standardOut.ToString(), Is.Empty);
            Assert.That(standardError.ToString(), Is.Empty);

            mockConsoleModeMiddleware.Verify(
                mock => mock.ConfigureConsoleMode(It.IsAny<InvocationContext>()),
                Times.Never);
        }

        [Test]
        [TestCase("-h")]
        [TestCase("-?")]
        public async Task MainInternal_ListCommand_HelpShortFlag_OutputsExpectedHelp(string helpFlag)
        {
            // Arrange
            var args = new string[] { "list", helpFlag };
            var expectedExitCode = (int)ErrorCode.Success;

            var mockConsoleClipboard = new Mock<IConsoleClipboard>(MockBehavior.Strict);
            var pastCommand = new PastCommand(mockConsoleClipboard.Object);

            var standardOut = new StringBuilder();
            var standardError = new StringBuilder();
            var mockConsole = CreateMockConsole(standardOut, standardError);

            // Act
            var actualExitCode = await Program.MainInternal(args, pastCommand, null, mockConsole.Object);

            // Assert
            Assert.That(actualExitCode, Is.EqualTo(expectedExitCode));

            Assert.That(standardOut.ToString(), Is.EqualTo(ListCommandShortHelp));
            Assert.That(standardError.ToString(), Is.Empty);
        }

        [Test]
        public async Task MainInternal_ListCommand_HelpLongFlag_OutputsExpectedHelp()
        {
            // Arrange
            var args = new string[] { "list", "--help" };
            var expectedExitCode = (int)ErrorCode.Success;

            var mockConsoleClipboard = new Mock<IConsoleClipboard>(MockBehavior.Strict);
            var pastCommand = new PastCommand(mockConsoleClipboard.Object);

            var standardOut = new StringBuilder();
            var standardError = new StringBuilder();
            var mockConsole = CreateMockConsole(standardOut, standardError);

            // Act
            var actualExitCode = await Program.MainInternal(args, pastCommand, null, mockConsole.Object);

            // Assert
            Assert.That(actualExitCode, Is.EqualTo(expectedExitCode));

            Assert.That(standardOut.ToString(), Is.EqualTo(ListCommandLongHelp));
            Assert.That(standardError.ToString(), Is.Empty);
        }

        [Test]
        public async Task MainInternal_ListCommand_ThrowsException_ReturnsUnexpectedError()
        {
            // Arrange
            var args = new string[] { "list" };
            var expectedExitCode = (int)ErrorCode.UnexpectedError;
            var expectedException = new Exception("Uh-oh! :O");

            var mockConsoleClipboard = new Mock<IConsoleClipboard>(MockBehavior.Strict);
            mockConsoleClipboard
                .Setup(mock => mock.ListClipboardHistoryAsync(
                    It.IsAny<IConsoleWriter>(),
                    It.IsAny<IValueFormatter>(),
                    It.IsAny<ContentType>(),
                    It.IsAny<bool>(),
                    It.IsAny<CancellationToken>()))
                .Throws(expectedException);

            var pastCommand = new PastCommand(mockConsoleClipboard.Object);

            var standardOut = new StringBuilder();
            var standardError = new StringBuilder();
            var mockConsole = CreateMockConsole(standardOut, standardError);

            // Act
            var actualExitCode = await Program.MainInternal(args, pastCommand, null, mockConsole.Object);

            // Assert
            Assert.That(actualExitCode, Is.EqualTo(expectedExitCode));

            Assert.That(standardOut.ToString(), Is.Empty);
            Assert.That(standardError.ToString(), Is.EqualTo($"Unhandled exception: {expectedException}{Environment.NewLine}"));
        }
        #endregion List Command

        #region Status Command
        [Test]
        public async Task MainInternal_StatusCommand_NoOptions_ParsesWithDefaultValues()
        {
            // Arrange
            var args = new string[] { "status" };
            var expectedExitCode = (int)ErrorCode.Success;

            var mockConsoleModeMiddleware = new Mock<IConsoleModeMiddleware>();
            var mockConsoleClipboard = new Mock<IConsoleClipboard>(MockBehavior.Strict);

            IConsoleWriter? actualConsoleWriter = null;
            mockConsoleClipboard
                .Setup(mock => mock.GetClipboardHistoryStatus(
                    It.IsAny<IConsoleWriter>(),
                    It.IsAny<InvocationContext>()))
                .Callback((IConsoleWriter consoleWriter, InvocationContext invocationContext) =>
                {
                    actualConsoleWriter = consoleWriter;
                    invocationContext.ExitCode = expectedExitCode;
                });

            var pastCommand = new PastCommand(mockConsoleClipboard.Object);

            var standardOut = new StringBuilder();
            var standardError = new StringBuilder();
            var mockConsole = CreateMockConsole(standardOut, standardError);

            // Act
            var actualExitCode = await Program.MainInternal(args, pastCommand, mockConsoleModeMiddleware.Object, mockConsole.Object);

            // Assert
            Assert.That(actualExitCode, Is.EqualTo(expectedExitCode));

            Assert.That(actualConsoleWriter, Is.Not.Null);
            Assert.That(actualConsoleWriter.SuppressErrorOutput, Is.False);
            Assert.That(actualConsoleWriter.EnableAnsiProcessing, Is.False);
            Assert.That(actualConsoleWriter.AnsiResetType, Is.EqualTo(AnsiResetType.Off));

            Assert.That(standardOut.ToString(), Is.Empty);
            Assert.That(standardError.ToString(), Is.Empty);

            mockConsoleModeMiddleware.Verify(
                mock => mock.ConfigureConsoleMode(It.IsAny<InvocationContext>()),
                Times.Never);
        }

        [Test]
        [TestCase("--quiet")]
        [TestCase("-q")]
        public async Task MainInternal_StatusCommand_QuietFlag_SuppressErrorOutputTrue(string quietFlag)
        {
            // Arrange
            var args = new string[] { "status", quietFlag };
            var expectedExitCode = (int)ErrorCode.Success;

            var mockConsoleModeMiddleware = new Mock<IConsoleModeMiddleware>();
            var mockConsoleClipboard = new Mock<IConsoleClipboard>(MockBehavior.Strict);

            IConsoleWriter? actualConsoleWriter = null;
            mockConsoleClipboard
                .Setup(mock => mock.GetClipboardHistoryStatus(
                    It.IsAny<IConsoleWriter>(),
                    It.IsAny<InvocationContext>()))
                .Callback((IConsoleWriter consoleWriter, InvocationContext invocationContext) =>
                {
                    actualConsoleWriter = consoleWriter;
                    invocationContext.ExitCode = expectedExitCode;
                });

            var pastCommand = new PastCommand(mockConsoleClipboard.Object);

            var standardOut = new StringBuilder();
            var standardError = new StringBuilder();
            var mockConsole = CreateMockConsole(standardOut, standardError);

            // Act
            var actualExitCode = await Program.MainInternal(args, pastCommand, mockConsoleModeMiddleware.Object, mockConsole.Object);

            // Assert
            Assert.That(actualExitCode, Is.EqualTo(expectedExitCode));

            Assert.That(actualConsoleWriter, Is.Not.Null);
            Assert.That(actualConsoleWriter.SuppressErrorOutput, Is.True);
            Assert.That(actualConsoleWriter.EnableAnsiProcessing, Is.False);
            Assert.That(actualConsoleWriter.AnsiResetType, Is.EqualTo(AnsiResetType.Off));

            Assert.That(standardOut.ToString(), Is.Empty);
            Assert.That(standardError.ToString(), Is.Empty);

            mockConsoleModeMiddleware.Verify(
                mock => mock.ConfigureConsoleMode(It.IsAny<InvocationContext>()),
                Times.Never);
        }

        [Test]
        [TestCase("-h")]
        [TestCase("-?")]
        public async Task MainInternal_StatusCommand_HelpShortFlag_OutputsExpectedHelp(string helpFlag)
        {
            // Arrange
            var args = new string[] { "status", helpFlag };
            var expectedExitCode = (int)ErrorCode.Success;

            var mockConsoleClipboard = new Mock<IConsoleClipboard>(MockBehavior.Strict);
            var pastCommand = new PastCommand(mockConsoleClipboard.Object);

            var standardOut = new StringBuilder();
            var standardError = new StringBuilder();
            var mockConsole = CreateMockConsole(standardOut, standardError);

            // Act
            var actualExitCode = await Program.MainInternal(args, pastCommand, null, mockConsole.Object);

            // Assert
            Assert.That(actualExitCode, Is.EqualTo(expectedExitCode));

            Assert.That(standardOut.ToString(), Is.EqualTo(StatusCommandShortHelp));
            Assert.That(standardError.ToString(), Is.Empty);
        }

        [Test]
        public async Task MainInternal_StatusCommand_HelpLongFlag_OutputsExpectedHelp()
        {
            // Arrange
            var args = new string[] { "status", "--help" };
            var expectedExitCode = (int)ErrorCode.Success;

            var mockConsoleClipboard = new Mock<IConsoleClipboard>(MockBehavior.Strict);
            var pastCommand = new PastCommand(mockConsoleClipboard.Object);

            var standardOut = new StringBuilder();
            var standardError = new StringBuilder();
            var mockConsole = CreateMockConsole(standardOut, standardError);

            // Act
            var actualExitCode = await Program.MainInternal(args, pastCommand, null, mockConsole.Object);

            // Assert
            Assert.That(actualExitCode, Is.EqualTo(expectedExitCode));

            Assert.That(standardOut.ToString(), Is.EqualTo(StatusCommandLongHelp));
            Assert.That(standardError.ToString(), Is.Empty);
        }

        [Test]
        public async Task MainInternal_StatusCommand_ThrowsException_ReturnsUnexpectedError()
        {
            // Arrange
            var args = new string[] { "status" };
            var expectedExitCode = (int)ErrorCode.UnexpectedError;
            var expectedException = new Exception("Uh-oh! :O");

            var mockConsoleClipboard = new Mock<IConsoleClipboard>(MockBehavior.Strict);
            mockConsoleClipboard
                .Setup(mock => mock.GetClipboardHistoryStatus(
                    It.IsAny<IConsoleWriter>(),
                    It.IsAny<InvocationContext>()))
                .Throws(expectedException);

            var pastCommand = new PastCommand(mockConsoleClipboard.Object);

            var standardOut = new StringBuilder();
            var standardError = new StringBuilder();
            var mockConsole = CreateMockConsole(standardOut, standardError);

            // Act
            var actualExitCode = await Program.MainInternal(args, pastCommand, null, mockConsole.Object);

            // Assert
            Assert.That(actualExitCode, Is.EqualTo(expectedExitCode));

            Assert.That(standardOut.ToString(), Is.Empty);
            Assert.That(standardError.ToString(), Is.EqualTo($"Unhandled exception: {expectedException}{Environment.NewLine}"));
        }

        [Test]
        public async Task MainInternal_StatusCommand_NullCommandAndMiddlewareWithAnsiOption_ReturnsParseError()
        {
            // This test basically only exists to bring code coverage to 100% by passing null for
            // the command and middleware so that this covers them being instantiaed when null.
            // Using the status command since this will fail parsing and not actually attempt to
            // hit the real system clipboard APIs.

            // Arrange
            var args = new string[] { "status", "--ansi" };
            var expectedExitCode = (int)ErrorCode.ParseError;

            // Act
            var actualExitCode = await Program.MainInternal(args, null, null, null);

            // Assert
            Assert.That(actualExitCode, Is.EqualTo(expectedExitCode));
        }
        #endregion Status Command

        #region Helpers
        /// <summary>
        /// Creates a mock <see cref="IConsole"/> with the provided <see cref="StringBuilder"/> instances
        /// to capture standard out and standard error.
        /// </summary>
        /// <param name="standardOut">String builder to use for capturing standard out.</param>
        /// <param name="standardError">String builder to use for capturing standard error.</param>
        /// <returns>The mock console.</returns>
        private static Mock<IConsole> CreateMockConsole(StringBuilder standardOut, StringBuilder standardError)
        {
            var mockErrorStreamWriter = new Mock<IStandardStreamWriter>(MockBehavior.Strict);
            mockErrorStreamWriter
                .Setup(mock => mock.Write(It.IsAny<string?>()))
                .Callback((string? value) => standardError.Append(value));

            var mockOutStreamWriter = new Mock<IStandardStreamWriter>(MockBehavior.Strict);
            mockOutStreamWriter
                .Setup(mock => mock.Write(It.IsAny<string?>()))
                .Callback((string? value) => standardOut.Append(value));

            var mockConsole = new Mock<IConsole>(MockBehavior.Strict);
            mockConsole
                .SetupGet(mock => mock.Error)
                .Returns(mockErrorStreamWriter.Object);
            mockConsole
                .SetupGet(mock => mock.Out)
                .Returns(mockOutStreamWriter.Object);

            return mockConsole;
        }

        /// <summary>
        /// Gets the expected version from the assembly.
        /// </summary>
        /// <returns></returns>
        private static string GetExpectedVersion()
        {
            // Replicate the logic used by System.CommandLine for choosing the assembly.
            // This unfortunately results in the version being derrived from the test assembly rather than the
            // executing assembly, so we need to match that here so tests inspecting the version can pass.
            var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
            var assemblyVersionAttribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();

            if (assemblyVersionAttribute is null)
            {
                return assembly.GetName().Version?.ToString() ?? "";
            }
            else
            {
                return assemblyVersionAttribute.InformationalVersion;
            }
        }
        #endregion Helpers
    }
}
