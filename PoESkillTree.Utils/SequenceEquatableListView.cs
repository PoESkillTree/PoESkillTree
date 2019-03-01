using System.Collections;
using System.Collections.Generic;
using MoreLinq;
using PoESkillTree.Utils.Extensions;

namespace PoESkillTree.Utils
{
    public class SequenceEquatableListView<T> : IReadOnlyList<T>
    {
        private readonly IReadOnlyList<T> _list;

        public SequenceEquatableListView(IReadOnlyList<T> list) => _list = list;

        public override bool Equals(object obj)
            => (obj == this) || (obj is SequenceEquatableListView<T> other && Equals(other));

        private bool Equals(SequenceEquatableListView<T> other)
            => _list.SequenceEqual(other._list);

        public override int GetHashCode()
            => _list.SequenceHash();

        public override string ToString()
            => "[" + _list.ToDelimitedString(", ") + "]";

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();

        public int Count => _list.Count;

        public T this[int index] => _list[index];
    }
}