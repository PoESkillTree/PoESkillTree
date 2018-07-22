using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Builders.Skills;
using PoESkillTree.Computation.Builders.Stats;
using PoESkillTree.Computation.Builders.Tests.Stats;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Builders.Tests.Skills
{
    [TestFixture]
    public class SkillBuilderTest
    {
        [TestCase(42)]
        [TestCase(1)]
        public void SkillIdBuildsToCorrectValue(int expected)
        {
            var coreBuilder = CoreBuilder.Create(new SkillDefinition("", expected));
            var sut = CreateSut(coreBuilder);

            var actual = sut.SkillId.Build().Calculate(null);

            Assert.AreEqual((NodeValue?) expected, actual);
        }

        [Test]
        public void SkillIdResolveBuildsToCorrectValue()
        {
            var expected = 42;
            var coreBuilder = CoreBuilder.Create(new SkillDefinition("", expected));
            var unresolved = Mock.Of<ICoreBuilder<SkillDefinition>>(b => b.Resolve(null) == coreBuilder);
            var sut = CreateSut(unresolved);

            var actual = sut.SkillId.Resolve(null).Build().Calculate(null);

            Assert.AreEqual((NodeValue?) expected, actual);
        }

        [Test]
        public void InstancesBuildsToCorrectResults()
        {
            var coreBuilder = CoreBuilder.Create(new SkillDefinition("skill", 0));
            var sut = CreateSut(coreBuilder);

            var stat = sut.Instances.BuildToSingleStat();

            Assert.AreEqual("skill.Instances", stat.Identity);
        }

        [Test]
        public void CastBuildsToCorrectResult()
        {
            var coreBuilder = CoreBuilder.Create(new SkillDefinition("skill", 0));
            var sut = CreateSut(coreBuilder);

            var actual = sut.Cast.Build();

            Assert.AreEqual("skill.Cast", actual);
        }

        private static SkillBuilder CreateSut(ICoreBuilder<SkillDefinition> coreBuilder) =>
            new SkillBuilder(new StatFactory(), coreBuilder);
    }
}