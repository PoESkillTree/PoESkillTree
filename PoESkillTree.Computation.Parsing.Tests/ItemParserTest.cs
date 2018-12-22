using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Parsing.ItemParsers;
using PoESkillTree.GameModel;
using PoESkillTree.GameModel.Items;
using PoESkillTree.GameModel.Modifiers;

namespace PoESkillTree.Computation.Parsing.Tests
{
    [TestFixture]
    public class ItemParserTest
    {
        [Test]
        public void ParseReturnsCorrectResultForGlobalModifier()
        {
            var mods = new Dictionary<ModLocation, IReadOnlyList<string>>
            {
                [ModLocation.Implicit] = new[] { "+42 to maximum Life" }
            };
            var item = new Item("0", "item", 0, 0, mods);
            var local = new ModifierSource.Local.Item(ItemSlot.BodyArmour, item.Name);
            var global = new ModifierSource.Global(local);
            var expected = ParseResult.Success(
                new[] { new Modifier(new IStat[0], Form.BaseAdd, new Constant(2), global) });
            var coreParser = Mock.Of<ICoreParser>(p =>
                p.Parse(new CoreParserParameter("+42 to maximum Life", global, Entity.Character))
                == expected);
            var sut = new ItemParser(coreParser);

            var actual = sut.Parse(new ItemParserParameter(item, ItemSlot.BodyArmour));

            Assert.AreEqual(expected, actual);
        }
    }
}