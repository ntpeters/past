using past.ConsoleApp.Binders;
using past.Core;
using System.CommandLine;
using System.CommandLine.Parsing;

namespace past.ConsoleApp.Test.Binders
{
    public class ContentTypeBinderTests
    {
        #region Constructor
        [Test]
        public void Constructor_NonNullParameters_Success()
        {
            var typeOption = new Option<ContentType>("--type");
            var allOption = new Option<bool>("--all");

            Assert.DoesNotThrow(() => new ContentTypeBinder(typeOption, allOption));
        }

        [Test]
        public void Constructor_NullTypeOption_ThrowsArgumentNullException()
        {
            var typeOption = new Option<ContentType>("--type");
            var allOption = new Option<bool>("--all");

            Assert.Throws<ArgumentNullException>(() => new ContentTypeBinder(null!, allOption));
        }

        [Test]
        public void Constructor_NullAllOption_ThrowsArgumentNullException()
        {
            var typeOption = new Option<ContentType>("--type");
            var allOption = new Option<bool>("--all");

            Assert.Throws<ArgumentNullException>(() => new ContentTypeBinder(typeOption, null!));
        }
        #endregion Constructor

        #region GetBoundValue
        [Test]
        public void GetBoundValue_OnlyAllOptionProvided_ReturnsAllContentType()
        {
            // Arrange
            var expectedContentType = ContentType.All;
            var typeOption = new Option<ContentType>("--type");
            var allOption = new Option<bool>("--all");

            var testCommand = new Command("test");
            testCommand.AddOption(typeOption);
            testCommand.AddOption(allOption);

            var testCommandParser = new Parser(testCommand);
            var parseResult = testCommandParser.Parse("--all");

            var binder = new ContentTypeBinder(typeOption, allOption);

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
            var typeOption = new Option<ContentType>("--type");
            var allOption = new Option<bool>("--all");

            var testCommand = new Command("test");
            testCommand.AddOption(typeOption);
            testCommand.AddOption(allOption);

            var testCommandParser = new Parser(testCommand);
            var parseResult = testCommandParser.Parse($"--all --type {type.ToString()}");

            var binder = new ContentTypeBinder(typeOption, allOption);

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
            var typeOption = new Option<ContentType>("--type");
            var allOption = new Option<bool>("--all");

            var testCommand = new Command("test");
            testCommand.AddOption(typeOption);
            testCommand.AddOption(allOption);

            var testCommandParser = new Parser(testCommand);
            var parseResult = testCommandParser.Parse($"--type {expectedContentType.ToString()}");

            var binder = new ContentTypeBinder(typeOption, allOption);

            // Act
            var actualContentType = binder.GetBoundValue(parseResult);

            // Assert
            Assert.That(actualContentType, Is.EqualTo(expectedContentType));
        }

        [Test]
        public void GetBoundValue_InvalidParseResult_ThrowsArgumentException()
        {
            // Arrange
            var expectedExceptionMessage = "Failed to bind content type";
            var typeOption = new Option<ContentType>("--type");
            var allOption = new Option<bool>("--all");

            var testCommand = new Command("test");
            var testCommandParser = new Parser(testCommand);
            var parseResult = testCommandParser.Parse(string.Empty);

            var binder = new ContentTypeBinder(typeOption, allOption);

            // Act + Assert
            var actualException = Assert.Throws<ArgumentException>(() => binder.GetBoundValue(parseResult));
            Assert.That(actualException.Message, Is.EqualTo(expectedExceptionMessage));
        }
        #endregion GetBoundValue
    }
}
