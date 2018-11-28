using PoESkillTree.GameModel.Items;

namespace PoESkillTree.GameModel.Skills
{
    public class Skill
    {
        public Skill(string id, int level, int quality, ItemSlot itemSlot, int socketIndex, int? gemGroup)
            => (Id, Level, Quality, ItemSlot, SocketIndex, GemGroup) =
                (id, level, quality, itemSlot, socketIndex, gemGroup);

        public string Id { get; }
        public int Level { get; }
        public int Quality { get; }

        public ItemSlot ItemSlot { get; }
        public int SocketIndex { get; }

        /// <summary>
        /// The skill's gem/link group. Null for item-inherent skills.
        /// </summary>
        public int? GemGroup { get; }

        public override string ToString()
            => $"{Id}(level: {Level}, quality: {Quality}, {ItemSlot}.{SocketIndex}, group: {GemGroup})";
    }
}