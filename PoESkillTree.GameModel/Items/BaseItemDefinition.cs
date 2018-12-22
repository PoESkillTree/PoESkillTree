using System.Collections.Generic;
using PoESkillTree.Utils;

namespace PoESkillTree.GameModel.Items
{
    public class BaseItemDefinition
    {
        public BaseItemDefinition(
            string metadataId, string name, ItemClass itemClass, IEnumerable<string> rawTags, Tags tags,
            IReadOnlyList<Property> properties, IReadOnlyList<UntranslatedStat> buffStats, Requirements requirements,
            IReadOnlyList<CraftableStat> implicitModifiers, int inventoryHeight, int inventoryWidth, int dropLevel,
            ReleaseState releaseState, string visualIdentity)
        {
            MetadataId = metadataId;
            Name = name;
            ItemClass = itemClass;
            RawTags = rawTags;
            Tags = tags;
            Properties = properties;
            BuffStats = buffStats;
            Requirements = requirements;
            ImplicitModifiers = implicitModifiers;
            InventoryHeight = inventoryHeight;
            InventoryWidth = inventoryWidth;
            DropLevel = dropLevel;
            ReleaseState = releaseState;
            VisualIdentity = visualIdentity;
        }

        public string MetadataId { get; }

        public string Name { get; }
        public ItemClass ItemClass { get; }
        public IEnumerable<string> RawTags { get; }
        public Tags Tags { get; }

        public IReadOnlyList<Property> Properties { get; }

        /// <summary>
        /// Stats granted through a buff when this flask is active.
        /// </summary>
        public IReadOnlyList<UntranslatedStat> BuffStats { get; }

        public Requirements Requirements { get; }

        public IReadOnlyList<CraftableStat> ImplicitModifiers { get; }

        public int InventoryHeight { get; }
        public int InventoryWidth { get; }

        public int DropLevel { get; }

        public ReleaseState ReleaseState { get; }

        public string VisualIdentity { get; }
    }

    public class Property : ValueObject
    {
        public Property(string id, int value) => (Id, Value) = (id, value);

        public void Deconstruct(out string id, out int value) => (id, value) = (Id, Value);

        public string Id { get; }
        public int Value { get; }

        protected override object ToTuple() => (Id, Value);
    }

    public class CraftableStat : ValueObject
    {
        public CraftableStat(string statId, int minValue, int maxValue)
            => (StatId, MinValue, MaxValue) = (statId, minValue, maxValue);

        public string StatId { get; }
        public int MinValue { get; }
        public int MaxValue { get; }

        protected override object ToTuple() => (StatId, MinValue, MaxValue);
    }
}