using past.ConsoleApp.Wrappers;
using System;
using System.Text;

namespace past.ConsoleApp
{
    /// <inheritdoc cref="IValueFormatter"/>
    public class ValueFormatter : IValueFormatter
    {
        #region Public Properties
        public bool NullLineEnding { get; }
        public bool IncludeIndex { get; }
        public bool IncludeId { get; }
        public bool IncludeTimestamp { get; }
        #endregion Public Properties

        #region Constructor
        /// <summary>
        /// Creates a new <see cref="ValueFormatter"/> with the given options.
        /// </summary>
        /// <param name="nullLineEnding">Whether to use the null byte ('\0') as the line ending instead of a newline.</param>
        /// <param name="includeIndex">Whether to include the index in formatted values.</param>
        /// <param name="includeId">Whether to include the ID in formatted values.</param>
        /// <param name="includeTimestamp">Whether to include the timestamp in formatted values.</param>
        public ValueFormatter(bool nullLineEnding, bool includeIndex, bool includeId, bool includeTimestamp)
        {
            NullLineEnding = nullLineEnding;
            IncludeIndex = includeIndex;
            IncludeId = includeId;
            IncludeTimestamp = includeTimestamp;
        }
        #endregion Constructor

        #region Public Methods
        public string Format(string value, bool emitAnsiReset, bool emitLineEnding = false)
        {
            var outputValue = new StringBuilder();
            outputValue.Append(value);
            AppendAnsiResetAndLineEnding(outputValue, emitAnsiReset, emitLineEnding);
            return outputValue.ToString();

        }

        public string Format(string value, int? index, string id, DateTimeOffset timestamp, bool emitAnsiReset, bool emitLineEnding = false)
        {
            var outputValue = new StringBuilder();
            if (IncludeIndex && index != null)
            {
                outputValue.Append($"{index}:");
            }

            if (IncludeId)
            {
                outputValue.Append($"{id}:");
            }

            if (IncludeTimestamp)
            {
                outputValue.Append($"{timestamp}:");
            }

            outputValue.Append(value);

            AppendAnsiResetAndLineEnding(outputValue, emitAnsiReset, emitLineEnding);

            return outputValue.ToString();
        }
        #endregion Public Methods

        #region Private Methods
        /// <summary>
        /// Appends ANSI reset and the line ending, if specified, to the given <see cref="StringBuilder"/>.
        /// </summary>
        /// <remarks>
        /// The line ending will be the current environment's line ending unless this <see cref="NullLineEnding"/> is true.
        /// If <see cref="NullLineEnding"/> is true then the null byte ('\0') will be appended instead.
        /// </remarks>
        /// <param name="stringBuilder">The <see cref="StringBuilder"/> to append to.</param>
        /// <param name="emitAnsiReset">Whether to emit ANSI reset.</param>
        /// <param name="emitLineEnding">Whether to emit the line ending.</param>
        /// <returns>A reference to the given <paramref name="stringBuilder"/> after the append operations have completed.</returns>
        private StringBuilder AppendAnsiResetAndLineEnding(StringBuilder stringBuilder, bool emitAnsiReset, bool emitLineEnding)
        {
            if (emitAnsiReset)
            {
                stringBuilder.Append(NativeConstants.ANSI_RESET);
            }

            if (emitLineEnding)
            {
                if (NullLineEnding)
                {
                    stringBuilder.Append('\0');
                }
                else
                {
                    stringBuilder.Append(Environment.NewLine);
                }
            }

            return stringBuilder;
        }
        #endregion Private Methods
    }
}
