using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Common;
using PoESkillTree.GameModel;
using PoESkillTree.GameModel.StatTranslation;

namespace PoESkillTree.Computation.Parsing.Tests
{
    [TestFixture]
    public class UntranslatedStatParserTest
    {
        [Test]
        public void ParseReturnsCorrectResult()
        {
            var untranslatedStats = new[]
            {
                new UntranslatedStat("a", 1),
                new UntranslatedStat("b", 2),
                new UntranslatedStat("c", 3),
            };
            var modifierLines = new[] { "a1", "b2" };
            var statTranslatorResult = new StatTranslatorResult(modifierLines, new UntranslatedStat[0]);
            var translator = Mock.Of<IStatTranslator>(t => t.Translate(untranslatedStats) == statTranslatorResult);
            var localSource = new ModifierSource.Local.Skill();
            var globalSource = new ModifierSource.Global(localSource);
            var coreParserParameters = new[]
            {
                new CoreParserParameter(modifierLines[0], globalSource, Entity.Character),
                new CoreParserParameter(modifierLines[1], globalSource, Entity.Character),
            };
            var parseResults = new[]
            {
                ParseResult.Empty,
                ParseResult.Failure("b2", ""),
            };
            var coreParser = Mock.Of<ICoreParser>(p =>
                p.Parse(coreParserParameters[0]) == parseResults[0] &&
                p.Parse(coreParserParameters[1]) == parseResults[1]);

            var sut = new UntranslatedStatParser(translator, coreParser);
            var parserParameter = new UntranslatedStatParserParameter(localSource, untranslatedStats);
            var expected = ParseResult.Aggregate(parseResults);

            var actual = sut.Parse(parserParameter);

            Assert.AreEqual(expected, actual);
        }
    }
}