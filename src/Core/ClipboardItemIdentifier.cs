using System.Diagnostics.CodeAnalysis;

namespace past.Core
{
    public class ClipboardItemIdentifier : IEquatable<ClipboardItemIdentifier>
    {
        #region Private Fields
        private readonly int? _index;
        private readonly Guid? _id;
        #endregion Private Fields

        #region Static Methods
        public static bool TryParse(string value, [NotNullWhen(true)] out ClipboardItemIdentifier? identifier)
        {
            if (int.TryParse(value, out var index))
            {
                identifier = new ClipboardItemIdentifier(index);
                return true;
            }
            else if (Guid.TryParse(value, out var id))
            {
                identifier = new ClipboardItemIdentifier(id);
                return true;
            }

            identifier = null;
            return false;
        }
        #endregion Static Methods

        #region Constructors
        public ClipboardItemIdentifier(int index)
        {
            _index = index;
        }

        public ClipboardItemIdentifier(Guid id)
        {
            _id = id;
        }
        #endregion Constructors

        #region Public Methods
        public bool TryGetAsIndex([NotNullWhen(true)] out int? index)
        {
            if (_index.HasValue)
            {
                index = _index.Value;
                return true;
            }

            index = null;
            return false;
        }

        public bool TryGetAsGuid([NotNullWhen(true)] out Guid? id)
        {
            if (_id.HasValue)
            {
                id = _id.Value;
                return true;
            }

            id = null;
            return false;
        }
        #endregion Public Methods

        #region Equality
        public bool Equals(ClipboardItemIdentifier? other)
        {
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            if (other is null)
            {
                return false;
            }

            if (other.TryGetAsIndex(out var otherIndex) && TryGetAsIndex(out var thisIndex) && otherIndex == thisIndex)
            {
                return true;
            }

            if (other.TryGetAsGuid(out var otherId) && TryGetAsGuid(out var thisId) && otherId == thisId)
            {
                return true;
            }

            return false;
        }

        public override bool Equals(object? other) => (other is ClipboardItemIdentifier otherId) && Equals(otherId);

        public override int GetHashCode() => (_index, _id).GetHashCode();

        public static bool Equals(ClipboardItemIdentifier? object1, ClipboardItemIdentifier? object2)
        {
            if (object1 is null)
            {
                return object2 is null;
            }

            return object1.Equals(object2);
        }
        #endregion Equality

        #region Operators
        public static bool operator ==(ClipboardItemIdentifier? object1, ClipboardItemIdentifier? object2) => Equals(object1, object2);
        public static bool operator !=(ClipboardItemIdentifier? object1, ClipboardItemIdentifier? object2) => !(object1 == object2);

        public static implicit operator ClipboardItemIdentifier(int index) => new ClipboardItemIdentifier(index);
        public static implicit operator ClipboardItemIdentifier(Guid id) => new ClipboardItemIdentifier(id);
        #endregion Operators
    }
}
