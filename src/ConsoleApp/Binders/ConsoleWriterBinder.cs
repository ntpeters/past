using past.ConsoleApp.Wrappers;
using System;
using System.CommandLine;
using System.CommandLine.Binding;
using System.CommandLine.Parsing;

namespace past.ConsoleApp.Binders
{
    /// <summary>
    /// Supports binding <see cref="ConsoleWriter"/>.
    /// </summary>
    public class ConsoleWriterBinder : BinderBase<IConsoleWriter>
    {
        private readonly Option<bool>? _ansiOption;
        private readonly Option<AnsiResetType>? _ansiResetOption;
        private readonly Option<bool> _quietOption;

        /// <summary>
        /// Creates a new <see cref="ConsoleWriterBinder"/> for the given options.
        /// </summary>
        /// <param name="ansiOption">ANSI option to bind to.</param>
        /// <param name="ansiResetOption">ANSI reset option to bind to.</param>
        /// <param name="quietOption">Quiet option to bind to.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="ansiOption"/>, <paramref name="ansiResetOption"/>, or <paramref name="quietOption"/> is null.
        /// </exception>
        public ConsoleWriterBinder(Option<bool> ansiOption, Option<AnsiResetType> ansiResetOption, Option<bool> quietOption)
            : this(quietOption)
        {
            _ansiOption = ansiOption ?? throw new ArgumentNullException(nameof(ansiOption));
            _ansiResetOption = ansiResetOption ?? throw new ArgumentNullException(nameof(ansiResetOption));
        }

        /// <summary>
        /// Creates a new <see cref="ConsoleWriterBinder"/> for the given option.
        /// </summary>
        /// <param name="quietOption">Quiet option to bind to.</param>
        /// <exception cref="ArgumentNullException"><paramref name="quietOption"/> is null.</exception>
        public ConsoleWriterBinder(Option<bool> quietOption)
        {
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
            var ansiEnabled = _ansiOption != null && parseResult.GetValueForOption(_ansiOption);
            var ansiResetType = _ansiResetOption != null ? parseResult.GetValueForOption(_ansiResetOption) : AnsiResetType.Off;
            var quietEnabled = parseResult.GetValueForOption(_quietOption);

            var consoleUtilities = new ConsoleUtilities();
            var environment = new EnvironmentWrapper();
            return new ConsoleWriter(console, consoleUtilities, environment, quietEnabled, ansiEnabled , ansiResetType);
        }

        protected override IConsoleWriter GetBoundValue(BindingContext bindingContext)
            => GetBoundValue(bindingContext.ParseResult, bindingContext.Console);
    }
}
