namespace PoESkillTree.Computation.Parsing.Builders.Skills
{
    // These are for modifier application and are converted from the skill's ActiveSkillTypes and
    // gem tags
    public interface IKeywordBuilders
    {
        // ActiveSkillType
        IKeywordBuilder Attack { get; }
        // ActiveSkillType
        IKeywordBuilder Spell { get; }
        // ActiveSkillType "projectile" or "explicit_deals_projectile_damage"
        IKeywordBuilder Projectile { get; }
        // ActiveSkillType "aoe"
        IKeywordBuilder AreaOfEffect { get; }
        // ActiveSkillType
        IKeywordBuilder Melee { get; }
        // ActiveSkillType
        IKeywordBuilder Totem { get; }
        // ActiveSkillType
        IKeywordBuilder Curse { get; }
        // ActiveSkillType
        IKeywordBuilder Trap { get; }
        // ActiveSkillType
        IKeywordBuilder Movement { get; }
        // ActiveSkillType
        IKeywordBuilder Cast { get; }
        // ActiveSkillType
        IKeywordBuilder Mine { get; }
        // ActiveSkillType
        IKeywordBuilder Vaal { get; }
        // ActiveSkillType
        IKeywordBuilder Aura { get; }
        // ActiveSkillType
        IKeywordBuilder Golem { get; }
        // ActiveSkillType
        IKeywordBuilder Minion { get; }
        // Gem Tag
        IKeywordBuilder Warcry { get; }

        // don't add keywords for damage types (e.g. physical, fire, elemental, chaos)
    }
}