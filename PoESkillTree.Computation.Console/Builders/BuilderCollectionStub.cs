using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Computation.Parsing.Builders;
using PoESkillTree.Computation.Parsing.Builders.Conditions;
using PoESkillTree.Computation.Parsing.Builders.Values;

namespace PoESkillTree.Computation.Console.Builders
{
    public class BuilderCollectionStub<T> : BuilderStub, IBuilderCollection<T>, IEnumerable<T>
    {
        protected IConditionBuilders ConditionBuilders { get; }

        private readonly IReadOnlyList<T> _elements;

        protected BuilderCollectionStub(IReadOnlyList<T> elements, 
            IConditionBuilders conditionBuilders)
            : base("[" + string.Join(", ", elements) + "]")
        {
            _elements = elements;
            ConditionBuilders = conditionBuilders;
        }

        protected BuilderCollectionStub(BuilderCollectionStub<T> source, string stringRepresentation)
            : base(stringRepresentation)
        {
            _elements = source._elements;
            ConditionBuilders = source.ConditionBuilders;
        }

        public ValueBuilder Count(Func<T, IConditionBuilder> predicate = null)
        {
            var str = predicate == null
                ? ToString()
                : string.Join(", ", _elements.Select(predicate));
            return new ValueBuilder(new ValueBuilderStub($"Count({str})"), ConditionBuilders);
        }

        public IConditionBuilder Any(Func<T, IConditionBuilder> predicate = null)
        {
            var str = predicate == null
                ? ToString()
                : string.Join(", ", _elements.Select(predicate));
            return new ConditionBuilderStub($"Any({str})");
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _elements.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}