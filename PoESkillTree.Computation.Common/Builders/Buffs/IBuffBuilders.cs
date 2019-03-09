using System;
using PoESkillTree.Computation.Common.Builders.Entities;
using PoESkillTree.Computation.Common.Builders.Stats;

namespace PoESkillTree.Computation.Common.Builders.Buffs
{
    /// <summary>
    /// Factory interface for buffs.
    /// </summary>
    public interface IBuffBuilders
    {
        /// <summary>
        /// Gets a buff representing Fortify.
        /// </summary>
        IBuffBuilder Fortify { get; }

        /// <summary>
        /// Gets a buff representing Maim.
        /// </summary>
        IBuffBuilder Maim { get; }

        /// <summary>
        /// Gets a buff representing Intimidate.
        /// </summary>
        IBuffBuilder Intimidate { get; }

        /// <summary>
        /// Gets a buff representing Taunt.
        /// </summary>
        IBuffBuilder Taunt { get; }

        /// <summary>
        /// Gets a buff representing Blind.
        /// </summary>
        IBuffBuilder Blind { get; }

        /// <summary>
        /// Gets a buff representing Onslaught.
        /// </summary>
        IBuffBuilder Onslaught { get; }

        /// <summary>
        /// Gets a buff representing Unholy Might.
        /// </summary>
        IBuffBuilder UnholyMight { get; }

        /// <summary>
        /// Gets a buff representing Phasing.
        /// </summary>
        IBuffBuilder Phasing { get; }

        IBuffBuilder ArcaneSurge { get; }
        IBuffBuilder Tailwind { get; }
        IBuffBuilder CoveredInAsh { get; }
        IBuffBuilder Innervation { get; }
        IBuffBuilder Impale { get; }
        IBuffBuilder Infusion { get; }

        /// <summary>
        /// Gets a buff factory that creates Conflux buffs.
        /// </summary>
        IConfluxBuffBuilders Conflux { get; }

        /// <summary>
        /// Returns a buff providing <paramref name="gainedStat"/>. Self gains the buff temporarily in some interval.
        /// <para> Whether the buff is currently active needs to be selected by the user. </para>
        /// </summary>
        IStatBuilder Temporary(IStatBuilder gainedStat);
        
        /// <summary>
        /// Returns a stat representing Self gaining <paramref name="gainedStat"/> as part of a buff rotation.
        /// <para> The buff is part of a buff rotation and is active when the current step is
        /// <paramref name="condition"/>. The current step needs to be selected by the user. </para>
        /// </summary>
        IStatBuilder Temporary<T>(IStatBuilder gainedStat, T condition) where T : struct, Enum;

        /// <summary>
        /// Returns a stat representing Self gaining <paramref name="buff"/> as part of a rotation.
        /// <para> The buff is part of a buff rotation and is active when the current step is
        /// <paramref name="condition"/>. The current step needs to be selected by the user. </para>
        /// </summary>
        IStatBuilder Temporary<T>(IBuffBuilder buff, T condition) where T : struct, Enum;

        /// <summary>
        /// Returns a buff providing <paramref name="gainedStat"/> and affecting <paramref name="affectedEntites"/>
        /// cast by Self.
        /// </summary>
        IStatBuilder Buff(IStatBuilder gainedStat, params IEntityBuilder[] affectedEntites);

        /// <summary>
        /// Returns an aura providing <paramref name="gainedStat"/> and affecting <paramref name="affectedEntites"/>
        /// cast by Self.
        /// </summary>
        IStatBuilder Aura(IStatBuilder gainedStat, params IEntityBuilder[] affectedEntites);

        /// <summary>
        /// Returns a collection of all buffs that currently affect any of <paramref name="targets"/> and originate from
        /// <paramref name="source"/>.
        /// <para>If no source and/or target is given, it defaults to any entity, e.g. Buffs() without parameters
        /// returns every active buff.</para>
        /// </summary>
        IBuffBuilderCollection Buffs(IEntityBuilder source = null, params IEntityBuilder[] targets);

        IStatBuilder CurseLimit { get; }
    }


    /// <summary>
    /// Factory for Conflux buffs.
    /// </summary>
    public interface IConfluxBuffBuilders
    {
        /// <summary>
        /// Gets a buff representing Igniting Conflux.
        /// </summary>
        IBuffBuilder Igniting { get; }

        /// <summary>
        /// Gets a buff representing Shocking Conflux.
        /// </summary>
        IBuffBuilder Shocking { get; }

        /// <summary>
        /// Gets a buff representing Chilling Conflux.
        /// </summary>
        IBuffBuilder Chilling { get; }

        /// <summary>
        /// Gets a buff representing Elemental Conflux.
        /// </summary>
        IBuffBuilder Elemental { get; }
    }
}