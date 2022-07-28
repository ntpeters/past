using past.Core;
using System;
using System.CommandLine;
using System.CommandLine.Binding;

namespace past.Console.Binders
{
    public class ClipboardItemIdentifierBinder : BinderBase<ClipboardItemIdentifier>
    {
        private readonly Argument<string> _identifierArgument;

        public ClipboardItemIdentifierBinder(Argument<string> identifierArgument)
        {
            _identifierArgument = identifierArgument ?? throw new ArgumentNullException(nameof(identifierArgument));
        }

        protected override ClipboardItemIdentifier GetBoundValue(BindingContext bindingContext)
        {
            var rawIdentifierValue = bindingContext.ParseResult.GetValueForArgument(_identifierArgument);
            if (ClipboardItemIdentifier.TryParse(rawIdentifierValue, out var identifier))
            {
                return identifier;
            }

            throw new ArgumentException($"Failed to parse identifier: ${rawIdentifierValue}");
        }
    }
}
