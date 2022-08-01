using System.Diagnostics.CodeAnalysis;
using Windows.ApplicationModel.DataTransfer;

namespace past.Core.Extensions
{
    /// <summary>
    /// Extensions for <see cref="ContentType"/>.
    /// </summary>
    public static class ContentTypeExtensions
    {
        /// <summary>
        /// Get the <see cref="StandardDataFormats"/> format IDs compatible with the content type.
        /// </summary>
        /// <param name="type">Content type to get the format IDs for.</param>
        /// <param name="formatIds">All format IDs compatible with the content type.</param>
        /// <returns>True if format IDs were found for the content type, false otherwise.</returns>
        public static bool TryToStandardDataFormat(this ContentType type, [NotNullWhen(true)] out IReadOnlyList<string>? formatIds)
        {
            var supportedFormats = new List<string>();
            if (type.HasFlag(ContentType.Text))
            {
                supportedFormats.Add(StandardDataFormats.Text);
            }

            if (type.HasFlag(ContentType.Image))
            {
                supportedFormats.Add(StandardDataFormats.Bitmap);
            }

            if (supportedFormats.Count == 0)
            {
                formatIds = null;
                return false;
            }

            formatIds = supportedFormats;
            return true;
        }

        /// <summary>
        /// Checks whether the content type is supported based on passing each compatible format ID to the provided predicate.
        /// </summary>
        /// <param name="type">Content type to evaluate the support of.</param>
        /// <param name="predicate">Function used to determine support.</param>
        /// <returns>True if the predicate returns true for any format ID value passed to it.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="predicate"/> is null.</exception>
        public static bool Supports(this ContentType type, Func<string, bool> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            if (type == ContentType.All)
            {
                return true;
            }

            if (TryToStandardDataFormat(type, out var formatIds))
            {
                return formatIds.Any(formatId => predicate(formatId));
            }

            return false;
        }
    }
}
