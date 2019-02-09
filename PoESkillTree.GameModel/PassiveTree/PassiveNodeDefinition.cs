using System.Collections.Generic;

namespace PoESkillTree.GameModel.PassiveTree
{
    public class PassiveNodeDefinition
    {
        public PassiveNodeDefinition(
            ushort id, PassiveNodeType type, string name, bool isAscendancyNode, bool costsPassivePoint,
            int passivePointsGranted, IReadOnlyList<string> modifiers)
            => (Id, Type, Name, IsAscendancyNode, CostsPassivePoint, PassivePointsGranted, Modifiers) =
                (id, type, name, isAscendancyNode, costsPassivePoint, passivePointsGranted, modifiers);

        public ushort Id { get; }

        public PassiveNodeType Type { get; }
        public string Name { get; }
        public bool IsAscendancyNode { get; }
        public bool CostsPassivePoint { get; }
        public int PassivePointsGranted { get; }

        public IReadOnlyList<string> Modifiers { get; }
    }
}