using PoESkillTree.Computation.Builders.Entities;
using PoESkillTree.Computation.Common.Builders.Actions;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Entities;
using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Computation.Common.Builders.Values;
using static PoESkillTree.Computation.Console.Builders.BuilderFactory;

namespace PoESkillTree.Computation.Console.Builders
{
    public class ActionBuilderStub : BuilderStub, IActionBuilder
    {
        private readonly Resolver<IActionBuilder> _resolver;

        private ActionBuilderStub(IEntityBuilder source, string stringRepresentation,
            Resolver<IActionBuilder> resolver)
            : base(stringRepresentation)
        {
            _source = source;
            _resolver = resolver;
        }

        public static IActionBuilder BySelf(string stringRepresentation, Resolver<IActionBuilder> resolver) =>
            new ActionBuilderStub(new ModifierSourceEntityBuilder(), stringRepresentation, resolver);

        private readonly IEntityBuilder _source;

        private IActionBuilder This => this;

        public IActionBuilder By(IEntityBuilder source)
        {
            IActionBuilder Resolve(ResolveContext context)
            {
                var inner = _resolver(this, context);
                return new ActionBuilderStub(
                    source.Resolve(context),
                    inner.ToString(),
                    (c, _) => c);
            }

            return new ActionBuilderStub(source, ToString(), (_, context) => Resolve(context));
        }

        public IConditionBuilder On =>
            CreateCondition(This,
                a => $"On {a}");

        public IConditionBuilder InPastXSeconds(IValueBuilder seconds) =>
            CreateCondition(This, seconds,
                (a, o) => $"If any {a} in the past {o}");

        public IConditionBuilder Recently =>
            CreateCondition(This,
                a => $"If any {a} recently");

        public ValueBuilder CountRecently =>
            new ValueBuilder(CreateValue($"Number of {this} recently"));

        public string Build() => ToString();

        public IActionBuilder Resolve(ResolveContext context) => _resolver(this, context);
    }
}