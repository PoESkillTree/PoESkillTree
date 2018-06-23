using PoESkillTree.Computation.Common.Builders.Resolving;
using PoESkillTree.Computation.Common.Builders.Skills;

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

        public IKeywordBuilder Resolve(ResolveContext context) => _resolver(this, context);

        public Keyword Build() => Keyword.Projectile;
    }


    public class KeywordBuildersStub : IKeywordBuilders
    {
        private static IKeywordBuilder Create(string s) => new KeywordBuilderStub(s, (c, _) => c);

        public IKeywordBuilder Attack { get; } = Create("Attack");
        public IKeywordBuilder Spell { get; } = Create("Spell");
        public IKeywordBuilder Projectile { get; } = Create("Projectile");
        public IKeywordBuilder Bow { get; } = Create("Bow");
        public IKeywordBuilder AreaOfEffect { get; } = Create("Area of Effect");
        public IKeywordBuilder Melee { get; } = Create("Melee");
        public IKeywordBuilder Totem { get; } = Create("Totem");
        public IKeywordBuilder Curse { get; } = Create("Curse");
        public IKeywordBuilder Trap { get; } = Create("Trap");
        public IKeywordBuilder Movement { get; } = Create("Movement");
        public IKeywordBuilder Mine { get; } = Create("Mine");
        public IKeywordBuilder Vaal { get; } = Create("Vaal");
        public IKeywordBuilder Aura { get; } = Create("Aura");
        public IKeywordBuilder Golem { get; } = Create("Golem");
        public IKeywordBuilder Minion { get; } = Create("Minion");
        public IKeywordBuilder Warcry { get; } = Create("Warcry");
    }
}