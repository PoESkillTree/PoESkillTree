using NUnit.Framework;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Parsing.PassiveTreeParsers;
using PoESkillTree.GameModel.PassiveTree;
using static PoESkillTree.Computation.Parsing.Tests.ParserTestUtils;

namespace PoESkillTree.Computation.Parsing.Tests.PassiveTreeParsers
{
    [TestFixture]
    public class SkilledPassiveNodeParserTest
    {
        [TestCase((ushort) 1)]
        [TestCase((ushort) 42)]
        public void ReturnsCorrectModifier(ushort nodeId)
        {
            var definition = new PassiveNodeDefinition(nodeId, PassiveNodeType.Normal, "", false,
                true, 0, new string[0]);
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
            => new ModifierSource.Global(new ModifierSource.Local.Tree(nodeDefinition.Name));
    }
}