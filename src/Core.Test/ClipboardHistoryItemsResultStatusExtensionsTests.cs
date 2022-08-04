using past.Core.Extensions;
using Windows.ApplicationModel.DataTransfer;

namespace past.Core.Test
{
    public class ClipboardHistoryItemsResultStatusExtensionsTests
    {
        [Test]
        [TestCase(ClipboardHistoryItemsResultStatus.Success, ErrorCode.Success)]
        [TestCase(ClipboardHistoryItemsResultStatus.AccessDenied, ErrorCode.AccessDenied)]
        [TestCase(ClipboardHistoryItemsResultStatus.ClipboardHistoryDisabled, ErrorCode.ClipboardHistoryDisabled)]
        public void ToErrorCode_DefinedValue_ReturnsExpectedValue(ClipboardHistoryItemsResultStatus resultStatus, ErrorCode expectedErrorCode)
        {
            var actualErrorCode = resultStatus.ToErrorCode();
            Assert.That(actualErrorCode, Is.EqualTo(expectedErrorCode));
        }

        [Test]
        public void ToErrorCode_UndefinedValue_ReturnsSuccess()
        {
            var undefinedResultStatus = (ClipboardHistoryItemsResultStatus)int.MaxValue;
            var actualErrorCode = undefinedResultStatus.ToErrorCode();
            Assert.That(actualErrorCode, Is.EqualTo(ErrorCode.Success));
        }
    }
}
