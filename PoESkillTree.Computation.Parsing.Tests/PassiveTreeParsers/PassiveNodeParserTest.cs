using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Parsing.PassiveTreeParsers;
using PoESkillTree.GameModel;
using PoESkillTree.GameModel.PassiveTree;
using static PoESkillTree.Computation.Parsing.Tests.ParserTestUtils;

namespace PoESkillTree.Computation.Parsing.Tests.PassiveTreeParsers
{
    [TestFixture]
    public class PassiveNodeParserTest
    {
        [Test]
        public void ParsesModifiersCorrectly()
        {
            var definition =
                new PassiveNodeDefinition(42, PassiveNodeType.Normal, "node", new[] { "+5 to maximum Life" });
            var source = CreateGlobalSource(definition);
            var coreResult = CreateModifier("Life", Form.BaseAdd, 2, source);
            var expected = new[]
            {
                CreateModifier("Life", Form.BaseAdd, new FunctionalValue(_ => null,
                    $"Character.{definition.Id}.Skilled.Value(Total, Global).IsSet ? 2 : null"), source)
            };
            var coreParser = Mock.Of<ICoreParser>(p =>
                p.Parse(new CoreParserParameter("+5 to maximum Life", source, Entity.Character))
                == ParseResult.Success(new[] { coreResult }));
            var sut = CreateSut(definition, coreParser);

            var result = sut.Parse(definition.Id);

            Assert.AreEqual(expected, result.Modifiers);
        }

        [Test]
        public void AddsSkilledModifierForKeystones()
        {
            var definition = new PassiveNodeDefinition(2, PassiveNodeType.Keystone, "keystone", new string[0]);
            var source = CreateGlobalSource(definition);
            var expected = new[]
            {
                CreateModifier($"{definition.Name}.Skilled", Form.TotalOverride, new FunctionalValue(_ => null,
                    $"Character.{definition.Id}.Skilled.Value(Total, Global).IsSet ? 1 : null"), source)
            };
            var sut = CreateSut(definition);

            var result = sut.Parse(definition.Id);

            Assert.AreEqual(expected, result.Modifiers);
        }

        private static PassiveNodeParser CreateSut(PassiveNodeDefinition nodeDefinition, ICoreParser coreParser = null)
        {
            coreParser = coreParser ?? Mock.Of<ICoreParser>();
            var treeDefinition = new PassiveTreeDefinition(new[] { nodeDefinition });
            return new PassiveNodeParser(treeDefinition, CreateBuilderFactories(), coreParser);
        }

        private static ModifierSource.Global CreateGlobalSource(PassiveNodeDefinition nodeDefinition)
            => new ModifierSource.Global(new ModifierSource.Local.Tree(nodeDefinition.Name));
    }
}