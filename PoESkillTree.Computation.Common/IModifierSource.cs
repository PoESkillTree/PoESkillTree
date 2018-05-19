using System;
using System.Collections.Generic;

namespace PoESkillTree.Computation.Common
{
    /// <summary>
    /// Interface for the sources of modifiers.
    /// </summary>
    public interface IModifierSource : IEquatable<IModifierSource>
    {
        ModifierSourceFirstLevel FirstLevel { get; }
        // Representation of further levels is not yet decided. For most, it should be enough to simply include it
        // in Equals() calculations.
        // At least Local->Skill needs to be surfaced as conversion behaviors need to access it.

        /// <summary>
        /// The last differentiation level of this source for UI display purposes. E.g. Given, Helmet or Skill.
        /// </summary>
        string LastLevel { get; }

        /// <summary>
        /// The detailed name of this source. E.g. tree node names, description of given mods like
        /// "x per y dexterity", item names or skill names.
        /// </summary>
        string SourceName { get; }

        /// <summary>
        /// The (canonical) modifier sources of increase/more modifiers influencing base values of this source,
        /// including this source itself.
        /// </summary>
        /// <remarks>
        /// E.g.:
        /// Global: Global
        /// Local->Item->BodyArmour: Local->Item->BodyArmour, Global, (Local->Item if such modifiers exist)
        /// </remarks>
        IReadOnlyList<IModifierSource> InfluencingSources { get; }

        /// <summary>
        /// This instance but only containing data necessary for determining equivalence, no additional infos.
        /// E.g. <see cref="SourceName"/> returns an empty string and <see cref="LastLevel"/> will be Global if
        /// <see cref="FirstLevel"/> is Global.
        /// </summary>
        /// <remarks>
        /// These sources are stored in stat graph nodes and returned by <see cref="InfluencingSources"/>.
        /// </remarks>
        IModifierSource CanonicalSource { get; }
    }


    /// <summary>
    /// The first level of differentiation for <see cref="IModifierSource"/>s.
    /// </summary>
    public enum ModifierSourceFirstLevel
    {
        /// <summary>
        /// This modifier is global. Includes mods from the tree, given mods, most? mods on items and some mods from
        /// skills.
        /// <para>
        /// All global mods are considered coming from the same source in calculations, independent of further
        /// details contained in the source (like those for Local).
        /// </para>
        /// </summary>
        Global,
        /// <summary>
        /// This modifier is local. Includes some mods on items and most mods from skills.
        /// <para>
        /// The next level of differentiation is Given, Tree, Skill or Item. Item is further differentiated by item
        /// slot.
        /// </para>
        /// </summary>
        Local,
        // Not quite sure yet, probably necessary to allow separate calculations between damage over time directly from
        // skills and from ailments.
        // The second level specifies the ailment: Poison, Bleed or Ignite.
        // Inheriting the source from the original Hit type damage might also be necessary.
        Ailment
    }


    /// <summary>
    /// The canonical <see cref="IModifierSource"/> with <see cref="ModifierSourceFirstLevel.Global"/>.
    /// </summary>
    public class GlobalModifierSource : IModifierSource
    {
        public ModifierSourceFirstLevel FirstLevel => ModifierSourceFirstLevel.Global;
        public string LastLevel => FirstLevel.ToString();
        public string SourceName => "";
        public IReadOnlyList<IModifierSource> InfluencingSources => new[] { this };
        public IModifierSource CanonicalSource => this;

        public override bool Equals(object other) => 
            other is IModifierSource s && Equals(s);

        public bool Equals(IModifierSource other) =>
            (other != null) && (other.FirstLevel == ModifierSourceFirstLevel.Global);

        public override int GetHashCode() => FirstLevel.GetHashCode();

        public override string ToString() => LastLevel;
    }
}