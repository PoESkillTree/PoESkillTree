using System.Collections.Generic;
using System.Linq;

namespace PoESkillTree.GameModel.PassiveTree
{
    public class PassiveTreeDefinition : DefinitionsBase<ushort, PassiveNodeDefinition>
    {
        public PassiveTreeDefinition(IReadOnlyList<PassiveNodeDefinition> nodes) : base(nodes)
        {
        }

        public IReadOnlyList<PassiveNodeDefinition> Nodes => Definitions;

        public PassiveNodeDefinition GetNodeById(ushort id) => GetDefinitionById(id);

        // TODO Replace by real skill tree data
        public static IReadOnlyList<PassiveNodeDefinition> CreateKeystoneDefinitions()
        {
            var nodes = new List<PassiveNodeDefinition>
            {
                new PassiveNodeDefinition(0, PassiveNodeType.JewelSocket, "jewel", false,
                    true, 0, default, new string[0]),
                new PassiveNodeDefinition(1, PassiveNodeType.Normal, "attributes", false,
                    true, 0, new NodePosition(10, 10), 
                    new[] { "+100 to Strength", "+100 to Dexterity", "+100 to Intelligence" }),
            };

            ushort id = 2;
            var keystones = new[]
            {
                "Acrobatics", "Ancestral Bond", "Arrow Dancing", "Avatar of Fire", "Blood Magic", "Chaos Inoculation",
                "Conduit", "Crimson Dance", "Eldritch Batter", "Elemental Equilibrium", "Elemental Overload",
                "Ghost Reaver", "Iron Grip", "Iron Reflexes", "Mind Over Matter", "Minion Instability",
                "Necromantic Aegis", "Pain Attunement", "Perfect Agony", "Phase Acrobatics", "Point Blank",
                "Resolute Technique", "Runebinder", "Unwavering Stance", "Vaal Pact", "Zealot's Oath",
            };
            nodes.AddRange(keystones.Select(Create));
            return nodes;

            PassiveNodeDefinition Create(string name)
                => new PassiveNodeDefinition(id++, PassiveNodeType.Keystone, name, false,
                    true, 0, new NodePosition(id, 1000), new string[0]);
        }
    }
}