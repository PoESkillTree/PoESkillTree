using PoESkillTree.Computation.Parsing.Builders.Skills;

namespace PoESkillTree.Computation.Console.Builders
{
    public class KeywordBuilderStub : BuilderStub, IKeywordBuilder
    {
        public KeywordBuilderStub(string stringRepresentation) : base(stringRepresentation)
        {
        }
    }


    public class KeywordBuildersStub : IKeywordBuilders
    {
        public IKeywordBuilder Attack { get; } = new KeywordBuilderStub("Attack");
        public IKeywordBuilder Spell { get; } = new KeywordBuilderStub("Spell");
        public IKeywordBuilder Projectile { get; } = new KeywordBuilderStub("Projectile");
        public IKeywordBuilder AreaOfEffect { get; } = new KeywordBuilderStub("Area of Effect");
        public IKeywordBuilder Melee { get; } = new KeywordBuilderStub("Melee");
        public IKeywordBuilder Totem { get; } = new KeywordBuilderStub("Totem");
        public IKeywordBuilder Curse { get; } = new KeywordBuilderStub("Curse");
        public IKeywordBuilder Trap { get; } = new KeywordBuilderStub("Trap");
        public IKeywordBuilder Movement { get; } = new KeywordBuilderStub("Movement");
        public IKeywordBuilder Cast { get; } = new KeywordBuilderStub("Cast");
        public IKeywordBuilder Mine { get; } = new KeywordBuilderStub("Mine");
        public IKeywordBuilder Vaal { get; } = new KeywordBuilderStub("Vaal");
        public IKeywordBuilder Aura { get; } = new KeywordBuilderStub("Aura");
        public IKeywordBuilder Golem { get; } = new KeywordBuilderStub("Golem");
        public IKeywordBuilder Warcry { get; } = new KeywordBuilderStub("Warcry");
    }
}