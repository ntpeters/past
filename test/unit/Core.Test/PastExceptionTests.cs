namespace past.Core.Test
{
    public class PastExceptionTests
    {
        [Test]
        public void Create_ValidParameters_Success([ValueSource(nameof(ErrorCodeValueSource))] ErrorCode expectedErrorCode)
        {
            var expectedMessage = "Well there's your problem";
            PastException? actualException = null;
            Assert.DoesNotThrow(() => actualException = new PastException(expectedErrorCode, expectedMessage));
            Assert.That(actualException, Is.Not.Null);
            Assert.That(actualException.Message, Is.EqualTo(expectedMessage));
            Assert.That(actualException.ErrorCode, Is.EqualTo(expectedErrorCode));
        }

        [Test]
        public void Create_InvalidErrorCode_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new PastException((ErrorCode)int.MaxValue, "Well there's your problem"));
        }

        #region Helpers
        public static IEnumerable<ErrorCode> ErrorCodeValueSource()
        {
            foreach (var errorCode in Enum.GetValues<ErrorCode>())
            {
                yield return errorCode;
            }
        }
        #endregion Helpers
    }
}
