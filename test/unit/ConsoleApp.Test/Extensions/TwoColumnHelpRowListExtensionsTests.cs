using past.ConsoleApp.Extensions;
using past.ConsoleApp.Test.TestHelpers;
using past.Core;
using System.CommandLine.Help;

namespace past.ConsoleApp.Test.Extensions
{
    public class TwoColumnHelpRowListExtensionsTests
    {
        [Test]
        public void AddExitCodeHelpRow_WithErrorCode_AddsRowWithExpectedColumnValues([EnumValueSource] ErrorCode errorCode)
        {
            // Arrange
            var expectedFirstColumnText = errorCode.ToString();
            var expectedSecondColumnText = $"{(int)errorCode}";

            var list = new List<TwoColumnHelpRow>();

            // Act
            list.AddExitCodeHelpRow(errorCode);

            // Assert
            Assert.That(list, Has.Exactly(1).Items);

            var row = list[0];
            Assert.That(row.FirstColumnText, Is.EqualTo(expectedFirstColumnText));
            Assert.That(row.SecondColumnText, Is.EqualTo(expectedSecondColumnText));
        }

        [Test]
        public void AddExitCodeHelpRow_WithNameAndValue_AddsRowWithExpectedColumnValues()
        {
            // Arrange
            var expectedFirstColumnText = "Column One";
            var expectedSecondColumnText = "Column Two";

            var list = new List<TwoColumnHelpRow>();

            // Act
            list.AddExitCodeHelpRow(expectedFirstColumnText, expectedSecondColumnText);

            // Assert
            Assert.That(list, Has.Exactly(1).Items);

            var row = list[0];
            Assert.That(row.FirstColumnText, Is.EqualTo(expectedFirstColumnText));
            Assert.That(row.SecondColumnText, Is.EqualTo(expectedSecondColumnText));
        }
    }
}
