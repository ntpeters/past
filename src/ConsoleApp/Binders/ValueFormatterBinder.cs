using System;
using System.CommandLine;
using System.CommandLine.Binding;
using System.CommandLine.Parsing;

namespace past.ConsoleApp.Binders
{
    public class ValueFormatterBinder : BinderBase<IValueFormatter>
    {
        private readonly Option<bool> _nullOption;
        private readonly Option<bool> _indexOption;
        private readonly Option<bool> _idOption;
        private readonly Option<bool> _timeOption;

        public ValueFormatterBinder(Option<bool> nullOption, Option<bool> indexOption, Option<bool> idOption, Option<bool> timeOption)
        {
            _nullOption = nullOption ?? throw new ArgumentNullException(nameof(nullOption));
            _indexOption = indexOption ?? throw new ArgumentNullException(nameof(indexOption));
            _idOption = idOption ?? throw new ArgumentNullException(nameof(idOption));
            _timeOption = timeOption ?? throw new ArgumentNullException(nameof(timeOption));
        }

        /// <summary>
        /// Gets a value from the parse result.
        /// </summary>
        /// <remarks>
        /// This overload of <see cref="GetBoundValue(BindingContext)"/> is provided to support
        /// unit testing this binder directly.
        /// </remarks>
        /// <param name="parseResult"><see cref="ParseResult"/> to get the value from.</param>
        /// <returns><see cref="IValueFormatter"/> based on the values from the parse result.</returns>
        public IValueFormatter GetBoundValue(ParseResult parseResult)
        {
            var nullEnabled = parseResult.GetValueForOption(_nullOption);
            var indexEnabled = parseResult.GetValueForOption(_indexOption);
            var idEnabled = parseResult.GetValueForOption(_idOption);
            var timeEnabled = parseResult.GetValueForOption(_timeOption);
            return new ValueFormatter(nullEnabled, indexEnabled, idEnabled, timeEnabled);
        }

        protected override IValueFormatter GetBoundValue(BindingContext bindingContext)
            => GetBoundValue(bindingContext.ParseResult);
    }
}
