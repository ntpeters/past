using System.Diagnostics.CodeAnalysis;

namespace past.Core.Models
{
    /// <summary>
    /// Identifier describing a clipbpoard history item with either the index or the item ID.
    /// </summary>
    public class ClipboardItemIdentifier : IEquatable<ClipboardItemIdentifier>
    {
        #region Private Fields
        private readonly int? _index;
        private readonly Guid? _id;
        #endregion Private Fields

        #region Static Methods
        /// <summary>
        /// Converts the string representation of either a 32-bit signed integer or a <see cref="Guid"/>
        /// to the equivalent <see cref="ClipboardItemIdentifier"/>.
        /// </summary>
        /// <param name="value">Value to parse.</param>
        /// <param name="identifier">Identifier represented by the given value if parsing succeeds, null otherwise.</param>
        /// <returns>True if parsing succeeds, false otherwise.</returns>
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
        /// <summary>
        /// Creates a new <see cref="ClipboardItemIdentifier"/> wrapping the given index.
        /// </summary>
        /// <param name="index">Index to create the identifier with.</param>
        public ClipboardItemIdentifier(int index)
        {
            _index = index;
        }

        /// <summary>
        /// Creates a new <see cref="ClipboardItemIdentifier"/> wrapping the given ID.
        /// </summary>
        /// <param name="id">ID to create the identifier with.</param>
        public ClipboardItemIdentifier(Guid id)
        {
            _id = id;
        }
        #endregion Constructors

        #region Public Methods
        /// <summary>
        /// Gets the identifier value as a 32-bit signed integer.
        /// </summary>
        /// <param name="index">Index represented by the identifier if it was created with an index, null otherwise.</param>
        /// <returns>True if the identifier was created with an index, false otherwise.</returns>
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

        /// <summary>
        /// Gets the identifier value as a <see cref="Guid"/>.
        /// </summary>
        /// <param name="id">ID represented by the identifier if it was created with an ID, null otherwise.</param>
        /// <returns>True if the identifier was created with an ID, false otherwise.</returns>
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

        public override bool Equals(object? other) => other is ClipboardItemIdentifier otherId && Equals(otherId);

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
