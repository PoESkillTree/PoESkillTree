using System.Collections.Generic;
using PoESkillTree.Utils;

namespace PoESkillTree.GameModel.Items
{
    public class Item : ValueObject
    {
        public Item(
            string baseMetadataId, string name, int quality, int requiredLevel, FrameType frameType, bool isCorrupted,
            IReadOnlyList<string> modifiers, bool isEnabled)
        {
            BaseMetadataId = baseMetadataId;
            Name = name;
            Quality = quality;
            RequiredLevel = requiredLevel;
            FrameType = frameType;
            IsCorrupted = isCorrupted;
            Modifiers = modifiers;
            IsEnabled = isEnabled;
        }

        public string BaseMetadataId { get; }

        public string Name { get; }
        public int Quality { get; }
        public int RequiredLevel { get; }

        public FrameType FrameType { get; }
        public bool IsCorrupted { get; }

        public IReadOnlyList<string> Modifiers { get; }

        public bool IsEnabled { get; }

        protected override object ToTuple()
            => (BaseMetadataId, Name, Quality, RequiredLevel, FrameType, IsCorrupted, WithSequenceEquality(Modifiers),
                IsEnabled);
    }
}