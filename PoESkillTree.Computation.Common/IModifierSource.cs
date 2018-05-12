using System;
using System.Collections.Generic;

namespace PoESkillTree.Computation.Common
{
    public interface IModifierSource : IEquatable<IModifierSource>
    {
        // First level: ModifierSourceFirstLevel
        // Global and Local:
        // - Second level: Given, Tree, Skill or Item (maybe more).
        // - Third level: item slot for items
        // - For global, further levels are only for more detailed breakdowns. For calculation, all global sources are
        //   considered the same.
        // - Also contains information about e.g. tree node names, "Dexterity", item names, ...
        // Ailment second level: Poison, Bleed, Ignite
        // (if necessary, these could inherit further levels from the Hit type damage they originate from)
        ModifierSourceFirstLevel FirstLevel { get; }

        // The modifier sources of increase/more modifiers influence a base value of this source (including this source itself)
        // E.g.:
        // - Global: only Global
        // - Local->Item->BodyArmour: Local->Item->BodyArmour, Global, (Local->Item if such modifiers exist)
        IReadOnlyList<IModifierSource> InfluencingSources { get; }

        // Returns an instance that only contains data necessary for determining equivalence and no additional infos.
        // Such instances are what's stored in stat graph paths.
        IModifierSource ToCanonical();
    }


    public enum ModifierSourceFirstLevel
    {
        Global,
        Local,
        Ailment
    }


    public class GlobalModifierSource : IModifierSource
    {
        public ModifierSourceFirstLevel FirstLevel => ModifierSourceFirstLevel.Global;
        public IReadOnlyList<IModifierSource> InfluencingSources => new[] { this };
        public IModifierSource ToCanonical() => this;

        public override bool Equals(object other) => 
            other is IModifierSource s && Equals(s);

        public bool Equals(IModifierSource other) =>
            (other != null) && (other.FirstLevel == ModifierSourceFirstLevel.Global);

        public override int GetHashCode() => 1;
    }
}