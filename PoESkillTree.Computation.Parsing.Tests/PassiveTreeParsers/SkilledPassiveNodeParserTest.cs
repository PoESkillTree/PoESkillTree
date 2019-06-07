using NUnit.Framework;
using PoESkillTree.Computation.Common;
using PoESkillTree.GameModel.PassiveTree;
using static PoESkillTree.Computation.Parsing.ParserTestUtils;

namespace PoESkillTree.Computation.Parsing.PassiveTreeParsers
{
    [TestFixture]
    public class SkilledPassiveNodeParserTest
    {
        [TestCase((ushort) 1)]
        [TestCase((ushort) 42)]
        public void ReturnsCorrectModifier(ushort nodeId)
        {
            var definition = new PassiveNodeDefinition(nodeId, PassiveNodeType.Small, "", false,
                true, 0, default, new string[0]);
            var expected = new[]
            {
                CreateModifier($"{nodeId}.Skilled", Form.TotalOverride, 1, CreateGlobalSource(definition))
            };
            var treeDefinition = new PassiveTreeDefinition(new[] { definition });
            var sut = new SkilledPassiveNodeParser(treeDefinition, CreateBuilderFactories());

            var result = sut.Parse(nodeId);

            Assert.AreEqual(expected, result.Modifiers);
        }

        private static ModifierSource.Global CreateGlobalSource(PassiveNodeDefinition nodeDefinition)
            => new ModifierSource.Global(new ModifierSource.Local.PassiveNode(nodeDefinition.Id, nodeDefinition.Name));
    }
}