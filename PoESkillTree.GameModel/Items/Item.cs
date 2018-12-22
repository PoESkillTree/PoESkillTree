using System.Collections.Generic;
using PoESkillTree.GameModel.Modifiers;

namespace PoESkillTree.GameModel.Items
{
    public class Item
    {
        public Item(
            string baseMetadataId, string name, int quality, int requiredLevel, FrameType frameType, bool isCorrupted,
            IReadOnlyDictionary<ModLocation, IReadOnlyList<string>> modifiers)
        {
            BaseMetadataId = baseMetadataId;
            Name = name;
            Quality = quality;
            RequiredLevel = requiredLevel;
            FrameType = frameType;
            IsCorrupted = isCorrupted;
            Modifiers = modifiers;
        }

        public string BaseMetadataId { get; }

        public string Name { get; }
        public int Quality { get; }
        public int RequiredLevel { get; }

        public FrameType FrameType { get; }
        public bool IsCorrupted { get; }

        public IReadOnlyDictionary<ModLocation, IReadOnlyList<string>> Modifiers { get; }
    }
}