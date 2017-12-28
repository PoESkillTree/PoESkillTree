using PoESkillTree.Computation.Parsing.Builders.Entities;
using PoESkillTree.Computation.Parsing.Builders.Skills;
using PoESkillTree.Computation.Parsing.Builders.Stats;
using PoESkillTree.Computation.Parsing.Builders.Values;

namespace PoESkillTree.Computation.Parsing.Builders.Buffs
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
        /// Gets a buff factory that creates Conflux buffs.
        /// </summary>
        IConfluxBuffBuilderFactory Conflux { get; }

        // TODO this probably needs changes when other skills from items are added
        /// <summary>
        /// Returns a buff representing the curse (de-)buff applied by the skill <paramref name="skill"/> at level
        /// <paramref name="level"/>.
        /// </summary>
        /// <remarks>
        /// The stats of the skill starting with "cursed enemies ..." are the (de-)buff.
        /// </remarks>
        IBuffBuilder Curse(ISkillBuilder skill, IValueBuilder level);

        /// <summary>
        /// Returns a buff factory that can be used to create a buff rotation through different buffs with a total
        /// duration of <paramref name="duration"/> seconds.
        /// </summary>
        /// <remarks>
        /// Buff rotations originate from Self and target Self.
        /// <para> The currently active step in the rotation will need to be selected by the user.
        /// </para>
        /// </remarks>
        IBuffRotation Rotation(IValueBuilder duration);

        /// <summary>
        /// Returns a collection of all buffs that currently affect <paramref name="target"/> and originate from
        /// <paramref name="source"/>. The parameters default to any entity, e.g. Buffs() without parameters returns
        /// every active buff.
        /// </summary>
        IBuffBuilderCollection Buffs(IEntityBuilder source = null, IEntityBuilder target = null);
    }


    /// <summary>
    /// Factory for Conflux buffs.
    /// </summary>
    public interface IConfluxBuffBuilderFactory
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


    /// <summary>
    /// Factory for buff rotations. Buff rotations are stats whose value indicates that the entity with the stat is
    /// affected by the buff rotation.
    /// </summary>
    public interface IBuffRotation : IFlagStatBuilder
    {
        /// <summary>
        /// Returns a new buff rotation that is created by adding a new step to this buff rotation.
        /// </summary>
        /// <param name="duration">The duration of the new step.</param>
        /// <param name="buffs">The buffs gained while the new step is active.</param>
        IBuffRotation Step(IValueBuilder duration, params IBuffBuilder[] buffs);
    }
}