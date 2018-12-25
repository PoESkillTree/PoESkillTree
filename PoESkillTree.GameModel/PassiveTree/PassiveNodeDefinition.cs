using System.Collections.Generic;

namespace PoESkillTree.GameModel.PassiveTree
{
    public class PassiveNodeDefinition
    {
        public PassiveNodeDefinition(ushort id, PassiveNodeType type, string name, IReadOnlyList<string> modifiers)
            => (Id, Type, Name, Modifiers) = (id, type, name, modifiers);

        public ushort Id { get; }

        public PassiveNodeType Type { get; }
        public string Name { get; }
        public IReadOnlyList<string> Modifiers { get; }
    }
}