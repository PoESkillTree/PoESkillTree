namespace PoESkillTree.Computation.Common.Builders.Skills
{
    /// <summary>
    /// Factory interface for keywords. For information on the keywords, see <see cref="GameModel.Skills.Keyword"/>.
    /// </summary>
    /// <remarks>
    /// The keywords for damage types are contained in <see cref="Damage.IDamageTypeBuilders"/>. They are used as more
    /// than just keywords.
    /// <para>Referenced ActiveSkillTypes follow the naming in RePoE.</para>
    /// </remarks>
    public interface IKeywordBuilders
    {
        IKeywordBuilder Attack { get; }

        IKeywordBuilder Spell { get; }

        IKeywordBuilder Projectile { get; }

        IKeywordBuilder AreaOfEffect { get; }

        IKeywordBuilder Melee { get; }

        IKeywordBuilder Totem { get; }

        IKeywordBuilder Curse { get; }

        IKeywordBuilder Trap { get; }

        IKeywordBuilder Movement { get; }

        IKeywordBuilder Mine { get; }

        IKeywordBuilder Vaal { get; }

        IKeywordBuilder Aura { get; }

        IKeywordBuilder Golem { get; }

        IKeywordBuilder Minion { get; }

        IKeywordBuilder Warcry { get; }

        IKeywordBuilder Herald { get; }

        IKeywordBuilder Offering { get; }

        IKeywordBuilder CounterAttack { get; }
    }
}