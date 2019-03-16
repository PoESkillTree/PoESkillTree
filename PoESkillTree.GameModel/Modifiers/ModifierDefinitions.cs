using System;
using System.Collections.Generic;
using System.Linq;

namespace PoESkillTree.GameModel.Modifiers
{
    public class ModifierDefinitions
    {
        private readonly Lazy<IReadOnlyDictionary<string, ModifierDefinition>> _modifierDict;

        public ModifierDefinitions(IReadOnlyList<ModifierDefinition> modifiers)
        {
            Modifiers = modifiers;
            _modifierDict = new Lazy<IReadOnlyDictionary<string, ModifierDefinition>>(
                () => Modifiers.ToDictionary(s => s.Id));
        }

        public IReadOnlyList<ModifierDefinition> Modifiers { get; }

        public ModifierDefinition GetModifierById(string id) => _modifierDict.Value[id];
    }
}