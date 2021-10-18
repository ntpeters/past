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
    }
}
