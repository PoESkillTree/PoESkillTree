using System;
using System.Collections.Generic;
using PoESkillTree.GameModel.Items;
using PoESkillTree.Utils;

namespace PoESkillTree.Computation.Common
{
    /// <summary>
    /// Union data type representing a source of a modifier.
    /// <para>
    /// Other information in an instance, e.g. <see cref="SourceName"/>, does not influence equality and is not
    /// contained in <see cref="CanonicalSource"/> instances.
    /// </para>
    /// </summary>
    /// <remarks>
    /// It is guaranteed that <see cref="ModifierSource"/> only has the subclasses that are inner classes of it:
    /// All abstract classes have private constructors and all non-abstract classes are sealed.
    /// </remarks>
#pragma warning disable 660,661 // Equals and GetHashCode are overridden in ValueObject
    public abstract class ModifierSource : ValueObject, IEquatable<ModifierSource>
#pragma warning restore 660,661
    {
        private ModifierSource(
            ModifierSource canonicalSource, string sourceName, params ModifierSource[] influencingSources)
        {
            CanonicalSource = canonicalSource ?? this;
            SourceName = sourceName;
            var sources = new ModifierSource[influencingSources.Length + 1];
            sources[0] = CanonicalSource;
            Array.Copy(influencingSources, 0, sources, 1, influencingSources.Length);
            InfluencingSources = sources;
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
        /// This instance but containing only the information that is relevant for the calculation graph.
        /// E.g. <see cref="SourceName"/> returns an empty string.
        /// </summary>
        /// <remarks>
        /// These sources are stored in stat graph nodes and returned by <see cref="InfluencingSources"/>.
        /// </remarks>
        public ModifierSource CanonicalSource { get; }

        public static bool operator ==(ModifierSource left, ModifierSource right)
            => left?.Equals(right) ?? right is null;

        public static bool operator !=(ModifierSource left, ModifierSource right)
            => !(left == right);

        public bool Equals(ModifierSource other)
            => Equals((object) other);

        protected override object ToTuple()
            => (GetType(), SourceName);

        /// <summary>
        /// String representation of this source for UI display purposes. E.g. Given, Helmet or Skill.
        /// </summary>
        public override string ToString() => GetType().Name;

        /// <summary>
        /// The detailed name of this source. E.g. tree node names, description of given mods like
        /// "x per y dexterity", item names or skill ids.
        /// </summary>
        public string SourceName { get; }


        /// <summary>
        /// This modifier is global. Includes mods from the tree, given mods and most mods on items and skills.
        /// </summary>
        public sealed class Global : ModifierSource
        {
            /// <param name="localSource">The further differentiated source this source is the global version of</param>
            public Global(Local localSource) : base(new Global(), localSource.SourceName)
            {
                LocalSource = localSource;
            }

            public Global()
            {
            }

            public Local LocalSource { get; }

            protected override object ToTuple() => (base.ToTuple(), LocalSource);

            public override string ToString() => LocalSource?.ToString() ?? base.ToString();
        }


        /// <summary>
        /// This modifier is local. Includes some mods on items and skills and mods from gems.
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

            public sealed class PassiveNode : Local
            {
                public PassiveNode(ushort nodeId, string nodeName) : base(new PassiveNode(nodeId), nodeName)
                {
                    NodeId = nodeId;
                }

                public PassiveNode(ushort nodeId)
                {
                    NodeId = nodeId;
                }

                public ushort NodeId { get; }

                protected override object ToTuple() => (base.ToTuple(), NodeId);
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

                protected override object ToTuple() => (base.ToTuple(), Slot);

                public override string ToString() => Slot.ToString();
            }

            /// <summary>
            /// ModifierSource for jewels that are socketed into the skill tree.
            /// Other jewels use ModifierSource.Local.Item.
            /// </summary>
            public sealed class Jewel : Local
            {
                public Jewel(JewelRadius radius, ushort passiveNodeId, string itemName)
                    : base(new Jewel(radius, passiveNodeId), itemName)
                {
                    Radius = radius;
                    PassiveNodeId = passiveNodeId;
                }

                public Jewel(JewelRadius radius, ushort passiveNodeId)
                {
                    Radius = radius;
                    PassiveNodeId = passiveNodeId;
                }

                public JewelRadius Radius { get; }
                public ushort PassiveNodeId { get; }

                protected override object ToTuple() => (base.ToTuple(), Radius, PassiveNodeId);
            }

            public sealed class Skill : Local
            {
                public Skill(string skillId, string displayName) : base(new Skill(skillId), displayName)
                {
                    SkillId = skillId;
                }

                public Skill(string skillId)
                {
                    SkillId = skillId;
                }

                public string SkillId { get; }

                protected override object ToTuple() => (base.ToTuple(), SkillId);
            }

            /// <summary>
            /// As opposed to <see cref="ModifierSource.Local.Skill"/>, this modifier is provided by the gem itself.
            /// Skills not coming from gems, i.e. item-innate ones, don't have modifiers with this source.
            /// Atm, this is only used for level and attribute requirements.
            /// </summary>
            public sealed class Gem : Local
            {
                public Gem(ItemSlot slot, int socketIndex, string skillId, string displayName)
                    : base(new Gem(slot, socketIndex), displayName)
                    => (Slot, SocketIndex, SkillId) = (slot, socketIndex, skillId);

                public Gem(ItemSlot slot, int socketIndex)
                    => (Slot, SocketIndex) = (slot, socketIndex);

                public ItemSlot Slot { get; }
                public int SocketIndex { get; }
                public string SkillId { get; }

                protected override object ToTuple() => (base.ToTuple(), Slot, SocketIndex);
            }

            public sealed class UserSpecified : Local
            {
            }
        }
    }
}