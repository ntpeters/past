using past.Core;
using System;
using System.CommandLine;
using System.CommandLine.Binding;

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

        protected override ContentType GetBoundValue(BindingContext bindingContext)
        {
            if (bindingContext.ParseResult.GetValueForOption(_allOption))
            {
                return ContentType.All;
            }

            return bindingContext.ParseResult.GetValueForOption(_typeOption);
        }
    }
}
