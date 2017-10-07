using System;
using PoESkillTree.Computation.Parsing.Builders;
using PoESkillTree.Computation.Parsing.Builders.Conditions;
using PoESkillTree.Computation.Parsing.Builders.Matching;
using PoESkillTree.Computation.Parsing.Builders.Values;
using static PoESkillTree.Computation.Console.Builders.BuilderFactory;

namespace PoESkillTree.Computation.Console.Builders
{
    public abstract class BuilderCollectionStub<T> : BuilderStub, IBuilderCollection<T>
    {
        protected T DummyElement { get; }

        private readonly Resolver<IBuilderCollection<T>> _resolver;

        protected BuilderCollectionStub(T dummyElement, string stringRepresentation, 
            Resolver<IBuilderCollection<T>> resolver)
            : base(stringRepresentation)
        {
            DummyElement = dummyElement;
            _resolver = resolver;
        }

        private IBuilderCollection<T> This => this;

        public ValueBuilder Count(Func<T, IConditionBuilder> predicate = null)
        {
            string StringRepresentation(IBuilderCollection<T> coll, IConditionBuilder cond) =>
                $"Count({coll}" + (cond == null ? "" : $".Where({cond})") + ")";

            var condition = predicate?.Invoke(DummyElement);
            return new ValueBuilder(CreateValue(This, condition, StringRepresentation));
        }

        public IConditionBuilder Any(Func<T, IConditionBuilder> predicate = null)
        {
            string StringRepresentation(IBuilderCollection<T> coll, IConditionBuilder cond) =>
                $"Any({coll}" + (cond == null ? "" : $".Where({cond})") + ")";

            var condition = predicate?.Invoke(DummyElement);
            return CreateCondition(This, condition, StringRepresentation);
        }

        public IBuilderCollection<T> Resolve(IMatchContext<IValueBuilder> valueContext)
            => _resolver(this, valueContext);
    }
}