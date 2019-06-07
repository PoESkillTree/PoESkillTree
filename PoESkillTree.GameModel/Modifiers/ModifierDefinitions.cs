using System.Collections.Generic;

namespace PoESkillTree.GameModel.Modifiers
{
    public class ModifierDefinitions : DefinitionsBase<string, ModifierDefinition>
    {
        public ModifierDefinitions(IReadOnlyList<ModifierDefinition> modifiers) : base(modifiers)
        {
        }

        public IReadOnlyList<ModifierDefinition> Modifiers => Definitions;

        public ModifierDefinition GetModifierById(string id) => GetDefinitionById(id);
    }
}