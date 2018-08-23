namespace PoESkillTree.Computation.Common.Builders.Skills
{
    /// <summary>
    /// Factory interface for keywords.
    /// </summary>
    /// <remarks>
    /// The keywords for damage types are contained in <see cref="Damage.IDamageTypeBuilders"/>. They are used as more
    /// than just keywords.
    /// <para>Referenced ActiveSkillTypes follow the naming in RePoE.</para>
    /// </remarks>
    public interface IKeywordBuilders
    {
        /// <summary>
        /// Equivalent to the ActiveSkillType "attack".
        /// </summary>
        IKeywordBuilder Attack { get; }

        /// <summary>
        /// Equivalent to the ActiveSkillType "spell".
        /// </summary>
        IKeywordBuilder Spell { get; }

        /// <summary>
        /// Equivalent to the union of the ActiveSkillTypes "projectile" and "explicit_deals_projectile_damage"
        /// (skills have this keyword if they have at least one of the two types).
        /// </summary>
        IKeywordBuilder Projectile { get; }

        /// <summary>
        /// Equivalent to the ActiveSkillType "aoe".
        /// </summary>
        IKeywordBuilder AreaOfEffect { get; }

        /// <summary>
        /// Equivalent to the ActiveSkillType "melee".
        /// </summary>
        IKeywordBuilder Melee { get; }

        /// <summary>
        /// Equivalent to the ActiveSkillType "totem".
        /// </summary>
        IKeywordBuilder Totem { get; }

        /// <summary>
        /// Equivalent to the ActiveSkillType "curse".
        /// </summary>
        IKeywordBuilder Curse { get; }

        /// <summary>
        /// Equivalent to the ActiveSkillType "trap".
        /// </summary>
        IKeywordBuilder Trap { get; }

        /// <summary>
        /// Equivalent to the ActiveSkillType "movement".
        /// </summary>
        IKeywordBuilder Movement { get; }

        /// <summary>
        /// Equivalent to the ActiveSkillType "mine".
        /// </summary>
        IKeywordBuilder Mine { get; }

        /// <summary>
        /// Equivalent to the ActiveSkillType "vaal".
        /// </summary>
        IKeywordBuilder Vaal { get; }

        /// <summary>
        /// Equivalent to the ActiveSkillType "aura".
        /// </summary>
        IKeywordBuilder Aura { get; }

        /// <summary>
        /// Equivalent to the ActiveSkillType "golem".
        /// </summary>
        IKeywordBuilder Golem { get; }

        /// <summary>
        /// Equivalent to the ActiveSkillType "minion".
        /// </summary>
        IKeywordBuilder Minion { get; }

        /// <summary>
        /// Equivalent to the gem tag "Warcry".
        /// </summary>
        IKeywordBuilder Warcry { get; }

        /// <summary>
        /// Equivalent to the gem tag.
        /// </summary>
        IKeywordBuilder Herald { get; }

        /// <summary>
        /// Has no equivalent gem tag or ActiveSkillType.
        /// </summary>
        IKeywordBuilder Offering { get; }

        /// <summary>
        /// Equivalent to the ActiveSkillType "trigger_attack"
        /// </summary>
        IKeywordBuilder CounterAttack { get; }
    }
}