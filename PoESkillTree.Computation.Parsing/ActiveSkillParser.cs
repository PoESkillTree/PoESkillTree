using PoESkillTree.Computation.Common;
using PoESkillTree.GameModel.Items;
using PoESkillTree.GameModel.Skills;

namespace PoESkillTree.Computation.Parsing
{
    public class ActiveSkillParser : IParser<Skill>
    {
        private readonly SkillDefinitions _skillDefinitions;

        public ActiveSkillParser(SkillDefinitions skillDefinitions)
            => _skillDefinitions = skillDefinitions;

        public ParseResult Parse(Skill parameter)
        {
            return ParseResult.Success(new Modifier[0]);
        }
    }

    public struct Skill
    {
        public Skill(string id, int level, int quality, ItemSlot itemSlot, int socketIndex, int? gemGroup)
            => (Id, Level, Quality, ItemSlot, SocketIndex, GemGroup) =
                (id, level, quality, itemSlot, socketIndex, gemGroup);

        public string Id { get; }
        public int Level { get; }
        public int Quality { get; }

        public ItemSlot ItemSlot { get; }

        // Null: item inherent skill
        public int? GemGroup { get; }

        public int SocketIndex { get; }
    }
}