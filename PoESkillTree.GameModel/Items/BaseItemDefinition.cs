using System.Collections.Generic;

namespace PoESkillTree.GameModel.Items
{
    public class BaseItemDefinition
    {
        public string MetadataId { get; }

        public string Name { get; }
        public ItemClass ItemClass { get; }
        public IEnumerable<string> RawTags { get; }
        public Tags Tags { get; }

        public IReadOnlyList<Property> Properties { get; }

        public Requirements Requirements { get; }

        public IReadOnlyList<CraftableStat> ImplicitModifiers { get; }

        public int InventoryHeight { get; }
        public int InventoryWidth { get; }

        public int DropLevel { get; }

        public ReleaseState ReleaseState { get; }

        public string VisualIdentity { get; }
    }

    public class Property
    {
        public Property(string name, int value) => (Name, Value) = (name, value);

        public string Name { get; }
        public int Value { get; }

        public override bool Equals(object obj)
            => obj is Property other && Equals(other);

        private bool Equals(Property other)
            => Name == other.Name && Value == other.Value;

        public override int GetHashCode()
            => (Name, Value).GetHashCode();

        public override string ToString()
            => $"{Name}: {Value}";
    }

    public class CraftableStat
    {
        public CraftableStat(string statId, int minValue, int maxValue)
            => (StatId, MinValue, MaxValue) = (statId, minValue, maxValue);

        public string StatId { get; }
        public int MinValue { get; }
        public int MaxValue { get; }

        public override bool Equals(object obj)
            => obj is CraftableStat other && Equals(other);

        private bool Equals(CraftableStat other)
            => StatId == other.StatId && MinValue == other.MinValue && MaxValue == other.MaxValue;

        public override int GetHashCode()
            => (StatId, MinValue, MaxValue).GetHashCode();
    }
}