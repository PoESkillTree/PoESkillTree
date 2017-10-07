using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Common.Utils.Extensions;

namespace PoESkillTree.Computation.Parsing.Builders.Matching
{
    public class ResolvedMatchContext<T> : IMatchContext<T>
    {
        private readonly IReadOnlyList<T> _values;

        public ResolvedMatchContext(IReadOnlyList<T> values)
        {
            _values = values;
        }

        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= _values.Count)
                    throw new ParseException(
                        $"Index out of bounds: index={index}, number of values={_values.Count}");
                return _values[index];
            }
        }

        public T First
        {
            get
            {
                if (_values.IsEmpty())
                    throw new ParseException(
                        "Tried to access first value in context with no values");
                return _values.First();
            }
        }

        public T Last
        {
            get
            {
                if (_values.IsEmpty())
                    throw new ParseException(
                        "Tried to access first value in context with no values");
                return _values.Last();
            }
        }

        public T Single
        {
            get
            {
                if (_values.IsEmpty())
                    throw new ParseException(
                        "Tried to access single value in context with no values");
                if (_values.Count > 1)
                    throw new ParseException(
                        "Tried to access single value in context with multiple values");
                return _values.Single();
            }
        }
    }
}