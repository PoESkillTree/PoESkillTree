using Moq;
using MoreLinq;
using NUnit.Framework;
using PoESkillTree.Computation.Builders.Skills;
using PoESkillTree.Computation.Builders.Stats;
using PoESkillTree.Computation.Common.Builders.Skills;

namespace PoESkillTree.Computation.Builders.Tests.Stats
{
    [TestFixture]
    public class CastSpeedStatBuilderTest
    {
        [Test]
        public void WithKeywordCallsStatFactoryCorrectly()
        {
            var statFactoryMock = new Mock<IStatFactory>();
            var sut = new CastSpeedStatBuilder(statFactoryMock.Object);

            var withKeyword = sut.With(new KeywordBuilder(Keyword.Projectile));
            withKeyword.Build(default, null).Consume();

            statFactoryMock.Verify(f => f.ActiveSkillPartCastSpeedHasKeyword(default, Keyword.Projectile));
        }
    }
}