using System.Collections.Generic;
using System.Linq;
using Moq;
using MoreLinq;
using NUnit.Framework;
using PoESkillTree.Computation.Builders.Stats;
using PoESkillTree.Computation.Common;
using PoESkillTree.GameModel.Items;
using PoESkillTree.GameModel.Skills;
using static PoESkillTree.Computation.Common.Helper;
using static PoESkillTree.Computation.Parsing.SkillParsers.SkillParserTestUtils;

namespace PoESkillTree.Computation.Parsing.SkillParsers
{
    [TestFixture]
    public class SkillsParserTest
    {
        [Test]
        public void ParsesSingleActiveSkillCorrectly()
        {
            var expected = CreateParseResultForActive("0");
            var skill = CreateSkill("0", 0);
            var sut = CreateSut();

            var actual = sut.Parse(new[] { skill });

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ParsesSingleSupportedActiveSkillCorrectly()
        {
            var expected = CreateParseResult("0", "a", "b");
            var active = CreateSkill("0", 0);
            var supports = new[] { CreateSkill("a", 0), CreateSkill("b", 0) };
            var sut = CreateSut();

            var actual = sut.Parse(supports.Prepend(active).ToList());

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void UsesSupportabilityTester()
        {
            var expected = CreateParseResult("0", "b");
            var active = CreateSkill("0", 0);
            var supports = new[] { CreateSkill("a", 1), CreateSkill("b", 0) };
            var sut = CreateSut();

            var actual = sut.Parse(supports.Prepend(active).ToList());

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ParseMultipleSupportedActiveSkillsCorrectly()
        {
            var expected = ParseResult.Aggregate(new[]
            {
                CreateParseResult("0", "b", "c"),
                CreateParseResult("1", "b"),
                CreateParseResult("2", "a", "b", "d"),
            });
            var actives = new[]
            {
                CreateSkill("0", 0),
                CreateSkill("1", 1),
                CreateSkill("2", 2),
            };
            var supports = new[]
            {
                CreateSkill("a", 2), 
                CreateSkill("b", null),
                CreateSkill("c", 0),
                CreateSkill("d", 2),
            };
            var sut = CreateSut();

            var actual = sut.Parse(actives.Concat(supports).ToList());

            Assert.AreEqual(expected, actual);
        }

        private static SkillsParser CreateSut()
        {
            var activeParser = new Mock<IParser<Skill>>();
            activeParser.Setup(p => p.Parse(It.IsAny<Skill>()))
                .Returns((Skill s) => CreateParseResultForActive(s.Id));
            var supportParser = new Mock<IParser<SupportSkillParserParameter>>();
            supportParser.Setup(p => p.Parse(It.IsAny<SupportSkillParserParameter>()))
                .Returns((SupportSkillParserParameter p)
                    => CreateParseResultForSupport(p.ActiveSkill.Id, p.SupportSkill.Id));
            return new SkillsParser(CreateSkillDefinitions(), activeParser.Object, supportParser.Object);
        }

        private static SkillDefinitions CreateSkillDefinitions()
        {
            var actives = Enumerable.Range(0, 3).Select(i => CreateActive(
                i.ToString(),
                CreateActiveSkillDefinition(i.ToString(), activeSkillTypes: new[] { "ast" }),
                new Dictionary<int, SkillLevelDefinition>()));
            var supports = Enumerable.Range(0, 4).Select(i => CreateSupport(
                ((char) (i + 97)).ToString(),
                CreateSupportSkillDefinition(new[] { "ast" }),
                new Dictionary<int, SkillLevelDefinition>()));
            return new SkillDefinitions(actives.Concat(supports).ToList());
        }

        private static Skill CreateSkill(string id, int? gemGroup)
            => new Skill(id, 1, 0, ItemSlot.Belt, 0, gemGroup);

        private static ParseResult CreateParseResultForActive(string activeId)
            => ParseResult.Success(new[] { CreateModifier(activeId) });

        private static ParseResult CreateParseResultForSupport(string activeId, string supportId)
            => ParseResult.Success(new[] { CreateModifier($"{activeId} {supportId}") });

        private static ParseResult CreateParseResult(string activeId, params string[] supportIds)
        {
            var modifiers = supportIds
                .Select(s => CreateModifier($"{activeId} {s}"))
                .Prepend(CreateModifier(activeId));
            return ParseResult.Success(modifiers.ToList());
        }

        private static Modifier CreateModifier(string id)
            => MockModifier(new Stat(id), value: new Constant(0));
    }
}