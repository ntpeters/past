using System;
using System.Collections.Generic;
using System.Linq;
using Windows.ApplicationModel.DataTransfer;
using static past.Program;

namespace past.Extensions
{
    public static class ContentTypeExtensions
    {
        public static ContentType Resolve(this ContentType type, bool allOption)
        {
            _ = TryResolve(type, allOption, out var resolvedType);
            return resolvedType;
        }

        public static bool TryResolve(this ContentType type, bool allOption, out ContentType resolvedType)
        {
            if (allOption)
            {
                resolvedType = ContentType.All;
                if (type == ContentType.Default)
                {
                    // All option provided with default content type is the default behavior, therefore no "resolving" needed
                    return false;
                }
                return true;
            }
            else if (type == ContentType.Default)
            {
                // Text is the default content type to return, therefore no "resolving" needed
                resolvedType = ContentType.Text;
            }
            else
            {
                // Nothing overriding content type, therefore no "resolving" needed
                resolvedType = type;
            }

            return false;
        }

        public static bool TryToStandardDataFormat(this ContentType type, out string formatId)
        {
            switch (type)
            {
                case ContentType.Default: // Fallthrough
                case ContentType.Text:
                    formatId = StandardDataFormats.Text;
                    return true;
                case ContentType.Image:
                    formatId = StandardDataFormats.Bitmap;
                    return true;
                case ContentType.File:
                    formatId = StandardDataFormats.StorageItems;
                    return true;
                default:
                    formatId = string.Empty;
                    return false;
            }
        }

        public static bool Supports(this ContentType type, Func<string, bool> predicate)
        {
            if (type == ContentType.All)
            {
                return true;
            }

            if (TryToStandardDataFormat(type, out var formatId))
            {
                return predicate(formatId);
            }

            return false;
        }

        public static bool Supports(this ContentType type, IEnumerable<string> formatIds) => formatIds.Any(formatId => type.Supports(formatId));

        public static bool Supports(this ContentType type, string formatId)
        {
            if (type == ContentType.All)
            {
                return true;
            }

            if (formatId == StandardDataFormats.Text && type == ContentType.Text)
            {
                return true;
            }

            return false;
        }
    }
}
