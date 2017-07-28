namespace PoESkillTree.Computation.Providers
{
    public interface IKeywordProvider
    {

    }


    public interface IKeywordProviderFactory
    {
        IKeywordProvider Melee { get; }
        IKeywordProvider Attack { get; }
        IKeywordProvider Bow { get; }

        IKeywordProvider Projectile { get; }
        IKeywordProvider AreaOfEffect { get; }

        IKeywordProvider Movement { get; }

        IKeywordProvider Spell { get; }
        IKeywordProvider Curse { get; }
        IKeywordProvider Aura { get; }
        IKeywordProvider Offering { get; }
        IKeywordProvider Warcry { get; }

        IKeywordProvider Golem { get; }
        IKeywordProvider Trap { get; }
        IKeywordProvider Mine { get; }
        IKeywordProvider Totem { get; }

        IKeywordProvider Vaal { get; }

        // don't add keywords for damage types (e.g. physical, fire, elemental, chaos)
    }


    public static class KeywordProviders
    {
        public static readonly IKeywordProviderFactory Keyword;
    }
}