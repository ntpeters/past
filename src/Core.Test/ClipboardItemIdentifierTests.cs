namespace past.Core.Test
{
    public class ClipboardItemIdentifierTests
    {
        #region TryParse
        [Test]
        [TestCase("-1")]
        [TestCase("0")]
        [TestCase("1")]
        public void TryParse_WithValidIndex_ReturnsTrueWithExpectedIdentifier(string indexValue)
        {
            // Arrange
            var expectedIndex = int.Parse(indexValue);

            // Act
            var result = ClipboardItemIdentifier.TryParse(indexValue, out var identifier);

            // Assert
            Assert.That(result, Is.True);
            Assert.That(identifier, Is.Not.Null);
            Assert.That(identifier.TryGetAsIndex(out var actualIndex), Is.True);
            Assert.That(actualIndex, Is.EqualTo(expectedIndex));
        }

        [Test]
        [TestCase("bc65714b-1c69-448b-967f-7925c6970db5")]
        [TestCase("EB368F36-69CB-4620-AAD4-DF9D37BC7C09")]
        [TestCase("{10b52d39-9b75-4b6b-81ee-212e7d309bb2}")]
        [TestCase("6bc476c3c29f42b38e8b7e34017689c8")]
        public void TryParse_WithValidGuid_ReturnsTrueWithExpectedIdentifier(string guidValue)
        {
            // Arrange
            var expectedGuid = Guid.Parse(guidValue);

            // Act
            var result = ClipboardItemIdentifier.TryParse(guidValue, out var identifier);

            // Assert
            Assert.That(result, Is.True);
            Assert.That(identifier, Is.Not.Null);
            Assert.That(identifier.TryGetAsGuid(out var actualGuid), Is.True);
            Assert.That(actualGuid, Is.EqualTo(expectedGuid));
        }

        [Test]
        [TestCase("")]
        [TestCase("abcdef")]
        [TestCase("42.42")]
        [TestCase(null)]
        public void TryParse_WithInvalid_ReturnsFalseWithNullIdentifier(string? invalidValue)
        {
            // Act
            var result = ClipboardItemIdentifier.TryParse(invalidValue!, out var identifier);

            // Assert
            Assert.That(result, Is.False);
            Assert.That(identifier, Is.Null);
        }
        #endregion TryParse

        #region Constructors
        [Test]
        [TestCase(-1)]
        [TestCase(0)]
        [TestCase(1)]
        public void Constructor_CreateWithIndex_Success(int index)
        {
            Assert.DoesNotThrow(() => new ClipboardItemIdentifier(index));
        }

        [Test]
        public void Constructor_CreateWithIndex_Success()
        {
            Assert.DoesNotThrow(() => new ClipboardItemIdentifier(Guid.NewGuid()));
        }
        #endregion Constructors

        #region TryGetAsIndex
        [Test]
        public void TryGetAsIndex_CreatedWithIndex_ReturnsTrueWithExpectedIndex()
        {
            // Arrange
            var expectedIndex = 42;
            var identifier = new ClipboardItemIdentifier(expectedIndex);

            // Act
            var result = identifier.TryGetAsIndex(out var actualIndex);

            // Assert
            Assert.That(result, Is.True);
            Assert.That(actualIndex, Is.EqualTo(expectedIndex));
        }

        [Test]
        public void TryGetAsIndex_CreatedWithGuid_ReturnsFalseWithNullIndex()
        {
            // Arrange
            var identifier = new ClipboardItemIdentifier(Guid.NewGuid());

            // Act
            var result = identifier.TryGetAsIndex(out var actualIndex);

            // Assert
            Assert.That(result, Is.False);
            Assert.That(actualIndex, Is.Null);
        }
        #endregion TryGetAsIndex

        #region TryGetAsGuid
        [Test]
        public void TryGetAsGuid_CreatedWithGuid_ReturnsTrueWithExpectedGuid()
        {
            // Arrange
            var expectedGuid = Guid.NewGuid();
            var identifier = new ClipboardItemIdentifier(expectedGuid);

            // Act
            var result = identifier.TryGetAsGuid(out var actualGuid);

            // Assert
            Assert.That(result, Is.True);
            Assert.That(actualGuid, Is.EqualTo(expectedGuid));
        }

        [Test]
        public void TryGetAsGuid_CreatedWithIndex_ReturnsFalseWithNullGuid()
        {
            // Arrange
            var identifier = new ClipboardItemIdentifier(0);

            // Act
            var result = identifier.TryGetAsGuid(out var actualGuid);

            // Assert
            Assert.That(result, Is.False);
            Assert.That(actualGuid, Is.Null);
        }
        #endregion TryGetAsGuid

        #region Implicit Cast
        [Test]
        public void ImplicitCase_FromInt_Success()
        {
            // Arrange
            int expectedIndex = 42;

            // Act
            ClipboardItemIdentifier identifier = expectedIndex;

            // Assert
            Assert.That(identifier.TryGetAsIndex(out var actualIndex), Is.True);
            Assert.That(actualIndex, Is.EqualTo(expectedIndex));
        }

        [Test]
        public void ImplicitCase_FromGuid_Success()
        {
            // Arrange
            Guid expectedId = Guid.NewGuid();

            // Act
            ClipboardItemIdentifier identifier = expectedId;

            // Assert
            Assert.That(identifier.TryGetAsGuid(out var actualId), Is.True);
            Assert.That(actualId, Is.EqualTo(expectedId));
        }
        #endregion Implicit Cast

        #region GetHashCode
        [Test]
        public void GetHashCode_CreatedWithIndex_ReturnsExpectedHashCode()
        {
            // Arrange
            int index = 42;
            Guid? id = null;
            var identifier = new ClipboardItemIdentifier(index);
            var expectedHashCode = (index, id).GetHashCode();

            // Act
            var actualHashCode = identifier.GetHashCode();

            // Assert
            Assert.That(actualHashCode, Is.EqualTo(expectedHashCode));
        }

        [Test]
        public void GetHashCode_CreatedWithGuid_ReturnsExpectedHashCode()
        {
            // Arrange
            int? index = null;
            Guid id = Guid.NewGuid();
            var identifier = new ClipboardItemIdentifier(id);
            var expectedHashCode = (index, id).GetHashCode();

            // Act
            var actualHashCode = identifier.GetHashCode();

            // Assert
            Assert.That(actualHashCode, Is.EqualTo(expectedHashCode));
        }
        #endregion GetHashCode

        // Disable NUnit EqualConstraint warning for equality tests, since the Equals methods are the code under test
#pragma warning disable NUnit2010 // Use EqualConstraint for better assertion messages in case of failure
        #region Equals(ClipboardItemIdentifier)
        [Test]
        public void Equals_SameInstance_ReturnsTrue()
        {
            // Arrange
            var identifier = new ClipboardItemIdentifier(0);

            // Act + Assert
            Assert.That(identifier.Equals(identifier), Is.True);
        }

        [Test]
        public void Equals_WithMatchingIndex_ReturnsTrue()
        {
            // Arrange
            int index = 42;
            var identifier1 = new ClipboardItemIdentifier(index);
            var identifier2 = new ClipboardItemIdentifier(index);

            // Act + Assert
            Assert.That(identifier1.Equals(identifier2), Is.True);
            Assert.That(identifier2.Equals(identifier1), Is.True);
        }

        [Test]
        public void Equals_WithMatchingGuid_ReturnsTrue()
        {
            // Arrange
            Guid guid = Guid.NewGuid();
            var identifier1 = new ClipboardItemIdentifier(guid);
            var identifier2 = new ClipboardItemIdentifier(guid);

            // Act + Assert
            Assert.That(identifier1.Equals(identifier2), Is.True);
            Assert.That(identifier2.Equals(identifier1), Is.True);
        }

        [Test]
        public void Equals_WithNonMatchingIndex_ReturnsFalse()
        {
            // Arrange
            var identifier1 = new ClipboardItemIdentifier(42);
            var identifier2 = new ClipboardItemIdentifier(9001);

            // Act + Assert
            Assert.That(identifier1.Equals(identifier2), Is.False);
            Assert.That(identifier2.Equals(identifier1), Is.False);
        }

        [Test]
        public void Equals_WithNonMatchingGuid_ReturnsFalse()
        {
            // Arrange
            var identifier1 = new ClipboardItemIdentifier(Guid.NewGuid());
            var identifier2 = new ClipboardItemIdentifier(Guid.NewGuid());

            // Act + Assert
            Assert.That(identifier1.Equals(identifier2), Is.False);
            Assert.That(identifier2.Equals(identifier1), Is.False);
        }

        [Test]
        public void Equals_Null_ReturnsFalse()
        {
            // Arrange
            var identifier = new ClipboardItemIdentifier(0);

            // Act + Assert
            Assert.That(identifier.Equals(null), Is.False);
        }
        #endregion Equals(ClipboardItemIdentifier)

        #region Equals(object)
        [Test]
        public void EqualsObject_SameInstance_ReturnsTrue()
        {
            // Arrange
            var identifier = new ClipboardItemIdentifier(0);

            // Act + Assert
            Assert.That(identifier.Equals((object)identifier), Is.True);
        }

        [Test]
        public void EqualsObject_WithMatchingIndex_ReturnsTrue()
        {
            // Arrange
            int index = 42;
            var identifier1 = new ClipboardItemIdentifier(index);
            var identifier2 = new ClipboardItemIdentifier(index);

            // Act + Assert
            Assert.That(identifier1.Equals((object)identifier2), Is.True);
            Assert.That(identifier2.Equals((object)identifier1), Is.True);
        }

        [Test]
        public void EqualsObject_WithMatchingGuid_ReturnsTrue()
        {
            // Arrange
            Guid guid = Guid.NewGuid();
            var identifier1 = new ClipboardItemIdentifier(guid);
            var identifier2 = new ClipboardItemIdentifier(guid);

            // Act + Assert
            Assert.That(identifier1.Equals((object)identifier2), Is.True);
            Assert.That(identifier2.Equals((object)identifier1), Is.True);
        }

        [Test]
        public void EqualsObject_WithNonMatchingIndex_ReturnsFalse()
        {
            // Arrange
            var identifier1 = new ClipboardItemIdentifier(42);
            var identifier2 = new ClipboardItemIdentifier(9001);

            // Act + Assert
            Assert.That(identifier1.Equals((object)identifier2), Is.False);
            Assert.That(identifier2.Equals((object)identifier1), Is.False);
        }

        [Test]
        public void EqualsObject_WithNonMatchingGuid_ReturnsFalse()
        {
            // Arrange
            var identifier1 = new ClipboardItemIdentifier(Guid.NewGuid());
            var identifier2 = new ClipboardItemIdentifier(Guid.NewGuid());

            // Act + Assert
            Assert.That(identifier1.Equals((object)identifier2), Is.False);
            Assert.That(identifier2.Equals((object)identifier1), Is.False);
        }

        [Test]
        public void EqualsObject_Null_ReturnsFalse()
        {
            // Arrange
            var identifier = new ClipboardItemIdentifier(0);

            // Act + Assert
            Assert.That(identifier.Equals((object?)null), Is.False);
        }
        #endregion Equals(object)

        #region Static Equals
        [Test]
        public void StaticEquals_SameInstance_ReturnsTrue()
        {
            // Arrange
            var identifier = new ClipboardItemIdentifier(0);

            // Act + Assert
            Assert.That(Equals(identifier, identifier), Is.True);
        }

        [Test]
        public void StaticEquals_WithMatchingIndex_ReturnsTrue()
        {
            // Arrange
            int index = 42;
            var identifier1 = new ClipboardItemIdentifier(index);
            var identifier2 = new ClipboardItemIdentifier(index);

            // Act + Assert
            Assert.That(Equals(identifier1, identifier2), Is.True);
            Assert.That(Equals(identifier2, identifier1), Is.True);
        }

        [Test]
        public void StaticEquals_WithMatchingGuid_ReturnsTrue()
        {
            // Arrange
            Guid guid = Guid.NewGuid();
            var identifier1 = new ClipboardItemIdentifier(guid);
            var identifier2 = new ClipboardItemIdentifier(guid);

            // Act + Assert
            Assert.That(Equals(identifier1, identifier2), Is.True);
            Assert.That(Equals(identifier2, identifier1), Is.True);
        }

        [Test]
        public void StaticEquals_WithNonMatchingIndex_ReturnsFalse()
        {
            // Arrange
            var identifier1 = new ClipboardItemIdentifier(42);
            var identifier2 = new ClipboardItemIdentifier(9001);

            // Act + Assert
            Assert.That(Equals(identifier1, identifier2), Is.False);
            Assert.That(Equals(identifier2, identifier1), Is.False);
        }

        [Test]
        public void StaticEquals_WithNonMatchingGuid_ReturnsFalse()
        {
            // Arrange
            var identifier1 = new ClipboardItemIdentifier(Guid.NewGuid());
            var identifier2 = new ClipboardItemIdentifier(Guid.NewGuid());

            // Act + Assert
            Assert.That(Equals(identifier1, identifier2), Is.False);
            Assert.That(Equals(identifier2, identifier1), Is.False);
        }

        [Test]
        public void StaticEquals_OneObjectNull_ReturnsFalse()
        {
            // Arrange
            var identifier = new ClipboardItemIdentifier(0);

            // Act + Assert
            Assert.That(Equals(identifier, null), Is.False);
            Assert.That(Equals(null, identifier), Is.False);
        }

        [Test]
        public void StaticEquals_BothObjectsNull_ReturnsTrue()
        {
            Assert.That(Equals(null, null), Is.True);
        }
        #endregion Static Equals

        #region Equals Operator
        [Test]
        public void EqualsOperator_SameInstance_ReturnsTrue()
        {
            // Arrange
            var identifier = new ClipboardItemIdentifier(0);

            // Act + Assert
            Assert.That(identifier == identifier, Is.True);
        }

        [Test]
        public void EqualsOperator_WithMatchingIndex_ReturnsTrue()
        {
            // Arrange
            int index = 42;
            var identifier1 = new ClipboardItemIdentifier(index);
            var identifier2 = new ClipboardItemIdentifier(index);

            // Act + Assert
            Assert.That(identifier1 == identifier2, Is.True);
            Assert.That(identifier2 == identifier1, Is.True);
        }

        [Test]
        public void EqualsOperator_WithMatchingGuid_ReturnsTrue()
        {
            // Arrange
            Guid guid = Guid.NewGuid();
            var identifier1 = new ClipboardItemIdentifier(guid);
            var identifier2 = new ClipboardItemIdentifier(guid);

            // Act + Assert
            Assert.That(identifier1 == identifier2, Is.True);
            Assert.That(identifier2 == identifier1, Is.True);
        }

        [Test]
        public void EqualsOperator_WithNonMatchingIndex_ReturnsFalse()
        {
            // Arrange
            var identifier1 = new ClipboardItemIdentifier(42);
            var identifier2 = new ClipboardItemIdentifier(9001);

            // Act + Assert
            Assert.That(identifier1 == identifier2, Is.False);
            Assert.That(identifier2 == identifier1, Is.False);
        }

        [Test]
        public void EqualsOperator_WithNonMatchingGuid_ReturnsFalse()
        {
            // Arrange
            var identifier1 = new ClipboardItemIdentifier(Guid.NewGuid());
            var identifier2 = new ClipboardItemIdentifier(Guid.NewGuid());

            // Act + Assert
            Assert.That(identifier1 == identifier2, Is.False);
            Assert.That(identifier2 == identifier1, Is.False);
        }

        [Test]
        public void EqualsOperator_OneObjectNull_ReturnsFalse()
        {
            // Arrange
            var identifier = new ClipboardItemIdentifier(0);

            // Act + Assert
            Assert.That(identifier == null, Is.False);
            Assert.That(null == identifier, Is.False);
        }

        [Test]
        public void EqualsOperator_BothObjectsNull_ReturnsTrue()
        {
            Assert.That(null == (ClipboardItemIdentifier?)null, Is.True);
        }
        #endregion Equals Operator

        #region Not Equal Operator
        [Test]
        public void NotEqualOperator_SameInstance_ReturnsFalse()
        {
            // Arrange
            var identifier = new ClipboardItemIdentifier(0);

            // Act + Assert
            Assert.That(identifier != identifier, Is.False);
        }

        [Test]
        public void NotEqualOperator_WithMatchingIndex_ReturnsFalse()
        {
            // Arrange
            int index = 42;
            var identifier1 = new ClipboardItemIdentifier(index);
            var identifier2 = new ClipboardItemIdentifier(index);

            // Act + Assert
            Assert.That(identifier1 != identifier2, Is.False);
            Assert.That(identifier2 != identifier1, Is.False);
        }

        [Test]
        public void NotEqualOperator_WithMatchingGuid_ReturnsFalse()
        {
            // Arrange
            Guid guid = Guid.NewGuid();
            var identifier1 = new ClipboardItemIdentifier(guid);
            var identifier2 = new ClipboardItemIdentifier(guid);

            // Act + Assert
            Assert.That(identifier1 != identifier2, Is.False);
            Assert.That(identifier2 != identifier1, Is.False);
        }

        [Test]
        public void NotEqualOperator_WithNonMatchingIndex_ReturnsTrue()
        {
            // Arrange
            var identifier1 = new ClipboardItemIdentifier(42);
            var identifier2 = new ClipboardItemIdentifier(9001);

            // Act + Assert
            Assert.That(identifier1 != identifier2, Is.True);
            Assert.That(identifier2 != identifier1, Is.True);
        }

        [Test]
        public void NotEqualOperator_WithNonMatchingGuid_ReturnsTrue()
        {
            // Arrange
            var identifier1 = new ClipboardItemIdentifier(Guid.NewGuid());
            var identifier2 = new ClipboardItemIdentifier(Guid.NewGuid());

            // Act + Assert
            Assert.That(identifier1 != identifier2, Is.True);
            Assert.That(identifier2 != identifier1, Is.True);
        }

        [Test]
        public void NotEqualOperator_OneObjectNull_ReturnsTrue()
        {
            // Arrange
            var identifier = new ClipboardItemIdentifier(0);

            // Act + Assert
            Assert.That(identifier != null, Is.True);
            Assert.That(null != identifier, Is.True);
        }

        [Test]
        public void NotEqualOperator_BothObjectsNull_ReturnsFalse()
        {
            Assert.That(null != (ClipboardItemIdentifier?)null, Is.False);
        }
        #endregion Not Equal Operator
#pragma warning restore NUnit2010 // Use EqualConstraint for better assertion messages in case of failure
    }
}
