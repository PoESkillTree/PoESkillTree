using System.Collections.Generic;
using PoESkillTree.GameModel.Modifiers;

namespace PoESkillTree.GameModel.Items
{
    public class Item
    {
        public Item(
            string baseMetadataId, string name, int quality, int requiredLevel,
            IReadOnlyDictionary<ModLocation, IReadOnlyList<string>> modifiers)
        {
            BaseMetadataId = baseMetadataId;
            Name = name;
            Quality = quality;
            RequiredLevel = requiredLevel;
            Modifiers = modifiers;
        }

        public string BaseMetadataId { get; }

        public string Name { get; }
        public int Quality { get; }
        public int RequiredLevel { get; }

        public IReadOnlyDictionary<ModLocation, IReadOnlyList<string>> Modifiers { get; }
    }
}