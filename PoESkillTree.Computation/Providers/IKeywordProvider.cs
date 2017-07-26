namespace PoESkillTree.Computation.Providers
{
    public interface IKeywordProvider
    {

    }

    public static class KeywordProviders
    {
        // TODO invert names (e.g. Keyword.Melee) for far less constants

        public static readonly IKeywordProvider MeleeKeyword;
        public static readonly IKeywordProvider AttackKeyword;
        public static readonly IKeywordProvider BowKeyword;

        public static readonly IKeywordProvider ProjectileKeyword;
        public static readonly IKeywordProvider AreaOfEffectKeyword;

        public static readonly IKeywordProvider MovementKeyword;

        public static readonly IKeywordProvider SpellKeyword;
        public static readonly IKeywordProvider CurseKeyword;
        public static readonly IKeywordProvider AuraKeyword;
        public static readonly IKeywordProvider OfferingKeyword;
        public static readonly IKeywordProvider WarcryKeyword;

        public static readonly IKeywordProvider GolemKeyword;
        public static readonly IKeywordProvider TrapKeyword;
        public static readonly IKeywordProvider MineKeyword;
        public static readonly IKeywordProvider TotemKeyword;

        public static readonly IKeywordProvider VaalKeyword;

        // don't add keywords for damage types (e.g. physical, fire, elemental, chaos)
    }
}