using past.ConsoleApp.Binders;
using past.Core;
using System.CommandLine;
using System.CommandLine.Parsing;

namespace past.ConsoleApp.Test
{
    public class ContentTypeBinderTests
    {
        #region Constructor
        [Test]
        public void Constructor_NonNullParameters_Success()
        {
            var commandFactory = new CommandFactory();
            Assert.DoesNotThrow(() => new ContentTypeBinder(commandFactory.TypeOption, commandFactory.AllOption));
        }

        [Test]
        public void Constructor_NullTypeOption_ThrowsArgumentNullException()
        {
            var commandFactory = new CommandFactory();
            Assert.Throws<ArgumentNullException>(() => new ContentTypeBinder(null!, commandFactory.AllOption));
        }

        [Test]
        public void Constructor_NullAllOption_ThrowsArgumentNullException()
        {
            var commandFactory = new CommandFactory();
            Assert.Throws<ArgumentNullException>(() => new ContentTypeBinder(commandFactory.TypeOption, null!));
        }
        #endregion Constructor

        #region GetBoundValue
        [Test]
        public void GetBoundValue_OnlyAllOptionProvided_ReturnsAllContentType()
        {
            // Arrange
            var expectedContentType = ContentType.All;
            var commandFactory = new CommandFactory();

            var testCommand = new Command("test");
            testCommand.AddOption(commandFactory.TypeOption);
            testCommand.AddOption(commandFactory.AllOption);

            var testCommandParser = new Parser(testCommand);
            var parseResult = testCommandParser.Parse("--all");

            var binder = new ContentTypeBinder(commandFactory.TypeOption, commandFactory.AllOption);

            // Act
            var actualContentType = binder.GetBoundValue(parseResult);

            // Assert
            Assert.That(actualContentType, Is.EqualTo(expectedContentType));
        }

        [Test]
        [TestCase(ContentType.Text)]
        [TestCase(ContentType.Image)]
        [TestCase(ContentType.All)]
        public void GetBoundValue_AllOptionWithContentType_ReturnsAllContentType(ContentType type)
        {
            // Arrange
            var expectedContentType = ContentType.All;
            var commandFactory = new CommandFactory();

            var testCommand = new Command("test");
            testCommand.AddOption(commandFactory.TypeOption);
            testCommand.AddOption(commandFactory.AllOption);

            var testCommandParser = new Parser(testCommand);
            var parseResult = testCommandParser.Parse($"--all --type {type.ToString()}");

            var binder = new ContentTypeBinder(commandFactory.TypeOption, commandFactory.AllOption);

            // Act
            var actualContentType = binder.GetBoundValue(parseResult);

            // Assert
            Assert.That(actualContentType, Is.EqualTo(expectedContentType));
        }

        [Test]
        [TestCase(ContentType.Text)]
        [TestCase(ContentType.Image)]
        [TestCase(ContentType.All)]
        public void GetBoundValue_OnlyContentTypeProvided_ReturnsExpectedContentType(ContentType expectedContentType)
        {
            // Arrange
            var commandFactory = new CommandFactory();

            var testCommand = new Command("test");
            testCommand.AddOption(commandFactory.TypeOption);
            testCommand.AddOption(commandFactory.AllOption);

            var testCommandParser = new Parser(testCommand);
            var parseResult = testCommandParser.Parse($"--type {expectedContentType.ToString()}");

            var binder = new ContentTypeBinder(commandFactory.TypeOption, commandFactory.AllOption);

            // Act
            var actualContentType = binder.GetBoundValue(parseResult);

            // Assert
            Assert.That(actualContentType, Is.EqualTo(expectedContentType));
        }
        #endregion GetBoundValue
    }
}
