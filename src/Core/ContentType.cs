namespace past.Core
{
    /// <summary>
    /// Describes the type of content stored on the clipboard.
    /// </summary>
    [Flags]
    public enum ContentType
    {
        /// <summary>
        /// Text based content.
        /// </summary>
        Text = 1,

        /// <summary>
        /// Image based content.
        /// </summary>
        Image = 2,

        /// <summary>
        /// Represents all content types.
        /// </summary>
        All = Text | Image
    }
}
