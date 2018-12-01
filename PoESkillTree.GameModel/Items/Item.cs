using System.Collections.Generic;

namespace PoESkillTree.GameModel.Items
{
    public class Item
    {
        public string BaseMetadataId { get; }

        public string Name { get; }
        public int Quality { get; }
        public Requirements Requirements { get; }

        public IReadOnlyList<string> ImplicitModifiers { get; }
        public IReadOnlyList<string> CorruptionModifiers { get; }
        public IReadOnlyList<string> EnchantmentModifiers { get; }
        public IReadOnlyList<string> ExplicitModifiers { get; }
        public IReadOnlyList<string> CraftedModifiers { get; }
    }
}