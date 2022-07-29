using past.Core;
using System;
using System.CommandLine;
using System.CommandLine.Binding;
using System.CommandLine.Parsing;

namespace past.ConsoleApp.Binders
{
    public class ContentTypeBinder : BinderBase<ContentType>
    {
        private readonly Option<ContentType> _typeOption;
        private readonly Option<bool> _allOption;

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

            return parseResult.GetValueForOption(_typeOption);
        }

        protected override ContentType GetBoundValue(BindingContext bindingContext)
            => GetBoundValue(bindingContext.ParseResult);
    }
}
