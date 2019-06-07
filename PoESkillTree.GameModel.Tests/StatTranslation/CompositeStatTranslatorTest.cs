using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;

namespace PoESkillTree.GameModel.StatTranslation
{
    [TestFixture]
    public class CompositeStatTranslatorTest
    {
        [Test]
        public void TranslateReturnsCorrectResult()
        {
            var untranslatedStats = new[]
            {
                new UntranslatedStat("0", 0),
                new UntranslatedStat("1", 1),
                new UntranslatedStat("2", 2),
                new UntranslatedStat("3", 3),
                new UntranslatedStat("4", 4),
            };
            var componentParameters = new IReadOnlyList<UntranslatedStat>[]
            {
                untranslatedStats,
                untranslatedStats.Skip(2).ToList(),
                untranslatedStats.Skip(3).ToList(),
                untranslatedStats.Skip(4).ToList(),
            };
            var componentResults = new[]
            {
                new StatTranslatorResult(new[] { "t0", "t1" }, componentParameters[1]),
                new StatTranslatorResult(new[] { "t2" }, componentParameters[2]),
                new StatTranslatorResult(new[] { "t3" }, componentParameters[3]),
            };
            var components = new[]
            {
                Mock.Of<IStatTranslator>(t => t.Translate(componentParameters[0]) == componentResults[0]),
                Mock.Of<IStatTranslator>(t => t.Translate(componentParameters[1]) == componentResults[1]),
                Mock.Of<IStatTranslator>(t => t.Translate(componentParameters[2]) == componentResults[2]),
            };
            var expected =
                new StatTranslatorResult(new[] { "t0", "t1", "t2", "t3" }, componentParameters[3]);
            var sut = new CompositeStatTranslator(components);

            var actual = sut.Translate(untranslatedStats);

            Assert.AreEqual(expected, actual);
        }
    }
}