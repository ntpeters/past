using past.Core.Extensions;
using Windows.ApplicationModel.DataTransfer;

namespace past.Core.Test.Extensions
{
    public class ContentTypeExtensionsTests
    {
        #region TryToStandardDataFormat
        [Test]
        public void TryToStandardDataFormat_TextContentType_ReturnsTrueAndTextFormatId()
        {
            // Arrange
            var expectedFormatIds = new List<string> { StandardDataFormats.Text };
            var type = ContentType.Text;

            // Act
            var result = type.TryToStandardDataFormat(out var actualFormatIds);

            // Assert
            Assert.That(result, Is.True);
            Assert.That(actualFormatIds, Is.EqualTo(expectedFormatIds));
        }

        [Test]
        public void TryToStandardDataFormat_ImageContentType_ReturnsTrueAndBitmapFormatId()
        {
            // Arrange
            var expectedFormatIds = new List<string> { StandardDataFormats.Bitmap };
            var type = ContentType.Image;

            // Act
            var result = type.TryToStandardDataFormat(out var actualFormatIds);

            // Assert
            Assert.That(result, Is.True);
            Assert.That(actualFormatIds, Is.EqualTo(expectedFormatIds));
        }

        [Test]
        public void TryToStandardDataFormat_AllContentType_ReturnsTrueAndAllFormatIds()
        {
            // Arrange
            var expectedFormatIds = new List<string>
            {
                StandardDataFormats.Text,
                StandardDataFormats.Bitmap
            };
            var type = ContentType.All;

            // Act
            var result = type.TryToStandardDataFormat(out var actualFormatIds);

            // Assert
            Assert.That(result, Is.True);
            Assert.That(actualFormatIds, Is.EqualTo(expectedFormatIds));
        }

        [Test]
        public void TryToStandardDataFormat_InvalidContentType_ReturnsFalseAndNullFormatIds()
        {
            // Arrange
            ContentType type = 0;

            // Act
            var result = type.TryToStandardDataFormat(out var actualFormatIds);

            // Assert
            Assert.That(result, Is.False);
            Assert.That(actualFormatIds, Is.Null);
        }
        #endregion TryToStandardDataFormat

        #region Supports
        [Test]
        public void Supports_TextContentType_PredicateReceivesTextFormatId()
        {
            // Arrange
            var expectedFormatId = StandardDataFormats.Text;
            bool expectedResult = true;
            var type = ContentType.Text;

            // Act
            string actualFormatId = string.Empty;
            var actualResult = type.Supports(formatId =>
            {
                actualFormatId = formatId;
                return expectedResult;
            });

            // Assert
            Assert.That(actualResult, Is.EqualTo(expectedResult));
            Assert.That(actualFormatId, Is.EqualTo(expectedFormatId));
        }

        [Test]
        public void Supports_ImageContentType_PredicateReceivesBitmapFormatId()
        {
            // Arrange
            var expectedFormatId = StandardDataFormats.Bitmap;
            bool expectedResult = true;
            var type = ContentType.Image;

            // Act
            string actualFormatId = string.Empty;
            var actualResult = type.Supports(formatId =>
            {
                actualFormatId = formatId;
                return expectedResult;
            });

            // Assert
            Assert.That(actualResult, Is.EqualTo(expectedResult));
            Assert.That(actualFormatId, Is.EqualTo(expectedFormatId));
        }

        [Test]
        public void Supports_AllContentType_ReturnsTrue()
        {
            // Arrange
            var type = ContentType.All;

            // Act
            string? actualFormatId = null;
            var actualResult = type.Supports(formatId =>
            {
                actualFormatId = formatId;
                return false;
            });

            // Assert
            Assert.That(actualResult, Is.True);
            Assert.That(actualFormatId, Is.Null);
        }

        [Test]
        public void Supports_NullPredicate_ThrowArgumentNullException()
        {
            // Arrange
            var type = ContentType.All;

            // Act + Assert
            Assert.Throws<ArgumentNullException>(() => type.Supports(null!));
        }
        #endregion Supports
    }
}
