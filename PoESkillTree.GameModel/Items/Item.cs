using System.Collections.Generic;

namespace PoESkillTree.GameModel.Items
{
    public class Item
    {
        public Item(
            string baseMetadataId, string name, int quality, int requiredLevel,
            IReadOnlyList<string> implicitModifiers, IReadOnlyList<string> corruptionModifiers,
            IReadOnlyList<string> enchantmentModifiers, IReadOnlyList<string> explicitModifiers,
            IReadOnlyList<string> craftedModifiers)
        {
            BaseMetadataId = baseMetadataId;
            Name = name;
            Quality = quality;
            RequiredLevel = requiredLevel;
            ImplicitModifiers = implicitModifiers;
            CorruptionModifiers = corruptionModifiers;
            EnchantmentModifiers = enchantmentModifiers;
            ExplicitModifiers = explicitModifiers;
            CraftedModifiers = craftedModifiers;
        }

        public string BaseMetadataId { get; }

        public string Name { get; }
        public int Quality { get; }
        public int RequiredLevel { get; }

        public IReadOnlyList<string> ImplicitModifiers { get; }
        public IReadOnlyList<string> CorruptionModifiers { get; }
        public IReadOnlyList<string> EnchantmentModifiers { get; }
        public IReadOnlyList<string> ExplicitModifiers { get; }
        public IReadOnlyList<string> CraftedModifiers { get; }
    }
}