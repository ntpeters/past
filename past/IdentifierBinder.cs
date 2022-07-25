using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Binding;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace past
{
    public class ClipboardItemIdentifier
    {
        public string RawValue { get; }

        public ClipboardItemIdentifier(string rawValue)
        {
            RawValue = rawValue ?? throw new ArgumentNullException(nameof(rawValue));
        }

        public bool TryGetAsIndex(out int index) => int.TryParse(RawValue, out index);

        public bool TryGetAsGuid(out Guid id) => Guid.TryParse(RawValue, out id);
    }

    public class IdentifierBinder : BinderBase<ClipboardItemIdentifier>
    {
        private readonly Argument<string> _identifierArgument;

        public IdentifierBinder(Argument<string> identifierArgument)
        {
            _identifierArgument = identifierArgument;
        }

        protected override ClipboardItemIdentifier GetBoundValue(BindingContext bindingContext)
        {
            return new ClipboardItemIdentifier(bindingContext.ParseResult.GetValueForArgument(_identifierArgument));
        }
    }
}
