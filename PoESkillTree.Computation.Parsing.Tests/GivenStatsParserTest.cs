using System.Linq;
using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Forms;
using PoESkillTree.Computation.Common.Builders.Modifiers;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.Computation.Common.Builders.Values;
using PoESkillTree.Computation.Common.Data;
using PoESkillTree.Computation.Common.Parsing;
using PoESkillTree.Computation.Common.Tests;
using PoESkillTree.Utils;

namespace PoESkillTree.Computation.Parsing.Tests
{
    [TestFixture]
    public class GivenStatsParserTest
    {
        [Test]
        public void ParseIsEmptyWithNoGivenStats()
        {
            var actual = GivenStatsParser.Parse(Mock.Of<IParser>(), new IGivenStats[0]);

            Assert.IsEmpty(actual);
        }

        [Test]
        public void AddAddsFromStatLinesCorrectly()
        {
            var expected = Helper.MockManyModifiers(4);
            var givenStats = new[]
            {
                Mock.Of<IGivenStats>(s =>
                    s.AffectedEntities == new[] { Entity.Character, Entity.Totem } &&
                    s.GivenStatLines == new[] { "s1", "s2" } &&
                    s.GivenModifiers == new IIntermediateModifier[0])
            };
            var parseResults = expected.Select(m => new ParseResult(true, "", new[] { m })).ToList();
            var parser = Mock.Of<IParser>(p =>
                p.Parse("s1", new ModifierSource.Global(), Entity.Character) == parseResults[0] &&
                p.Parse("s2", new ModifierSource.Global(), Entity.Character) == parseResults[1] &&
                p.Parse("s1", new ModifierSource.Global(), Entity.Totem) == parseResults[2] &&
                p.Parse("s2", new ModifierSource.Global(), Entity.Totem) == parseResults[3]);

            var actual = GivenStatsParser.Parse(parser, givenStats);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void AddAddsFromStatsCorrectly()
        {
            var expectedStats = new[] { new StatStub(), };
            var expectedForm = Form.BaseSet;
            var expectedValue = new Constant(42);
            var expectedModifierSource = new ModifierSource.Global();
            var expected = new []
            {
                new Modifier(expectedStats, expectedForm, expectedValue, expectedModifierSource)
            };

            var buildParams = new BuildParameters(expectedModifierSource, Entity.Enemy, expectedForm);
            var formBuilderMock = new Mock<IFormBuilder>();
            formBuilderMock.Setup(b => b.Build())
                .Returns((expectedForm, Funcs.Identity));
            var statBuilderMock = new Mock<IStatBuilder>();
            statBuilderMock.Setup(b => b.Build(buildParams))
                .Returns(new[] { new StatBuilderResult(expectedStats, expectedModifierSource, Funcs.Identity), });
            var valueBuilder = Mock.Of<IValueBuilder>(b => b.Build(buildParams) == expectedValue);
            var intermediateModifier = new ModifierBuilder()
                .WithForm(formBuilderMock.Object).WithStat(statBuilderMock.Object).WithValue(valueBuilder)
                .Build();
            var givenStats = new[]
            {
                Mock.Of<IGivenStats>(s =>
                    s.AffectedEntities == new[] { Entity.Enemy } &&
                    s.GivenStatLines == new string[0] &&
                    s.GivenModifiers == new[] { intermediateModifier })
            };

            var actual = GivenStatsParser.Parse(Mock.Of<IParser>(), givenStats);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void AddThrowsIfStatLineIsNotSuccessfullyParsed()
        {
            var givenStats = new[]
            {
                Mock.Of<IGivenStats>(s =>
                    s.AffectedEntities == new[] { Entity.Character } &&
                    s.GivenStatLines == new[] { "s1" } &&
                    s.GivenModifiers == new IIntermediateModifier[0])
            };
            var parseResult = new ParseResult(false, "", new Modifier[0]);
            var parser = Mock.Of<IParser>(p =>
                p.Parse("s1", new ModifierSource.Global(), Entity.Character) == parseResult);

            Assert.Throws<ParseException>(() => GivenStatsParser.Parse(parser, givenStats));
        }
    }
}