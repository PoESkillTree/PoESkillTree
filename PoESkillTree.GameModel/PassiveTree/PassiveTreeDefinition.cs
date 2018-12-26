using System;
using System.Collections.Generic;
using System.Linq;

namespace PoESkillTree.GameModel.PassiveTree
{
    public class PassiveTreeDefinition
    {
        private readonly Lazy<IReadOnlyDictionary<ushort, PassiveNodeDefinition>> _nodeDict;

        public PassiveTreeDefinition(IReadOnlyList<PassiveNodeDefinition> nodes)
        {
            Nodes = nodes;
            _nodeDict = new Lazy<IReadOnlyDictionary<ushort, PassiveNodeDefinition>>(
                () => Nodes.ToDictionary(s => s.Id));
        }

        public IReadOnlyList<PassiveNodeDefinition> Nodes { get; }

        public PassiveNodeDefinition GetNodeById(ushort id) => _nodeDict.Value[id];

        // TODO Replace by real skill tree data
        public static PassiveTreeDefinition CreateKeystoneDefinitions()
        {
            ushort id = 0;
            var keystones = new[]
            {
                "Acrobatics", "Ancestral Bond", "Arrow Dancing", "Avatar of Fire", "Blood Magic", "Chaos Inoculation",
                "Conduit", "Crimson Dance", "Eldritch Batter", "Elemental Equilibrium", "Elemental Overload",
                "Ghost Reaver", "Iron Grip", "Iron Reflexes", "Mind Over Matter", "Minion Instability",
                "Necromantic Aegis", "Pain Attunement", "Perfect Agony", "Phase Acrobatics", "Point Blank",
                "Resolute Technique", "Runebinder", "Unwavering Stance", "Vaal Pact", "Zealot's Oath",
            };
            return new PassiveTreeDefinition(keystones.Select(Create).ToList());

            PassiveNodeDefinition Create(string name)
                => new PassiveNodeDefinition(id++, PassiveNodeType.Keystone, name, new string[0]);
        }
    }
}