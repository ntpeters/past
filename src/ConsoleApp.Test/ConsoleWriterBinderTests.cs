using Moq;
using past.ConsoleApp.Binders;
using System.CommandLine;
using System.CommandLine.Parsing;

namespace past.ConsoleApp.Test
{
    public class ConsoleWriterBinderTests
    {
        #region Constructors
        [Test]
        public void CreateWithQuietOption_NonNull_Success()
        {
            var commandFactory = new CommandFactory();
            Assert.DoesNotThrow(() => new ConsoleWriterBinder(commandFactory.QuietOption));
        }

        [Test]
        public void CreateWithQuietOption_Null_ThrowsAgumentNullExceptioon()
        {
            Assert.Throws<ArgumentNullException>(() => new ConsoleWriterBinder(null!));
        }

        [Test]
        public void CreateWithAllOptions_AllNonNull_Success()
        {
            var commandFactory = new CommandFactory();
            Assert.DoesNotThrow(() => new ConsoleWriterBinder(
                commandFactory.AnsiOption,
                commandFactory.AnsiResetOption,
                commandFactory.QuietOption));
        }

        [Test]
        public void CreateWithAllOptions_NullAnsiOption_ThrowsArgumentNullException()
        {
            var commandFactory = new CommandFactory();
            Assert.Throws<ArgumentNullException>(() => new ConsoleWriterBinder(
                null!,
                commandFactory.AnsiResetOption,
                commandFactory.QuietOption));
        }

        [Test]
        public void CreateWithAllOptions_NullAnsiResetOption_ThrowsArgumentNullException()
        {
            var commandFactory = new CommandFactory();
            Assert.Throws<ArgumentNullException>(() => new ConsoleWriterBinder(
                commandFactory.AnsiOption,
                null!,
                commandFactory.QuietOption));
        }

        [Test]
        public void CreateWithAllOptions_NullQuietOption_ThrowsArgumentNullException()
        {
            var commandFactory = new CommandFactory();
            Assert.Throws<ArgumentNullException>(() => new ConsoleWriterBinder(
                commandFactory.AnsiOption,
                commandFactory.AnsiResetOption,
                null!));
        }
        #endregion Constructors

        #region GetBoundValue - Created With Quiet Option Only
        [Test]
        public void GetBoundValueQuietOptionOnly_NoBoundOptions_ReturnsConsoleWriterWithNoOptionsEnabled()
        {
            // Arrange
            var commandFactory = new CommandFactory();
            var testCommand = new Command("test");
            testCommand.AddOption(commandFactory.AnsiOption);
            testCommand.AddOption(commandFactory.AnsiResetOption);
            testCommand.AddOption(commandFactory.QuietOption);

            var testCommandParser = new Parser(testCommand);
            var parseResult = testCommandParser.Parse("");

            var mockConsole = new Mock<IConsole>(MockBehavior.Strict);

            var binder = new ConsoleWriterBinder(commandFactory.QuietOption);

            // Act
            var consoleWriter = binder.GetBoundValue(parseResult, mockConsole.Object);

            // Assert
            Assert.That(consoleWriter, Is.Not.Null);
            Assert.That(consoleWriter.EnableAnsiProcessing, Is.False);
            Assert.That(consoleWriter.AnsiResetType, Is.EqualTo(AnsiResetType.Off));
            Assert.That(consoleWriter.SuppressErrorOutput, Is.False);
        }

        [Test]
        public void GetBoundValueQuietOptionOnly_AnsiOption_ReturnsConsoleWriterWithNoOptionsEnabled()
        {
            // Arrange
            var commandFactory = new CommandFactory();
            var testCommand = new Command("test");
            testCommand.AddOption(commandFactory.AnsiOption);
            testCommand.AddOption(commandFactory.AnsiResetOption);
            testCommand.AddOption(commandFactory.QuietOption);

            var testCommandParser = new Parser(testCommand);
            var parseResult = testCommandParser.Parse("--ansi");

            var mockConsole = new Mock<IConsole>(MockBehavior.Strict);

            var binder = new ConsoleWriterBinder(commandFactory.QuietOption);

            // Act
            var consoleWriter = binder.GetBoundValue(parseResult, mockConsole.Object);

            // Assert
            Assert.That(consoleWriter, Is.Not.Null);
            Assert.That(consoleWriter.EnableAnsiProcessing, Is.False);
            Assert.That(consoleWriter.AnsiResetType, Is.EqualTo(AnsiResetType.Off));
            Assert.That(consoleWriter.SuppressErrorOutput, Is.False);
        }

        [Test]
        [TestCase(AnsiResetType.Auto)]
        [TestCase(AnsiResetType.On)]
        [TestCase(AnsiResetType.Off)]
        public void GetBoundValueQuietOptionOnly_AnsiResetOption_ReturnsConsoleWriterWithNoOptionsEnabled(AnsiResetType inputAnsiResetType)
        {
            // Arrange
            var commandFactory = new CommandFactory();
            var testCommand = new Command("test");
            testCommand.AddOption(commandFactory.AnsiOption);
            testCommand.AddOption(commandFactory.AnsiResetOption);
            testCommand.AddOption(commandFactory.QuietOption);

            var testCommandParser = new Parser(testCommand);
            var parseResult = testCommandParser.Parse($"--ansi-reset {inputAnsiResetType}");

            var mockConsole = new Mock<IConsole>(MockBehavior.Strict);

            var binder = new ConsoleWriterBinder(commandFactory.QuietOption);

            // Act
            var consoleWriter = binder.GetBoundValue(parseResult, mockConsole.Object);

            // Assert
            Assert.That(consoleWriter, Is.Not.Null);
            Assert.That(consoleWriter.EnableAnsiProcessing, Is.False);
            Assert.That(consoleWriter.AnsiResetType, Is.EqualTo(AnsiResetType.Off));
            Assert.That(consoleWriter.SuppressErrorOutput, Is.False);
        }

        [Test]
        public void GetBoundValueQuietOptionOnly_QuietOption_ReturnsConsoleWriterWithOnlySuppressErrorOutputEnabled()
        {
            // Arrange
            var commandFactory = new CommandFactory();
            var testCommand = new Command("test");
            testCommand.AddOption(commandFactory.AnsiOption);
            testCommand.AddOption(commandFactory.AnsiResetOption);
            testCommand.AddOption(commandFactory.QuietOption);

            var testCommandParser = new Parser(testCommand);
            var parseResult = testCommandParser.Parse("--quiet");

            var mockConsole = new Mock<IConsole>(MockBehavior.Strict);

            var binder = new ConsoleWriterBinder(commandFactory.QuietOption);

            // Act
            var consoleWriter = binder.GetBoundValue(parseResult, mockConsole.Object);

            // Assert
            Assert.That(consoleWriter, Is.Not.Null);
            Assert.That(consoleWriter.EnableAnsiProcessing, Is.False);
            Assert.That(consoleWriter.AnsiResetType, Is.EqualTo(AnsiResetType.Off));
            Assert.That(consoleWriter.SuppressErrorOutput, Is.True);
        }

        [Test]
        public void GetBoundValueQuietOptionOnly_AllOptions_ReturnsConsoleWriterWithOnlySuppressErrorOutputEnabled()
        {
            // Arrange
            var commandFactory = new CommandFactory();
            var testCommand = new Command("test");
            testCommand.AddOption(commandFactory.AnsiOption);
            testCommand.AddOption(commandFactory.AnsiResetOption);
            testCommand.AddOption(commandFactory.QuietOption);

            var testCommandParser = new Parser(testCommand);
            var parseResult = testCommandParser.Parse("--ansi --ansi-reset On --quiet");

            var mockConsole = new Mock<IConsole>(MockBehavior.Strict);

            var binder = new ConsoleWriterBinder(commandFactory.QuietOption);

            // Act
            var consoleWriter = binder.GetBoundValue(parseResult, mockConsole.Object);

            // Assert
            Assert.That(consoleWriter, Is.Not.Null);
            Assert.That(consoleWriter.EnableAnsiProcessing, Is.False);
            Assert.That(consoleWriter.AnsiResetType, Is.EqualTo(AnsiResetType.Off));
            Assert.That(consoleWriter.SuppressErrorOutput, Is.True);
        }
        #endregion GetBoundValue - Created With Quiet Option Only

        #region GetBoundValue - Created With All Options
        [Test]
        public void GetBoundValueAllOptions_NoBoundOptions_ReturnsConsoleWriterWithExpectedOptions()
        {
            // Arrange
            var commandFactory = new CommandFactory();
            var testCommand = new Command("test");
            testCommand.AddOption(commandFactory.AnsiOption);
            testCommand.AddOption(commandFactory.AnsiResetOption);
            testCommand.AddOption(commandFactory.QuietOption);

            var testCommandParser = new Parser(testCommand);
            var parseResult = testCommandParser.Parse("");

            var mockConsole = new Mock<IConsole>(MockBehavior.Strict);

            var binder = new ConsoleWriterBinder(
                commandFactory.AnsiOption,
                commandFactory.AnsiResetOption,
                commandFactory.QuietOption);

            // Act
            var consoleWriter = binder.GetBoundValue(parseResult, mockConsole.Object);

            // Assert
            Assert.That(consoleWriter, Is.Not.Null);
            Assert.That(consoleWriter.EnableAnsiProcessing, Is.False);
            Assert.That(consoleWriter.AnsiResetType, Is.EqualTo(AnsiResetType.Auto));
            Assert.That(consoleWriter.SuppressErrorOutput, Is.False);
        }

        [Test]
        public void GetBoundValueAllOptions_AnsiOption_ReturnsConsoleWriterWithExpectedOptions()
        {
            // Arrange
            var commandFactory = new CommandFactory();
            var testCommand = new Command("test");
            testCommand.AddOption(commandFactory.AnsiOption);
            testCommand.AddOption(commandFactory.AnsiResetOption);
            testCommand.AddOption(commandFactory.QuietOption);

            var testCommandParser = new Parser(testCommand);
            var parseResult = testCommandParser.Parse("--ansi");

            var mockConsole = new Mock<IConsole>(MockBehavior.Strict);

            var binder = new ConsoleWriterBinder(
                commandFactory.AnsiOption,
                commandFactory.AnsiResetOption,
                commandFactory.QuietOption);

            // Act
            var consoleWriter = binder.GetBoundValue(parseResult, mockConsole.Object);

            // Assert
            Assert.That(consoleWriter, Is.Not.Null);
            Assert.That(consoleWriter.EnableAnsiProcessing, Is.True);
            Assert.That(consoleWriter.AnsiResetType, Is.EqualTo(AnsiResetType.Auto));
            Assert.That(consoleWriter.SuppressErrorOutput, Is.False);
        }

        [Test]
        [TestCase(AnsiResetType.Auto)]
        [TestCase(AnsiResetType.On)]
        [TestCase(AnsiResetType.Off)]
        public void GetBoundValueAllOptions_AnsiResetOption_ReturnsConsoleWriterWithExpectedOptions(AnsiResetType inputAnsiResetType)
        {
            // Arrange
            var commandFactory = new CommandFactory();
            var testCommand = new Command("test");
            testCommand.AddOption(commandFactory.AnsiOption);
            testCommand.AddOption(commandFactory.AnsiResetOption);
            testCommand.AddOption(commandFactory.QuietOption);

            var testCommandParser = new Parser(testCommand);
            var parseResult = testCommandParser.Parse($"--ansi-reset {inputAnsiResetType}");

            var mockConsole = new Mock<IConsole>(MockBehavior.Strict);

            var binder = new ConsoleWriterBinder(
                commandFactory.AnsiOption,
                commandFactory.AnsiResetOption,
                commandFactory.QuietOption);

            // Act
            var consoleWriter = binder.GetBoundValue(parseResult, mockConsole.Object);

            // Assert
            Assert.That(consoleWriter, Is.Not.Null);
            Assert.That(consoleWriter.EnableAnsiProcessing, Is.False);
            Assert.That(consoleWriter.AnsiResetType, Is.EqualTo(inputAnsiResetType));
            Assert.That(consoleWriter.SuppressErrorOutput, Is.False);
        }

        [Test]
        public void GetBoundValueAllOptions_QuietOption_ReturnsConsoleWriterWithExpectedOptions()
        {
            // Arrange
            var commandFactory = new CommandFactory();
            var testCommand = new Command("test");
            testCommand.AddOption(commandFactory.AnsiOption);
            testCommand.AddOption(commandFactory.AnsiResetOption);
            testCommand.AddOption(commandFactory.QuietOption);

            var testCommandParser = new Parser(testCommand);
            var parseResult = testCommandParser.Parse("--quiet");

            var mockConsole = new Mock<IConsole>(MockBehavior.Strict);

            var binder = new ConsoleWriterBinder(
                commandFactory.AnsiOption,
                commandFactory.AnsiResetOption,
                commandFactory.QuietOption);

            // Act
            var consoleWriter = binder.GetBoundValue(parseResult, mockConsole.Object);

            // Assert
            Assert.That(consoleWriter, Is.Not.Null);
            Assert.That(consoleWriter.EnableAnsiProcessing, Is.False);
            Assert.That(consoleWriter.AnsiResetType, Is.EqualTo(AnsiResetType.Auto));
            Assert.That(consoleWriter.SuppressErrorOutput, Is.True);
        }

        [Test]
        public void GetBoundValueAllOptions_AllOptions_ReturnsConsoleWriterWithExpectedOptions()
        {
            // Arrange
            var commandFactory = new CommandFactory();
            var testCommand = new Command("test");
            testCommand.AddOption(commandFactory.AnsiOption);
            testCommand.AddOption(commandFactory.AnsiResetOption);
            testCommand.AddOption(commandFactory.QuietOption);

            var testCommandParser = new Parser(testCommand);
            var parseResult = testCommandParser.Parse("--ansi --ansi-reset On --quiet");

            var mockConsole = new Mock<IConsole>(MockBehavior.Strict);

            var binder = new ConsoleWriterBinder(
                commandFactory.AnsiOption,
                commandFactory.AnsiResetOption,
                commandFactory.QuietOption);

            // Act
            var consoleWriter = binder.GetBoundValue(parseResult, mockConsole.Object);

            // Assert
            Assert.That(consoleWriter, Is.Not.Null);
            Assert.That(consoleWriter.EnableAnsiProcessing, Is.True);
            Assert.That(consoleWriter.AnsiResetType, Is.EqualTo(AnsiResetType.On));
            Assert.That(consoleWriter.SuppressErrorOutput, Is.True);
        }
        #endregion GetBoundValue - Created With All Options
    }
}
