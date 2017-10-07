using PoESkillTree.Computation.Parsing.Builders.Matching;
using PoESkillTree.Computation.Parsing.Builders.Skills;
using PoESkillTree.Computation.Parsing.Builders.Values;

namespace PoESkillTree.Computation.Console.Builders
{
    public class KeywordBuilderStub : BuilderStub, IKeywordBuilder
    {
        private readonly Resolver<IKeywordBuilder> _resolver;

        public KeywordBuilderStub(string stringRepresentation, Resolver<IKeywordBuilder> resolver)
            : base(stringRepresentation)
        {
            _resolver = resolver;
        }

        public IKeywordBuilder Resolve(IMatchContext<IValueBuilder> valueContext) =>
            _resolver(this, valueContext);
    }


    public class KeywordBuildersStub : IKeywordBuilders
    {
        private static IKeywordBuilder Create(string s)
            => new KeywordBuilderStub(s, (c, _) => c);

        public IKeywordBuilder Attack { get; } = Create("Attack");
        public IKeywordBuilder Spell { get; } = Create("Spell");
        public IKeywordBuilder Projectile { get; } = Create("Projectile");
        public IKeywordBuilder AreaOfEffect { get; } = Create("Area of Effect");
        public IKeywordBuilder Melee { get; } = Create("Melee");
        public IKeywordBuilder Totem { get; } = Create("Totem");
        public IKeywordBuilder Curse { get; } = Create("Curse");
        public IKeywordBuilder Trap { get; } = Create("Trap");
        public IKeywordBuilder Movement { get; } = Create("Movement");
        public IKeywordBuilder Cast { get; } = Create("Cast");
        public IKeywordBuilder Mine { get; } = Create("Mine");
        public IKeywordBuilder Vaal { get; } = Create("Vaal");
        public IKeywordBuilder Aura { get; } = Create("Aura");
        public IKeywordBuilder Golem { get; } = Create("Golem");
        public IKeywordBuilder Warcry { get; } = Create("Warcry");
    }
}