using System;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Computation.Common.Builders.Values;
using static PoESkillTree.Computation.Console.Builders.BuilderFactory;

namespace PoESkillTree.Computation.Console.Builders
{
    public abstract class BuilderCollectionStub<T> : BuilderStub, IBuilderCollection<T>
    {
        /// <summary>
        /// Dummy element of type <typeparamref name="T"/> used as parameter to predicates.
        /// </summary>
        protected T DummyElement { get; }

        private readonly Resolver<IBuilderCollection> _resolver;

        protected BuilderCollectionStub(
            T dummyElement, string stringRepresentation, Resolver<IBuilderCollection> resolver)
            : base(stringRepresentation)
        {
            DummyElement = dummyElement;
            _resolver = resolver;
        }

        private IBuilderCollection This => this;

        public ValueBuilder Count() => new ValueBuilder(CreateValue(This, o => $"Count({o})"));

        public IConditionBuilder Any() => CreateCondition(This, o => $"Any({o})");

        public ValueBuilder Count(Func<T, IConditionBuilder> predicate)
        {
            string StringRepresentation(IBuilderCollection coll, IConditionBuilder cond) =>
                $"Count({coll}.Where({cond}))";

            var condition = predicate.Invoke(DummyElement);
            return new ValueBuilder(CreateValue(This, condition, StringRepresentation));
        }

        public IConditionBuilder Any(Func<T, IConditionBuilder> predicate)
        {
            string StringRepresentation(IBuilderCollection coll, IConditionBuilder cond) =>
                $"Any({coll}.Where({cond}))";

            var condition = predicate.Invoke(DummyElement);
            return CreateCondition(This, condition, StringRepresentation);
        }

        public IBuilderCollection Resolve(ResolveContext context) => _resolver(this, context);
    }
}