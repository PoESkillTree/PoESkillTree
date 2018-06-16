using System.Linq;
using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Builders.Stats;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Builders.Tests.Stats
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
            var statFactory = Mock.Of<IStatFactory>(f => f.FromIdentity("test", default, typeof(int), false) == expected);

            var statBuilder = LeafCoreStatBuilder.FromIdentity(statFactory, "test", typeof(int));
            var stats = statBuilder.Build(default, new ModifierSource.Global()).Single().Stats;

            Assert.AreEqual(expected, stats.Single());
        }
    }
}