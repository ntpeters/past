namespace past.Core
{
    public class PastException : Exception
    {
        /// <summary>
        /// Represents errors that occur while processing clipboard operations.
        /// </summary>
        public ErrorCode ErrorCode { get; }

        /// <summary>
        /// Creates a new <see cref="PastException"/> with the given error code and message.
        /// </summary>
        /// <param name="errorCode"><see cref="ErrorCode"/> that represents the error.</param>
        /// <param name="message">The message that describes the error.</param>
        /// <exception cref="ArgumentException"><paramref name="errorCode"/> is a value not defined by <see cref="ErrorCode"/>.</exception>
        public PastException(ErrorCode errorCode, string message)
            : base(message)
        {
            if (!Enum.IsDefined(errorCode))
            {
                throw new ArgumentException($"Invalid error code: {errorCode}", nameof(errorCode));
            }

            ErrorCode = errorCode;
        }
    }
}
