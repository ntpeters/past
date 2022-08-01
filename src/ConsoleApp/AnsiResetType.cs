namespace past.ConsoleApp
{
    /// <summary>
    /// Controls the output of ANSI reset after values written to the console.
    /// </summary>
    public enum AnsiResetType
    {
        /// <summary>
        /// Automatically determine whether to emit ANSI reset.
        /// </summary>
        Auto,

        /// <summary>
        /// Always emit ANSI reset.
        /// </summary>
        On,

        /// <summary>
        /// Never emit ANSI reset.
        /// </summary>
        Off
    }
}
