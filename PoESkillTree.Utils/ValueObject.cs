using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using PoESkillTree.Utils.Extensions;

namespace PoESkillTree.Utils
{
    public abstract class ValueObject
    {
        public override bool Equals(object obj)
            => (obj == this) || (obj is ValueObject other && Equals(other));

        private bool Equals(ValueObject other)
            => GetType() == other.GetType() &&
               ToTuple().Equals(other.ToTuple());

        public override int GetHashCode() => ToTuple().GetHashCode();

        public override string ToString() => ToTuple().ToString();

        protected abstract object ToTuple();

        protected static object WithSequenceEquality<T>(IReadOnlyList<T> list)
            => new SequenceEquatableView<T>(list);

        private class SequenceEquatableView<T>
        {
            private readonly IReadOnlyList<T> _list;

            public SequenceEquatableView(IReadOnlyList<T> list) => _list = list;

            public override bool Equals(object obj)
                => (obj == this) || (obj is SequenceEquatableView<T> other && Equals(other));

            private bool Equals(SequenceEquatableView<T> other)
                => _list.SequenceEqual(other._list);

            public override int GetHashCode()
                => _list.SequenceHash();

            public override string ToString()
                => "[" + _list.ToDelimitedString(", ") + "]";
        }
    }
}