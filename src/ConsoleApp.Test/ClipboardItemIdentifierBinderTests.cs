using past.ConsoleApp.Binders;
using System.CommandLine;
using System.CommandLine.Parsing;

namespace past.ConsoleApp.Test
{
    public class ClipboardItemIdentifierBinderTests
    {
        #region Constructor
        [Test]
        public void Constructor_NonNullArgument_Success()
        {
            var commandFactory = new CommandFactory();
            Assert.DoesNotThrow(() => new ClipboardItemIdentifierBinder(commandFactory.IdentifierArgument));
        }

        [Test]
        public void Constructor_NullArgument_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ClipboardItemIdentifierBinder(null!));
        }
        #endregion Constructor

        #region GetBoundValue
        [Test]
        public void GetBoundValue_ValidIndex_ReturnsIdentifierWithExpectedIndex()
        {
            // Arrange
            var expectedIndex = 42;
            var commandFactory = new CommandFactory();

            var testCommand = new Command("test");
            testCommand.AddArgument(commandFactory.IdentifierArgument);

            var testCommandParser = new Parser(testCommand);
            var parseResult = testCommandParser.Parse($"{expectedIndex}");

            var binder = new ClipboardItemIdentifierBinder(commandFactory.IdentifierArgument);

            // Act
            var identifier = binder.GetBoundValue(parseResult);

            // Assert
            Assert.That(identifier, Is.Not.Null);
            Assert.That(identifier.TryGetAsIndex(out var actualIndex), Is.True);
            Assert.That(actualIndex, Is.EqualTo(expectedIndex));
        }

        [Test]
        public void GetBoundValue_ValidId_ReturnsIdentifierWithExpectedId()
        {
            // Arrange
            var expectedId = Guid.NewGuid();
            var commandFactory = new CommandFactory();

            var testCommand = new Command("test");
            testCommand.AddArgument(commandFactory.IdentifierArgument);

            var testCommandParser = new Parser(testCommand);
            var parseResult = testCommandParser.Parse($"{expectedId}");

            var binder = new ClipboardItemIdentifierBinder(commandFactory.IdentifierArgument);

            // Act
            var identifier = binder.GetBoundValue(parseResult);

            // Assert
            Assert.That(identifier, Is.Not.Null);
            Assert.That(identifier.TryGetAsGuid(out var actualId), Is.True);
            Assert.That(actualId, Is.EqualTo(expectedId));
        }

        [Test]
        [TestCase("")]
        [TestCase("You_shall_not_parse!")]
        public void GetBoundValue_InvalidIdentifier_ThrowsArgumentException(string invalidIdentifierString)
        {
            // Arrange
            var expectedExceptionMessage = $"Failed to parse identifier: ${invalidIdentifierString}";
            var commandFactory = new CommandFactory();

            var testCommand = new Command("test");
            testCommand.AddArgument(commandFactory.IdentifierArgument);

            var testCommandParser = new Parser(testCommand);
            var parseResult = testCommandParser.Parse(invalidIdentifierString);

            var binder = new ClipboardItemIdentifierBinder(commandFactory.IdentifierArgument);

            // Act
            var actualException = Assert.Throws<ArgumentException>(() => binder.GetBoundValue(parseResult));
            Assert.That(actualException.Message, Is.EqualTo(expectedExceptionMessage));
        }
        #endregion GetBoundValue
    }
}
