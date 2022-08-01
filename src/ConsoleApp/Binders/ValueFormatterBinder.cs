using System;
using System.CommandLine;
using System.CommandLine.Binding;
using System.CommandLine.Parsing;

namespace past.ConsoleApp.Binders
{
    public class ValueFormatterBinder : BinderBase<IValueFormatter>
    {
        private readonly Option<bool>? _nullOption;
        private readonly Option<bool>? _indexOption;
        private readonly Option<bool>? _idOption;
        private readonly Option<bool>? _timeOption;

        public ValueFormatterBinder()
        {
            _nullOption = null;
            _indexOption = null;
            _idOption = null;
            _timeOption = null;
        }

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
            var nullEnabled = _nullOption != null && parseResult.GetValueForOption(_nullOption);
            var indexEnabled = _indexOption != null && parseResult.GetValueForOption(_indexOption);
            var idEnabled = _idOption != null && parseResult.GetValueForOption(_idOption);
            var timeEnabled = _timeOption != null && parseResult.GetValueForOption(_timeOption);
            return new ValueFormatter(nullEnabled, indexEnabled, idEnabled, timeEnabled);
        }

        protected override IValueFormatter GetBoundValue(BindingContext bindingContext)
            => GetBoundValue(bindingContext.ParseResult);
    }
}
