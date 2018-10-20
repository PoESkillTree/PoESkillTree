using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Builders.Skills;
using PoESkillTree.Computation.Builders.Stats;
using PoESkillTree.Computation.Builders.Tests.Stats;
using PoESkillTree.Computation.Common;
using PoESkillTree.GameModel.Items;
using PoESkillTree.GameModel.Skills;

namespace PoESkillTree.Computation.Builders.Tests.Skills
{
    [TestFixture]
    public class SkillBuilderTest
    {
        [TestCase(42)]
        [TestCase(1)]
        public void SkillIdBuildsToCorrectValue(int expected)
        {
            var coreBuilder = CreateCoreBuilder("", expected);
            var sut = CreateSut(coreBuilder);

            var actual = sut.SkillId.Build().Calculate(null);

            Assert.AreEqual((NodeValue?) expected, actual);
        }

        [Test]
        public void SkillIdResolveBuildsToCorrectValue()
        {
            var expected = 42;
            var coreBuilder = CreateCoreBuilder("", expected);
            var unresolved = Mock.Of<ICoreBuilder<SkillDefinition>>(b => b.Resolve(null) == coreBuilder);
            var sut = CreateSut(unresolved);

            var actual = sut.SkillId.Resolve(null).Build().Calculate(null);

            Assert.AreEqual((NodeValue?) expected, actual);
        }

        [Test]
        public void InstancesBuildsToCorrectResults()
        {
            var coreBuilder = CreateCoreBuilder("skill");
            var sut = CreateSut(coreBuilder);

            var stat = sut.Instances.BuildToSingleStat();

            Assert.AreEqual("skill.Instances", stat.Identity);
        }

        [Test]
        public void CastBuildsToCorrectResult()
        {
            var coreBuilder = CreateCoreBuilder("skill");
            var sut = CreateSut(coreBuilder);

            var actual = sut.Cast.Build();

            Assert.AreEqual("skill.Cast", actual);
        }

        private static ICoreBuilder<SkillDefinition> CreateCoreBuilder(string id, int numericId = 0) =>
            CoreBuilder.Create(CreateSkill(id, numericId));

        private static SkillDefinition CreateSkill(string id, int numericId)
            => SkillDefinition.CreateActive(
                id, numericId, "", null,
                new ActiveSkillDefinition(id, 0, new string[0], new string[0], new Keyword[0], false,
                    null, new ItemClass[0]),
                new Dictionary<int, SkillLevelDefinition>());

        private static SkillBuilder CreateSut(ICoreBuilder<SkillDefinition> coreBuilder) =>
            new SkillBuilder(new StatFactory(), coreBuilder);
    }
}