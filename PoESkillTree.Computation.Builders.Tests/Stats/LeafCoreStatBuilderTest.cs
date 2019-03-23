using System.Linq;
using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Builders.Stats
{
    // Most of the tests for LeafCoreStatBuilder are in StatBuilderTest. This only tests things not tested through
    // StatBuilder.
    [TestFixture]
    public class LeafCoreStatBuilderTest
    {
        [Test]
        public void FromIdentityBuildsToCorrectStat()
        {
            var expected = Mock.Of<IStat>();
            var statFactory = Mock.Of<IStatFactory>(f => f.FromIdentity("test", default, typeof(int), null) == expected);

            var statBuilder = LeafCoreStatBuilder.FromIdentity(statFactory, "test", typeof(int));
            var stats = statBuilder.Build(default).Single().Stats;

            Assert.AreEqual(expected, stats.Single());
        }
    }
}