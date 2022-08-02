using past.ConsoleApp.Wrappers;

namespace past.ConsoleApp.Test
{
    public class ValueFormatterTests
    {
        #region Constructor
        [Test]
        [TestCaseSource(nameof(ConstructorTestCases))]
        public void Create_ValidParameters_Success(bool nullLineEnding, bool includeIndex, bool includeId, bool includeTimestamp)
        {
            IValueFormatter? createdInstance = null;
            Assert.DoesNotThrow(() => createdInstance = new ValueFormatter(nullLineEnding, includeIndex, includeId, includeTimestamp));

            Assert.That(createdInstance, Is.Not.Null);
            Assert.That(createdInstance.NullLineEnding, Is.EqualTo(nullLineEnding));
            Assert.That(createdInstance.IncludeIndex, Is.EqualTo(includeIndex));
            Assert.That(createdInstance.IncludeId, Is.EqualTo(includeId));
            Assert.That(createdInstance.IncludeTimestamp, Is.EqualTo(includeTimestamp));
        }
        #endregion Constructor

        #region Format - With ANSI Reset and Line Ending
        [Test]
        public void Format_NoAnsiResetOrLineEnding_ReturnsOriginalValue()
        {
            // Arrange
            var expectedValue = "heyo";

            var formatter = new ValueFormatter(
                nullLineEnding: true,
                includeIndex: true,
                includeId: true,
                includeTimestamp: true);

            // Act
            var actualValue = formatter.Format(expectedValue, emitAnsiReset: false, emitLineEnding: false);

            // Assert
            Assert.That(actualValue, Is.EqualTo(expectedValue));
        }

        [Test]
        public void Format_AnsiResetOnly_ReturnsValueWithAnsiReset()
        {
            // Arrange
            var originalValue = "heyo";
            var expectedValue = $"{originalValue}{NativeConstants.ANSI_RESET}";

            var formatter = new ValueFormatter(
                nullLineEnding: true,
                includeIndex: true,
                includeId: true,
                includeTimestamp: true);

            // Act
            var actualValue = formatter.Format(originalValue, emitAnsiReset: true, emitLineEnding: false);

            // Assert
            Assert.That(actualValue, Is.EqualTo(expectedValue));
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void Format_LineEndingOnly_ReturnsValueWithExpectedLineEnding(bool nullLineEnding)
        {
            // Arrange
            var originalValue = "heyo";
            var expectedValue = $"{originalValue}{(nullLineEnding ? '\0' : Environment.NewLine)}";

            var formatter = new ValueFormatter(
                nullLineEnding,
                includeIndex: true,
                includeId: true,
                includeTimestamp: true);

            // Act
            var actualValue = formatter.Format(originalValue, emitAnsiReset: false, emitLineEnding: true);

            // Assert
            Assert.That(actualValue, Is.EqualTo(expectedValue));
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void Format_AnsiResetAndLineEnding_ReturnsValueWithAnsiResetAndExpectedLineEnding(bool nullLineEnding)
        {
            // Arrange
            var originalValue = "heyo";
            var expectedValue = $"{originalValue}{NativeConstants.ANSI_RESET}{(nullLineEnding ? '\0' : Environment.NewLine)}";

            var formatter = new ValueFormatter(
                nullLineEnding,
                includeIndex: true,
                includeId: true,
                includeTimestamp: true);

            // Act
            var actualValue = formatter.Format(originalValue, emitAnsiReset: true, emitLineEnding: true);

            // Assert
            Assert.That(actualValue, Is.EqualTo(expectedValue));
        }
        #endregion Format - With ANSI Reset and Line Ending

        #region Format - With index, ID, timestamp, ANSI Reset, and Line Ending
        [Test]
        public void FormatWithArgs_AllOptionsDisabled_ReturnsOriginalValue()
        {
            // Arrange
            var expectedValue = "heyo";
            var expectedIndex = 0;
            var expectedId = Guid.NewGuid().ToString();
            var expectedTimestamp = DateTimeOffset.Now;
            var expectedEmitAnsiReset = false;
            var expectedEmitLineEnding = false;

            var formatter = new ValueFormatter(
                nullLineEnding: false,
                includeIndex: false,
                includeId: false,
                includeTimestamp: false);

            // Act
            var actualValue = formatter.Format(
                expectedValue,
                expectedIndex,
                expectedId,
                expectedTimestamp,
                expectedEmitAnsiReset,
                expectedEmitLineEnding);

            // Assert
            Assert.That(actualValue, Is.EqualTo(expectedValue));
        }

        [Test]
        [TestCase(true, true)]
        [TestCase(true, false)]
        [TestCase(false, true)]
        [TestCase(false, false)]
        public void FormatWithArgs_IndexOnly_ReturnsValueWithIndex(bool expectedEmitAnsiReset, bool expectedEmitLineEnding)
        {
            // Arrange
            var originalValue = "heyo";
            var expectedIndex = 0;
            var expectedId = Guid.NewGuid().ToString();
            var expectedTimestamp = DateTimeOffset.Now;

            var expectedValue = $"{expectedIndex}:{originalValue}";
            if (expectedEmitAnsiReset)
            {
                expectedValue = $"{expectedValue}{NativeConstants.ANSI_RESET}";
            }
            if (expectedEmitLineEnding)
            {
                expectedValue = $"{expectedValue}{Environment.NewLine}";
            }

            var formatter = new ValueFormatter(
                nullLineEnding: false,
                includeIndex: true,
                includeId: false,
                includeTimestamp: false);

            // Act
            var actualValue = formatter.Format(
                originalValue,
                expectedIndex,
                expectedId,
                expectedTimestamp,
                expectedEmitAnsiReset,
                expectedEmitLineEnding);

            // Assert
            Assert.That(actualValue, Is.EqualTo(expectedValue));
        }

        [Test]
        [TestCase(true, true)]
        [TestCase(true, false)]
        [TestCase(false, true)]
        [TestCase(false, false)]
        public void FormatWithArgs_IndexEnabledWithNullIndex_ReturnsOriginalValue(bool expectedEmitAnsiReset, bool expectedEmitLineEnding)
        {
            // Arrange
            var originalValue = "heyo";
            int? expectedIndex = null;
            var expectedId = Guid.NewGuid().ToString();
            var expectedTimestamp = DateTimeOffset.Now;

            var expectedValue = originalValue;
            if (expectedEmitAnsiReset)
            {
                expectedValue = $"{expectedValue}{NativeConstants.ANSI_RESET}";
            }
            if (expectedEmitLineEnding)
            {
                expectedValue = $"{expectedValue}{Environment.NewLine}";
            }

            var formatter = new ValueFormatter(
                nullLineEnding: false,
                includeIndex: true,
                includeId: false,
                includeTimestamp: false);

            // Act
            var actualValue = formatter.Format(
                originalValue,
                expectedIndex,
                expectedId,
                expectedTimestamp,
                expectedEmitAnsiReset,
                expectedEmitLineEnding);

            // Assert
            Assert.That(actualValue, Is.EqualTo(expectedValue));
        }

        [Test]
        [TestCase(true, true)]
        [TestCase(true, false)]
        [TestCase(false, true)]
        [TestCase(false, false)]
        public void FormatWithArgs_IdOnly_ReturnsValueWithId(bool expectedEmitAnsiReset, bool expectedEmitLineEnding)
        {
            // Arrange
            var originalValue = "heyo";
            var expectedIndex = 0;
            var expectedId = Guid.NewGuid().ToString();
            var expectedTimestamp = DateTimeOffset.Now;

            var expectedValue = $"{expectedId}:{originalValue}";
            if (expectedEmitAnsiReset)
            {
                expectedValue = $"{expectedValue}{NativeConstants.ANSI_RESET}";
            }
            if (expectedEmitLineEnding)
            {
                expectedValue = $"{expectedValue}{Environment.NewLine}";
            }

            var formatter = new ValueFormatter(
                nullLineEnding: false,
                includeIndex: false,
                includeId: true,
                includeTimestamp: false);

            // Act
            var actualValue = formatter.Format(
                originalValue,
                expectedIndex,
                expectedId,
                expectedTimestamp,
                expectedEmitAnsiReset,
                expectedEmitLineEnding);

            // Assert
            Assert.That(actualValue, Is.EqualTo(expectedValue));
        }

        [Test]
        [TestCase(true, true)]
        [TestCase(true, false)]
        [TestCase(false, true)]
        [TestCase(false, false)]
        public void FormatWithArgs_TimestampOnly_ReturnsValueWithTimestamp(bool expectedEmitAnsiReset, bool expectedEmitLineEnding)
        {
            // Arrange
            var originalValue = "heyo";
            var expectedIndex = 0;
            var expectedId = Guid.NewGuid().ToString();
            var expectedTimestamp = DateTimeOffset.Now;

            var expectedValue = $"{expectedTimestamp}:{originalValue}";
            if (expectedEmitAnsiReset)
            {
                expectedValue = $"{expectedValue}{NativeConstants.ANSI_RESET}";
            }
            if (expectedEmitLineEnding)
            {
                expectedValue = $"{expectedValue}{Environment.NewLine}";
            }

            var formatter = new ValueFormatter(
                nullLineEnding: false,
                includeIndex: false,
                includeId: false,
                includeTimestamp: true);

            // Act
            var actualValue = formatter.Format(
                originalValue,
                expectedIndex,
                expectedId,
                expectedTimestamp,
                expectedEmitAnsiReset,
                expectedEmitLineEnding);

            // Assert
            Assert.That(actualValue, Is.EqualTo(expectedValue));
        }

        [Test]
        [TestCase(true, true)]
        [TestCase(true, false)]
        [TestCase(false, true)]
        [TestCase(false, false)]
        public void FormatWithArgs_AllOptionsEnabled_ReturnsValueWithAllValues(bool expectedEmitAnsiReset, bool expectedEmitLineEnding)
        {
            // Arrange
            var originalValue = "heyo";
            var expectedIndex = 0;
            var expectedId = Guid.NewGuid().ToString();
            var expectedTimestamp = DateTimeOffset.Now;

            var expectedValue = $"{expectedIndex}:{expectedId}:{expectedTimestamp}:{originalValue}";
            if (expectedEmitAnsiReset)
            {
                expectedValue = $"{expectedValue}{NativeConstants.ANSI_RESET}";
            }
            if (expectedEmitLineEnding)
            {
                expectedValue = $"{expectedValue}\0";
            }

            var formatter = new ValueFormatter(
                nullLineEnding: true,
                includeIndex: true,
                includeId: true,
                includeTimestamp: true);

            // Act
            var actualValue = formatter.Format(
                originalValue,
                expectedIndex,
                expectedId,
                expectedTimestamp,
                expectedEmitAnsiReset,
                expectedEmitLineEnding);

            // Assert
            Assert.That(actualValue, Is.EqualTo(expectedValue));
        }

        [Test]
        public void FormatWithArgs_AnsiResetOnly_ReturnsValueWithAnsiReset()
        {
            // Arrange
            var originalValue = "heyo";
            var expectedIndex = 0;
            var expectedId = Guid.NewGuid().ToString();
            var expectedTimestamp = DateTimeOffset.Now;
            var expectedEmitAnsiReset = true;
            var expectedEmitLineEnding = false;
            var expectedValue = $"{originalValue}{NativeConstants.ANSI_RESET}";

            var formatter = new ValueFormatter(
                nullLineEnding: false,
                includeIndex: false,
                includeId: false,
                includeTimestamp: false);

            // Act
            var actualValue = formatter.Format(
                originalValue,
                expectedIndex,
                expectedId,
                expectedTimestamp,
                expectedEmitAnsiReset,
                expectedEmitLineEnding);

            // Assert
            Assert.That(actualValue, Is.EqualTo(expectedValue));
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void FormatWithArgs_LineEndingOnly_ReturnsValueWithExpectedLineEnding(bool nullLineEnding)
        {
            // Arrange
            var originalValue = "heyo";
            var expectedIndex = 0;
            var expectedId = Guid.NewGuid().ToString();
            var expectedTimestamp = DateTimeOffset.Now;
            var expectedEmitAnsiReset = false;
            var expectedEmitLineEnding = true;
            var expectedValue = $"{originalValue}{(nullLineEnding ? '\0' : Environment.NewLine)}";

            var formatter = new ValueFormatter(
                nullLineEnding,
                includeIndex: false,
                includeId: false,
                includeTimestamp: false);

            // Act
            var actualValue = formatter.Format(
                originalValue,
                expectedIndex,
                expectedId,
                expectedTimestamp,
                expectedEmitAnsiReset,
                expectedEmitLineEnding);

            // Assert
            Assert.That(actualValue, Is.EqualTo(expectedValue));
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void FormatWithArgs_AnsiResetAndLineEnding_ReturnsValueWithAnsiResetAndExpectedLineEnding(bool nullLineEnding)
        {
            // Arrange
            var originalValue = "heyo";
            var expectedIndex = 0;
            var expectedId = Guid.NewGuid().ToString();
            var expectedTimestamp = DateTimeOffset.Now;
            var expectedEmitAnsiReset = true;
            var expectedEmitLineEnding = true;
            var expectedValue = $"{originalValue}{NativeConstants.ANSI_RESET}{(nullLineEnding ? '\0' : Environment.NewLine)}";

            var formatter = new ValueFormatter(
                nullLineEnding,
                includeIndex: false,
                includeId: false,
                includeTimestamp: false);

            // Act
            var actualValue = formatter.Format(
                originalValue,
                expectedIndex,
                expectedId,
                expectedTimestamp,
                expectedEmitAnsiReset,
                expectedEmitLineEnding);

            // Assert
            Assert.That(actualValue, Is.EqualTo(expectedValue));
        }
        #endregion Format - With index, ID, timestamp, ANSI Reset, and Line Ending

        #region Helpers
        public static IEnumerable<TestCaseData> ConstructorTestCases()
        {
            yield return new TestCaseData(true, true, true, true);
            yield return new TestCaseData(true, false, true, true);
            yield return new TestCaseData(true, false, false, true);
            yield return new TestCaseData(true, false, false, false);

            yield return new TestCaseData(false, true, true, true);
            yield return new TestCaseData(false, true, false, true);
            yield return new TestCaseData(false, true, false, false);

            yield return new TestCaseData(false, false, true, true);
            yield return new TestCaseData(false, false, true, false);

            yield return new TestCaseData(false, false, false, true);

            yield return new TestCaseData(false, false, false, false);
        }
        #endregion Helpers
    }
}
