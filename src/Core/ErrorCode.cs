namespace past.Core
{
    /// <summary>
    /// Describes an error that occurred while processing an operation.
    /// </summary>
    public enum ErrorCode
    {
        /// <summary>
        /// Access was denied to a resource while processing the operation.
        /// </summary>
        AccessDenied = -5,

        /// <summary>
        /// The clipboard history system setting is disabled.
        /// </summary>
        ClipboardHistoryDisabled = -4,

        /// <summary>
        /// The requested item(s) are not compatible with the specified content type.
        /// </summary>
        IncompatibleContentType = -3,

        /// <summary>
        /// The requested item(s) could not be found.
        /// </summary>
        NotFound = -2,

        /// <summary>
        /// Failed to parse the provided commandline parameters.
        /// </summary>
        ParseError = -1,

        /// <summary>
        /// The operation completed successfully.
        /// </summary>
        Success = 0
    }
}
