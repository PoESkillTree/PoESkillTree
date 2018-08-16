using System.Collections.Generic;
using System.Linq;
using EnumsNET;
using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Builders.Buffs;
using PoESkillTree.Computation.Builders.Entities;
using PoESkillTree.Computation.Builders.Stats;
using PoESkillTree.Computation.Builders.Tests.Stats;
using PoESkillTree.Computation.Builders.Values;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders.Entities;
using PoESkillTree.Computation.Common.Builders.Skills;

namespace PoESkillTree.Computation.Builders.Tests.Buffs
{
    [TestFixture]
    public class BuffBuildersTest
    {
        [TestCase(true)]
        [TestCase(false)]
        public void TemporaryBuildsToCorrectResult(bool expectedCondition)
        {
            var expectedValue = expectedCondition ? (NodeValue?) 3 : null;
            var gainedStatBuilder = StatBuilderUtils.FromIdentity(StatFactory, "s", null);
            var modifierSource = new ModifierSource.Local.Skill("skill node");
            var conditionStat = new Stat($"Is {modifierSource} active?");
            var buffEffectStat = new Stat($"Buff.EffectOn({default(Entity)})");
            var context = Mock.Of<IValueCalculationContext>(c =>
                c.GetValue(conditionStat, NodeType.Total, PathDefinition.MainPath) == (NodeValue?) expectedCondition &&
                c.GetValue(buffEffectStat, NodeType.Total, PathDefinition.MainPath) == new NodeValue(1.5));
            var sut = CreateSut();

            var (stats, _, valueConverter) = sut.Temporary(gainedStatBuilder)
                .BuildToSingleResult(modifierSource);
            var actualValue = valueConverter(new ValueBuilderImpl(2)).Build().Calculate(context);

            Assert.That(stats, Has.One.Items);
            Assert.AreEqual("s", stats[0].Identity);
            Assert.AreEqual(expectedValue, actualValue);
        }

        [TestCase(BuffRotationStage.Stage0)]
        [TestCase(BuffRotationStage.Stage1)]
        public void TemporaryTBuildsToCorrectResult(BuffRotationStage activeStage)
        {
            var temporaryStage = BuffRotationStage.Stage1;
            var expectedCondition = temporaryStage == activeStage;
            var modifierSource = new ModifierSource.Local.Skill("skill node");
            var stageStat = new Stat($"Current {modifierSource} stage");
            var context = Mock.Of<IValueCalculationContext>(c =>
                c.GetValue(stageStat, NodeType.Total, PathDefinition.MainPath) == new NodeValue((int) activeStage));
            var sut = CreateSut();

            var (stats, _, valueConverter) = sut.Temporary(sut.Fortify, temporaryStage)
                .BuildToSingleResult(modifierSource);
            var value = valueConverter(new ValueBuilderImpl(1)).Build();
            var actualCondition = value.Calculate(context).IsTrue();

            Assert.That(stats, Has.Exactly(3).Items);
            Assert.AreEqual("Fortify.Active", stats[0].Identity);
            Assert.AreEqual(expectedCondition, actualCondition);
        }

        [Test]
        public void BuffBuildsToCorrectResult()
        {
            var gainedStatBuilder = StatBuilderUtils.FromIdentity(StatFactory, "s", null);
            var entityBuilders = new IEntityBuilder[]
                { new ModifierSourceEntityBuilder(), new EntityBuilder(Entity.Enemy), };
            var buffEffectStats = new[]
                { new Stat($"Buff.EffectOn({default(Entity)})"), new Stat($"Buff.EffectOn({Entity.Enemy})") };
            var context = Mock.Of<IValueCalculationContext>(c =>
                c.GetValue(buffEffectStats[0], NodeType.Total, PathDefinition.MainPath) == new NodeValue(2) &&
                c.GetValue(buffEffectStats[1], NodeType.Total, PathDefinition.MainPath) == new NodeValue(3));
            var sut = CreateSut();

            var results = sut.Buff(gainedStatBuilder, entityBuilders).Build(default).ToList();
            Assert.That(results, Has.Exactly(2).Items);
            var (defaultEntityStats, _, defaultEnittyValueConverter) = results[0];
            var defaultEntityValue = defaultEnittyValueConverter(new ValueBuilderImpl(2)).Build().Calculate(context);
            var (enemyStats, _, enemyValueConverter) = results[1];
            var enemyValue = enemyValueConverter(new ValueBuilderImpl(2)).Build().Calculate(context);

            Assert.That(defaultEntityStats, Has.One.Items);
            Assert.AreEqual("s", defaultEntityStats[0].Identity);
            Assert.AreEqual(default(Entity), defaultEntityStats[0].Entity);
            Assert.AreEqual((NodeValue?) 4, defaultEntityValue);
            Assert.AreEqual(Entity.Enemy, enemyStats[0].Entity);
            Assert.AreEqual((NodeValue?) 6, enemyValue);
        }

        [Test]
        public void AuraBuildsToCorrectResults()
        {
            var gainedStatBuilder = StatBuilderUtils.FromIdentity(StatFactory, "s", null);
            var auraEffectStat = new Stat($"Aura.EffectOn({Entity.Minion})", Entity.Enemy);
            var context = Mock.Of<IValueCalculationContext>(c =>
                c.GetValue(auraEffectStat, NodeType.Total, PathDefinition.MainPath) == new NodeValue(1.5));
            var sut = CreateSut();

            var auraStatBuilder = sut.Aura(gainedStatBuilder, new EntityBuilder(Entity.Minion));
            var (stats, _, valueConverter) = auraStatBuilder.BuildToSingleResult(entity: Entity.Enemy);
            var value = valueConverter(new ValueBuilderImpl(2)).Build();
            var actualValue = value.Calculate(context);

            Assert.That(stats, Has.Exactly(1).Items);
            Assert.AreEqual("s", stats[0].Identity);
            Assert.AreEqual(Entity.Minion, stats[0].Entity);
            Assert.AreEqual((NodeValue?) 3, actualValue);
        }

        [Test]
        public void BuffsWithoutParametersCountBuildsToCorrectValue()
        {
            // Buff properties + conflux + dummy buff and aura + passed buff skills
            var expected = 11 + 4 + 2 + 3;
            // For every source entity
            expected *= Enums.GetMemberCount<Entity>();
            var context = Mock.Of<IValueCalculationContext>(c =>
                c.GetValue(It.IsAny<IStat>(), NodeType.Total, PathDefinition.MainPath) == (NodeValue?) true);
            var sut = CreateSut();

            var actual = sut.Buffs().Count().Build().Calculate(context);

            Assert.AreEqual((NodeValue?) expected, actual);
        }

        [Test]
        public void BuffsWithParamtersCountBuildsToCorrectValue()
        {
            // Buff properties + conflux + dummy buff and aura + passed buff skills
            var expected = 11 + 4 + 2 + 3;
            var source = new ModifierSourceEntityBuilder();
            var target = new ModifierSourceEntityBuilder();
            var context = Mock.Of<IValueCalculationContext>(c =>
                c.GetValue(It.IsAny<IStat>(), NodeType.Total, PathDefinition.MainPath) == (NodeValue?) true);
            var sut = CreateSut();

            var actual = sut.Buffs(source, target).Count().Build().Calculate(context);

            Assert.AreEqual((NodeValue?) expected, actual);
        }

        private static BuffBuilders CreateSut()
        {
            var buffSkills = new (string identifier, IReadOnlyList<Keyword> keywords)[]
            {
                ("wrath", new[] { Keyword.Aura, Keyword.AreaOfEffect, Keyword.Lightning }),
                ("herald", new[] { Keyword.Spell }),
                ("frostbite", new[] { Keyword.Curse })
            }.Select(t => new SkillDefinition(t.identifier, 0, t.keywords, true));
            return new BuffBuilders(StatFactory, buffSkills);
        }

        private static readonly IStatFactory StatFactory = new StatFactory();

        public enum BuffRotationStage
        {
            Stage0,
            Stage1
        }
    }
}