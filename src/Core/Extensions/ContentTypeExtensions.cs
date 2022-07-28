using System.Diagnostics.CodeAnalysis;
using Windows.ApplicationModel.DataTransfer;

namespace past.Core.Extensions
{
    public static class ContentTypeExtensions
    {
        public static bool TryToStandardDataFormat(this ContentType type, [NotNullWhen(true)] out IReadOnlyList<string>? formatIds)
        {
            var supportedFormats = new List<string>();
            if (type.HasFlag(ContentType.Text))
            {
                supportedFormats.Add(StandardDataFormats.Text);
            }
            else if (type.HasFlag(ContentType.Image))
            {
                supportedFormats.Add(StandardDataFormats.Bitmap);
            }
            else
            {
                formatIds = null;
                return false;
            }

            formatIds = supportedFormats;
            return true;
        }

        public static bool Supports(this ContentType type, Func<string, bool> predicate)
        {
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
