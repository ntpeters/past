using past.Core;
using System;
using System.CommandLine;
using System.CommandLine.Binding;
using System.CommandLine.Parsing;
using System.Diagnostics.CodeAnalysis;

namespace past.ConsoleApp.Binders
{
    /// <summary>
    /// Supports binding <see cref="ContentType"/>.
    /// </summary>
    public class ContentTypeBinder : BinderBase<ContentType>
    {
        private readonly Option<ContentType> _typeOption;
        private readonly Option<bool> _allOption;

        /// <summary>
        /// Creates a new <see cref="ContentTypeBinder"/> for the given options.
        /// </summary>
        /// <param name="typeOption">Type option to bind to.</param>
        /// <param name="allOption">All option to bind to.</param>
        /// <exception cref="ArgumentNullException"><paramref name="typeOption"/> or <paramref name="allOption"/> is null.</exception>
        public ContentTypeBinder(Option<ContentType> typeOption, Option<bool> allOption)
        {
            _typeOption = typeOption ?? throw new ArgumentNullException(nameof(typeOption));
            _allOption = allOption ?? throw new ArgumentNullException(nameof(allOption));
        }

        /// <summary>
        /// Gets a value from the parse result.
        /// </summary>
        /// <remarks>
        /// This overload of <see cref="GetBoundValue(BindingContext)"/> is provided to support
        /// unit testing this binder directly.
        /// </remarks>
        /// <param name="parseResult"><see cref="ParseResult"/> to get the value from.</param>
        /// <returns><see cref="ContentType"/> based on the values from the parse result.</returns>
        public ContentType GetBoundValue(ParseResult parseResult)
        {
            if (parseResult.GetValueForOption(_allOption))
            {
                return ContentType.All;
            }

            var type = parseResult.GetValueForOption(_typeOption);
            if (Enum.IsDefined(type))
            {
                return type;
            }

            throw new ArgumentException("Failed to bind content type");
        }

        [ExcludeFromCodeCoverage(Justification = "BindingContext is not mockable and has no public constructor. Use GetBoundValue(ParseResult) for testing instead.")]
        protected override ContentType GetBoundValue(BindingContext bindingContext)
            => GetBoundValue(bindingContext.ParseResult);
    }
}
