using past.ConsoleApp.Binders;
using System.CommandLine;
using System.CommandLine.Parsing;

namespace past.ConsoleApp.Test
{
    public class ValueFormatterBinderTests
    {
        #region Constructor
        [Test]
        public void Create_NonNullOptions_Success()
        {
            // Arrange
            var nullOption = new Option<bool>("--null");
            var indexOption = new Option<bool>("--index");
            var idOption = new Option<bool>("--id");
            var timeOption = new Option<bool>("--time");

            // Act + Assert
            Assert.DoesNotThrow(() => new ValueFormatterBinder(
                nullOption,
                indexOption,
                idOption,
                timeOption));
        }

        [Test]
        public void Create_NullNullOption_ThrowsArgumentNullException()
        {
            // Arrange
            var indexOption = new Option<bool>("--index");
            var idOption = new Option<bool>("--id");
            var timeOption = new Option<bool>("--time");

            // Act + Assert
            Assert.Throws<ArgumentNullException>(() => new ValueFormatterBinder(
                null!,
                indexOption,
                idOption,
                timeOption));
        }

        [Test]
        public void Create_NullIndexOption_ThrowsArgumentNullException()
        {
            // Arrange
            var nullOption = new Option<bool>("--null");
            var idOption = new Option<bool>("--id");
            var timeOption = new Option<bool>("--time");

            // Act + Assert
            Assert.Throws<ArgumentNullException>(() => new ValueFormatterBinder(
                nullOption,
                null!,
                idOption,
                timeOption));
        }

        [Test]
        public void Create_NullIdOption_ThrowsArgumentNullException()
        {
            // Arrange
            var nullOption = new Option<bool>("--null");
            var indexOption = new Option<bool>("--index");
            var timeOption = new Option<bool>("--time");

            // Act + Assert
            Assert.Throws<ArgumentNullException>(() => new ValueFormatterBinder(
                nullOption,
                indexOption,
                null!,
                timeOption));
        }

        [Test]
        public void Create_NullTimeOption_ThrowsArgumentNullException()
        {
            // Arrange
            var nullOption = new Option<bool>("--null");
            var indexOption = new Option<bool>("--index");
            var idOption = new Option<bool>("--id");

            // Act + Assert
            Assert.Throws<ArgumentNullException>(() => new ValueFormatterBinder(
                nullOption,
                indexOption,
                idOption,
                null!));
        }
        #endregion Constructor

        #region GetBoundValue
        [Test]
        public void GetBoundValue_NoBoundOptions_ReturnsFormatterWithExpectedOptions()
        {
            // Arrange
            var nullOption = new Option<bool>("--null");
            var indexOption = new Option<bool>("--index");
            var idOption = new Option<bool>("--id");
            var timeOption = new Option<bool>("--time");

            var testCommand = new Command("test");
            testCommand.AddOption(nullOption);
            testCommand.AddOption(indexOption);
            testCommand.AddOption(idOption);
            testCommand.AddOption(timeOption);

            var testCommandParser = new Parser(testCommand);
            var parseResult = testCommandParser.Parse("");

            var binder = new ValueFormatterBinder(nullOption, indexOption, idOption, timeOption);

            // Act
            var formatter = binder.GetBoundValue(parseResult);

            // Assert
            Assert.That(formatter, Is.Not.Null);
            Assert.That(formatter.NullLineEnding, Is.False);
            Assert.That(formatter.IncludeIndex, Is.False);
            Assert.That(formatter.IncludeId, Is.False);
            Assert.That(formatter.IncludeTimestamp, Is.False);
        }

        [Test]
        public void GetBoundValue_NullOption_ReturnsFormatterWithExpectedOptions()
        {
            // Arrange
            var nullOption = new Option<bool>("--null");
            var indexOption = new Option<bool>("--index");
            var idOption = new Option<bool>("--id");
            var timeOption = new Option<bool>("--time");

            var testCommand = new Command("test");
            testCommand.AddOption(nullOption);
            testCommand.AddOption(indexOption);
            testCommand.AddOption(idOption);
            testCommand.AddOption(timeOption);

            var testCommandParser = new Parser(testCommand);
            var parseResult = testCommandParser.Parse("--null");

            var binder = new ValueFormatterBinder(nullOption, indexOption, idOption, timeOption);

            // Act
            var formatter = binder.GetBoundValue(parseResult);

            // Assert
            Assert.That(formatter, Is.Not.Null);
            Assert.That(formatter.NullLineEnding, Is.True);
            Assert.That(formatter.IncludeIndex, Is.False);
            Assert.That(formatter.IncludeId, Is.False);
            Assert.That(formatter.IncludeTimestamp, Is.False);
        }

        [Test]
        public void GetBoundValue_IndexOption_ReturnsFormatterWithExpectedOptions()
        {
            // Arrange
            var nullOption = new Option<bool>("--null");
            var indexOption = new Option<bool>("--index");
            var idOption = new Option<bool>("--id");
            var timeOption = new Option<bool>("--time");

            var testCommand = new Command("test");
            testCommand.AddOption(nullOption);
            testCommand.AddOption(indexOption);
            testCommand.AddOption(idOption);
            testCommand.AddOption(timeOption);

            var testCommandParser = new Parser(testCommand);
            var parseResult = testCommandParser.Parse("--index");

            var binder = new ValueFormatterBinder(nullOption, indexOption, idOption, timeOption);

            // Act
            var formatter = binder.GetBoundValue(parseResult);

            // Assert
            Assert.That(formatter, Is.Not.Null);
            Assert.That(formatter.NullLineEnding, Is.False);
            Assert.That(formatter.IncludeIndex, Is.True);
            Assert.That(formatter.IncludeId, Is.False);
            Assert.That(formatter.IncludeTimestamp, Is.False);
        }

        [Test]
        public void GetBoundValue_IdOption_ReturnsFormatterWithExpectedOptions()
        {
            // Arrange
            var nullOption = new Option<bool>("--null");
            var indexOption = new Option<bool>("--index");
            var idOption = new Option<bool>("--id");
            var timeOption = new Option<bool>("--time");

            var testCommand = new Command("test");
            testCommand.AddOption(nullOption);
            testCommand.AddOption(indexOption);
            testCommand.AddOption(idOption);
            testCommand.AddOption(timeOption);

            var testCommandParser = new Parser(testCommand);
            var parseResult = testCommandParser.Parse("--id");

            var binder = new ValueFormatterBinder(nullOption, indexOption, idOption, timeOption);

            // Act
            var formatter = binder.GetBoundValue(parseResult);

            // Assert
            Assert.That(formatter, Is.Not.Null);
            Assert.That(formatter.NullLineEnding, Is.False);
            Assert.That(formatter.IncludeIndex, Is.False);
            Assert.That(formatter.IncludeId, Is.True);
            Assert.That(formatter.IncludeTimestamp, Is.False);
        }

        [Test]
        public void GetBoundValue_TimeOption_ReturnsFormatterWithExpectedOptions()
        {
            // Arrange
            var nullOption = new Option<bool>("--null");
            var indexOption = new Option<bool>("--index");
            var idOption = new Option<bool>("--id");
            var timeOption = new Option<bool>("--time");

            var testCommand = new Command("test");
            testCommand.AddOption(nullOption);
            testCommand.AddOption(indexOption);
            testCommand.AddOption(idOption);
            testCommand.AddOption(timeOption);

            var testCommandParser = new Parser(testCommand);
            var parseResult = testCommandParser.Parse("--time");

            var binder = new ValueFormatterBinder(nullOption, indexOption, idOption, timeOption);

            // Act
            var formatter = binder.GetBoundValue(parseResult);

            // Assert
            Assert.That(formatter, Is.Not.Null);
            Assert.That(formatter.NullLineEnding, Is.False);
            Assert.That(formatter.IncludeIndex, Is.False);
            Assert.That(formatter.IncludeId, Is.False);
            Assert.That(formatter.IncludeTimestamp, Is.True);
        }

        [Test]
        public void GetBoundValue_AllOptions_ReturnsFormatterWithExpectedOptions()
        {
            // Arrange
            var nullOption = new Option<bool>("--null");
            var indexOption = new Option<bool>("--index");
            var idOption = new Option<bool>("--id");
            var timeOption = new Option<bool>("--time");

            var testCommand = new Command("test");
            testCommand.AddOption(nullOption);
            testCommand.AddOption(indexOption);
            testCommand.AddOption(idOption);
            testCommand.AddOption(timeOption);

            var testCommandParser = new Parser(testCommand);
            var parseResult = testCommandParser.Parse("--null --index --id --time");

            var binder = new ValueFormatterBinder(nullOption, indexOption, idOption, timeOption);

            // Act
            var formatter = binder.GetBoundValue(parseResult);

            // Assert
            Assert.That(formatter, Is.Not.Null);
            Assert.That(formatter.NullLineEnding, Is.True);
            Assert.That(formatter.IncludeIndex, Is.True);
            Assert.That(formatter.IncludeId, Is.True);
            Assert.That(formatter.IncludeTimestamp, Is.True);
        }
        #endregion GetBoundValue
    }
}
