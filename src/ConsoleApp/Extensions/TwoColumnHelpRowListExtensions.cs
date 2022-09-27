using past.Core;
using System.Collections.Generic;
using System.CommandLine.Help;

namespace past.ConsoleApp.Extensions
{
    /// <summary>
    /// Extensions for <see cref="IList{T}"/> of <see cref="TwoColumnHelpRow"/>.
    /// </summary>
    public static class TwoColumnHelpRowListExtensions
    {
        /// <summary>
        /// Adds a <see cref="TwoColumnHelpRow"/> to the list for the given <paramref name="errorCode"/>.
        /// </summary>
        /// <remarks>
        /// The name of the specified error code is used as the first column text and the error code value is used as the second column text.
        /// </remarks>
        /// <param name="list">List to add the help row to.</param>
        /// <param name="errorCode">Error code to add a help row for.</param>
        public static void AddExitCodeHelpRow(this IList<TwoColumnHelpRow> list, ErrorCode errorCode)
            => AddExitCodeHelpRow(list, errorCode.ToString(), $"{(int)errorCode}");

        /// <summary>
        /// Adds a <see cref="TwoColumnHelpRow"/> to the list for the given <paramref name="name"/> and <paramref name="value"/>.
        /// </summary>
        /// <param name="list">List to add the help to.</param>
        /// <param name="name">Name to use for the first column text.</param>
        /// <param name="value">Value to use for the second column text.</param>
        public static void AddExitCodeHelpRow(this IList<TwoColumnHelpRow> list, string name, string value)
            => list.Add(new TwoColumnHelpRow(name, value));
    }
}
