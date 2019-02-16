using System.Collections.Generic;
using MoreLinq;
using PoESkillTree.Utils.Extensions;

namespace PoESkillTree.Utils
{
    /// <summary>
    /// Base class for plain objects that are only a set of their immutable properties.
    /// </summary>
    public abstract class ValueObject
    {
        private readonly bool _cacheTupleAndHashCode;
        private object _tuple;
        private int? _hashCode;

        protected ValueObject(bool cacheTupleAndHashCode = false)
            => _cacheTupleAndHashCode = cacheTupleAndHashCode;

        public override bool Equals(object obj)
            => (obj == this) || (obj is ValueObject other && Equals(other));

        private bool Equals(ValueObject other)
            => GetType() == other.GetType() &&
               GetTuple().Equals(other.GetTuple());

        public override int GetHashCode()
        {
            if (!_cacheTupleAndHashCode)
                return GetTuple().GetHashCode();
            // ReSharper disable twice NonReadonlyMemberInGetHashCode It's the cache, of course it's not readonly
            return _hashCode ?? (_hashCode = GetTuple().GetHashCode()).Value;
        }

        public override string ToString() => GetTuple().ToString();

        private object GetTuple()
        {
            if (!_cacheTupleAndHashCode)
                return ToTuple();
            return _tuple ?? (_tuple = ToTuple());
        }

        /// <summary>
        /// Returns an object (generally a tuple) that is used to implement Equals, GetHashCode and ToString.
        /// </summary>
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