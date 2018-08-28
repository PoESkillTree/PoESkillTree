using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using PoESkillTree.GameModel.Items;

namespace PoESkillTree.Computation.Common
{
    /// <summary>
    /// Union data type representing a source of a modifier.
    /// <para>
    /// Generally, each instance of a leaf ModifierSource class is considered the same, e.g. all instances of
    /// <see cref="Global"/> are equal to each other. The only exception to this is <see cref="Local.Item"/>, which
    /// also consists of an <see cref="ItemSlot"/>.
    /// </para>
    /// <para>
    /// Other information in an instance, e.g. <see cref="SourceName"/>, does not influence equality and is not
    /// contained in <see cref="CanonicalSource"/> instances.
    /// </para>
    /// </summary>
    /// <remarks>
    /// It is guaranteed that <see cref="ModifierSource"/> only has the subclasses that are inner classes of it:
    /// All abstract classes have private constructors and all non-abstract classes are sealed.
    /// </remarks>
    public abstract class ModifierSource : IEquatable<ModifierSource>
    {
        private ModifierSource(
            ModifierSource canonicalSource, string sourceName, params ModifierSource[] influencingSources)
        {
            CanonicalSource = canonicalSource ?? this;
            SourceName = sourceName;
            InfluencingSources = CanonicalSource.Concat(influencingSources).ToList();
        }

        private ModifierSource(params ModifierSource[] influencingSources)
            : this(null, "", influencingSources)
        {
        }
        
        /// <summary>
        /// The (canonical) modifier sources of increase/more modifiers influencing base values of this source,
        /// including this source itself.
        /// <para>
        /// E.g.:
        /// Global: Global
        /// Local.Item(BodyArmour): Local.Item(BodyArmour), Global
        /// </para>
        /// </summary>
        public IReadOnlyList<ModifierSource> InfluencingSources { get; }
        
        /// <summary>
        /// This instance but only containing data necessary for determining equivalence, no additional infos.
        /// E.g. <see cref="SourceName"/> returns an empty string and <see cref="ToString"/> will always return the
        /// class name.
        /// </summary>
        /// <remarks>
        /// These sources are stored in stat graph nodes and returned by <see cref="InfluencingSources"/>.
        /// </remarks>
        public ModifierSource CanonicalSource { get; }

        public static bool operator ==(ModifierSource left, ModifierSource right) =>
            left?.Equals(right) ?? right is null;

        public static bool operator !=(ModifierSource left, ModifierSource right) =>
            !(left == right);

        public sealed override bool Equals(object obj) => 
            ReferenceEquals(obj, this) || (obj is ModifierSource other && Equals(other));

        public virtual bool Equals(ModifierSource other) =>
            GetType() == other?.GetType();

        public override int GetHashCode() =>
            GetType().GetHashCode();

        /// <summary>
        /// String representation of this source for UI display purposes. E.g. Given, Helmet or Skill.
        /// </summary>
        public override string ToString() => GetType().Name;

        /// <summary>
        /// The detailed name of this source. E.g. tree node names, description of given mods like
        /// "x per y dexterity", item names or skill names.
        /// </summary>
        public string SourceName { get; }

        
        /// <summary>
        /// This modifier is global. Includes mods from the tree, given mods, most? mods on items and some mods from
        /// skills.
        /// </summary>
        public sealed class Global : ModifierSource
        {
            private readonly Local _localSource;

            /// <param name="localSource">The further differentiated source this source is the global version of</param>
            public Global(Local localSource) : base(new Global(), localSource.SourceName)
            {
                _localSource = localSource;
            }

            public Global()
            {
            }

            public override string ToString() => _localSource?.ToString() ?? base.ToString();
        }

        
        /// <summary>
        /// This modifier is local. Includes some mods on items and most mods from skills.
        /// </summary>
        public abstract class Local : ModifierSource
        {
            private Local(ModifierSource canonicalSource, string sourceName) 
                : base(canonicalSource, sourceName, new Global())
            {
            }

            private Local() : base(new Global())
            {
            }

            public sealed class Given : Local
            {
                public Given(string sourceName) : base(new Given(), sourceName)
                {
                }

                public Given()
                {
                }
            }

            public sealed class Tree : Local
            {
                public Tree(string treeNodeName) : base(new Tree(), treeNodeName)
                {
                }

                public Tree()
                {
                }
            }

            public sealed class Item : Local
            {
                public Item(ItemSlot slot, string itemName) : base(new Item(slot), itemName)
                {
                    Slot = slot;
                }

                public Item(ItemSlot slot)
                {
                    Slot = slot;
                }

                public ItemSlot Slot { get; }

                public override bool Equals(ModifierSource other) => other is Item item && Slot == item.Slot;

                public override int GetHashCode() => Slot.GetHashCode();

                public override string ToString() => Slot.ToString();
            }

            public sealed class Skill : Local
            {
                public Skill(string skillName) : base(new Skill(), skillName)
                {
                }

                public Skill()
                {
                }
            }
        }
    }
}