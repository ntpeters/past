using past.Core.Models;
using System;
using System.CommandLine;
using System.CommandLine.Binding;
using System.CommandLine.Parsing;
using System.Diagnostics.CodeAnalysis;

namespace past.ConsoleApp.Binders
{
    /// <summary>
    /// Supports binding <see cref="ClipboardItemIdentifier"/>.
    /// </summary>
    public class ClipboardItemIdentifierBinder : BinderBase<ClipboardItemIdentifier>
    {
        private readonly Argument<string> _identifierArgument;

        /// <summary>
        /// Creates a new <see cref="ClipboardItemIdentifierBinder"/> for the given argument.
        /// </summary>
        /// <param name="identifierArgument">Identifier argument to bind to.</param>
        /// <exception cref="ArgumentNullException"><paramref name="identifierArgument"/> is null.</exception>
        public ClipboardItemIdentifierBinder(Argument<string> identifierArgument)
        {
            _identifierArgument = identifierArgument ?? throw new ArgumentNullException(nameof(identifierArgument));
        }

        /// <summary>
        /// Gets a value from the parse result.
        /// </summary>
        /// <remarks>
        /// This overload of <see cref="GetBoundValue(BindingContext)"/> is provided to support
        /// unit testing this binder directly.
        /// </remarks>
        /// <param name="parseResult"><see cref="ParseResult"/> to get the value from.</param>
        /// <returns><see cref="ClipboardItemIdentifier"/> based on the values from the parse result.</returns>
        public ClipboardItemIdentifier GetBoundValue(ParseResult parseResult)
        {
            var rawIdentifierValue = parseResult.GetValueForArgument(_identifierArgument);
            if (ClipboardItemIdentifier.TryParse(rawIdentifierValue, out var identifier))
            {
                return identifier;
            }

            throw new ArgumentException($"Failed to parse identifier: ${rawIdentifierValue}");
        }

        [ExcludeFromCodeCoverage(Justification = "BindingContext is not mockable and has no public constructor. Use GetBoundValue(ParseResult) for testing instead.")]
        protected override ClipboardItemIdentifier GetBoundValue(BindingContext bindingContext)
            => GetBoundValue(bindingContext.ParseResult);
    }
}
