using past.Core;
using System;
using System.CommandLine;
using System.CommandLine.Binding;
using System.CommandLine.Parsing;

namespace past.ConsoleApp.Binders
{
    public class ClipboardItemIdentifierBinder : BinderBase<ClipboardItemIdentifier>
    {
        private readonly Argument<string> _identifierArgument;

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

        protected override ClipboardItemIdentifier GetBoundValue(BindingContext bindingContext)
            => GetBoundValue(bindingContext);
    }
}
