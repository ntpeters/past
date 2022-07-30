using System;
using System.CommandLine;
using System.CommandLine.Binding;
using System.CommandLine.Parsing;

namespace past.ConsoleApp.Binders
{
    public class ConsoleWriterBinder : BinderBase<IConsoleWriter>
    {
        private readonly Option<bool> _ansiOption;
        private readonly Option<AnsiResetType> _ansiResetOption;
        private readonly Option<bool> _quietOption;

        public ConsoleWriterBinder(Option<bool> ansiOption, Option<AnsiResetType> ansiResetOption, Option<bool> quietOption)
        {
            _ansiOption = ansiOption ?? throw new ArgumentNullException(nameof(ansiOption));
            _ansiResetOption = ansiResetOption ?? throw new ArgumentNullException(nameof(ansiResetOption));
            _quietOption = quietOption ?? throw new ArgumentNullException(nameof(quietOption));
        }

        /// <summary>
        /// Gets a value from the parse result.
        /// </summary>
        /// <remarks>
        /// This overload of <see cref="GetBoundValue(BindingContext)"/> is provided to support
        /// unit testing this binder directly.
        /// </remarks>
        /// <param name="parseResult"><see cref="ParseResult"/> to get the value from.</param>
        /// <param name="console"><see cref="IConsole"/> instance to use for output.</param>
        /// <returns><see cref="IConsoleWriter"/> based on the values from the parse result.</returns>
        public IConsoleWriter GetBoundValue(ParseResult parseResult, IConsole console)
        {
            var ansiEnabled = parseResult.GetValueForOption(_ansiOption);
            var ansiResetType = parseResult.GetValueForOption(_ansiResetOption);
            var quietEnabled = parseResult.GetValueForOption(_quietOption);
            return new ConsoleWriter(console, quietEnabled, ansiEnabled , ansiResetType);
        }

        protected override IConsoleWriter GetBoundValue(BindingContext bindingContext)
            => GetBoundValue(bindingContext.ParseResult, bindingContext.Console);
    }
}
