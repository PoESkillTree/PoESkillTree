namespace PoESkillTree.Computation.Providers.Skills
{
    // These are for modifier application and are converted from the skill's ActiveSkillTypes and
    // gem tags
    public interface IKeywordProviderFactory
    {
        // ActiveSkillType
        IKeywordProvider Attack { get; }
        // ActiveSkillType
        IKeywordProvider Spell { get; }
        // ActiveSkillType "projectile" or "explicit_deals_projectile_damage"
        IKeywordProvider Projectile { get; }
        // ActiveSkillType "aoe"
        IKeywordProvider AreaOfEffect { get; }
        // ActiveSkillType
        IKeywordProvider Melee { get; }
        // ActiveSkillType
        IKeywordProvider Totem { get; }
        // ActiveSkillType
        IKeywordProvider Curse { get; }
        // ActiveSkillType
        IKeywordProvider Trap { get; }
        // ActiveSkillType
        IKeywordProvider Movement { get; }
        // ActiveSkillType
        IKeywordProvider Cast { get; }
        // ActiveSkillType
        IKeywordProvider Mine { get; }
        // ActiveSkillType
        IKeywordProvider Vaal { get; }
        // ActiveSkillType
        IKeywordProvider Aura { get; }
        // ActiveSkillType
        IKeywordProvider Golem { get; }
        // Gem Tag
        IKeywordProvider Warcry { get; }

        // don't add keywords for damage types (e.g. physical, fire, elemental, chaos)
    }
}