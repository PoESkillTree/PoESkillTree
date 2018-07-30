using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using PoESkillTree.Common.Utils.Extensions;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Damage;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.Computation.Console;
using PoESkillTree.Computation.Core;
using PoESkillTree.Computation.IntegrationTests.Core;
using PoESkillTree.Computation.Parsing;

namespace PoESkillTree.Computation.IntegrationTests
{
    [TestFixture]
    public class MechanicsTest
    {
        private static IReadOnlyList<Modifier> _givenMods;
        private static IBuilderFactories _builderFactories;
        private static IMetaStatBuilders _metaStats;

        [OneTimeSetUp]
        public static void ClassInit()
        {
            var compRoot = new CompositionRoot();
            _builderFactories = compRoot.BuilderFactories;
            _metaStats = compRoot.MetaStats;
            _givenMods = GivenStatsParser.Parse(compRoot.Parser, compRoot.GivenStats)
                .Append(
                    new Modifier(
                        Build(_builderFactories.StatBuilders.Evasion.For(_builderFactories.EntityBuilders.Enemy)),
                        Form.BaseSet, new Constant(4000), new ModifierSource.Global()),
                    new Modifier(
                        Build(_builderFactories.DamageTypeBuilders.Physical.Damage.Taken.For(_builderFactories
                            .EntityBuilders.Enemy)),
                        Form.Increase, new Constant(20), new ModifierSource.Global()),
                    new Modifier(
                        Build(_builderFactories.DamageTypeBuilders.Physical.Resistance.For(_builderFactories
                            .EntityBuilders.Enemy)),
                        Form.BaseSet, new Constant(60), new ModifierSource.Global()),
                    new Modifier(
                        Build(_builderFactories.StatBuilders.Level),
                        Form.BaseSet, new Constant(90), new ModifierSource.Global()),
                    new Modifier(
                        Build(_builderFactories.DamageTypeBuilders.Physical.Damage),
                        Form.BaseSet, new Constant(5), new ModifierSource.Global()))
                .ToList();
        }

        [Test]
        public void SkillDpsWithHits()
        {
            var calculator = Calculator.CreateCalculator();
            var nodes = calculator.NodeRepository;

            calculator.NewBatchUpdate()
                .AddModifiers(_givenMods)
                .AddModifier(Build(_metaStats.SkillHitDamageSource), Form.BaseSet, (int) DamageSource.Attack)
                .AddModifier(Build(_builderFactories.StatBuilders.CastRate.With(DamageSource.Attack)), Form.BaseSet, 2)
                .AddModifier(Build(_builderFactories.StatBuilders.Accuracy), Form.BaseAdd, 1000)
                .AddModifier(Build(_builderFactories.ActionBuilders.CriticalStrike.Chance), Form.BaseSet, 10)
                .AddModifier(Build(_builderFactories.DamageTypeBuilders.Physical.Penetration), Form.BaseAdd, 10)
                .DoUpdate();

            var actual = nodes
                .GetNode(BuildMainHandSkillSingle(_metaStats.EnemyResistanceAgainstNonCrits(DamageType.Physical)))
                .Value.Single();
            var expectedEnemyResistance = 60 - 10;
            Assert.AreEqual(expectedEnemyResistance, actual);
            actual = nodes
                .GetNode(BuildMainHandSkillSingle(_metaStats.EnemyResistanceAgainstCrits(DamageType.Physical)))
                .Value.Single();
            Assert.AreEqual(expectedEnemyResistance, actual);
            actual = nodes
                .GetNode(
                    BuildMainHandSkillSingle(_metaStats.EffectiveDamageMultiplierWithNonCrits(DamageType.Physical)))
                .Value.Single();
            var expectedEffectiveDamageMultiplierWithNonCrits = (1 - expectedEnemyResistance / 100d) * 1.2;
            Assert.AreEqual(expectedEffectiveDamageMultiplierWithNonCrits, actual);
            actual = nodes
                .GetNode(BuildMainHandSkillSingle(_metaStats.EffectiveDamageMultiplierWithCrits(DamageType.Physical)))
                .Value.Single();
            var expectedEffectiveDamageMultiplierWithCrits = (1 - expectedEnemyResistance / 100d) * 1.2 * 1.5;
            Assert.AreEqual(expectedEffectiveDamageMultiplierWithCrits, actual);
            actual = nodes
                .GetNode(BuildMainHandSkillSingle(_metaStats.DamageWithNonCrits(DamageType.Physical)))
                .Value.Single();
            var expectedDamageWithNonCrits = 5 * expectedEffectiveDamageMultiplierWithNonCrits;
            Assert.AreEqual(expectedDamageWithNonCrits, actual);
            actual = nodes
                .GetNode(BuildMainHandSkillSingle(_metaStats.DamageWithCrits(DamageType.Physical)))
                .Value.Single();
            var expectedDamageWithCrits = 5 * expectedEffectiveDamageMultiplierWithCrits;
            Assert.AreEqual(expectedDamageWithCrits, actual);
            actual = nodes
                .GetNode(BuildMainHandSkillSingle(_builderFactories.StatBuilders.ChanceToHit))
                .Value.Single();
            var expectedAccuracy = (-2 + 2 * 90) + 1000;
            var expectedChanceToHit = 100 * expectedAccuracy / (expectedAccuracy + Math.Pow(1000, 0.8));
            Assert.AreEqual(expectedChanceToHit, actual);
            actual = nodes
                .GetNode(BuildMainHandSkillSingle(_metaStats.AverageDamagePerHit))
                .Value.Single();
            var effectiveCritChance = 0.1 * expectedChanceToHit / 100;
            var expectedAverageDamagePerHit = expectedDamageWithNonCrits * (1 - effectiveCritChance) +
                                              expectedDamageWithCrits * effectiveCritChance;
            Assert.AreEqual(expectedAverageDamagePerHit, actual);
            actual = nodes
                .GetNode(BuildMainHandSkillSingle(_metaStats.AverageDamage))
                .Value.Single();
            var expectedAverageDamage = expectedAverageDamagePerHit * expectedChanceToHit / 100;
            Assert.AreEqual(expectedAverageDamage, actual);
            actual = nodes
                .GetNode(Build(_metaStats.SkillDpsWithHits).Single())
                .Value.Single();
            var expectedSkillDpsWithHits = expectedAverageDamage * 2;
            Assert.AreEqual(expectedSkillDpsWithHits, actual);
        }

        [Test]
        public void SkillDpsWithDoTs()
        {
            var calculator = Calculator.CreateCalculator();
            var nodes = calculator.NodeRepository;

            calculator.NewBatchUpdate()
                .AddModifiers(_givenMods)
                .DoUpdate();

            var actual = nodes
                .GetNode(Build(_metaStats.EffectiveDamageMultiplierWithNonCrits(DamageType.Physical)
                    .WithSkills(DamageSource.OverTime)).Single())
                .Value.Single();
            var expectedEffectiveDamageMultiplierWithNonCrits = (1 - 60 / 100d) * 1.2;
            Assert.AreEqual(expectedEffectiveDamageMultiplierWithNonCrits, actual);
            actual = nodes
                .GetNode(Build(_metaStats.DamageWithNonCrits(DamageType.Physical)
                    .WithSkills(DamageSource.OverTime)).Single())
                .Value.Single();
            var expectedDamageWithNonCrits = 5 * expectedEffectiveDamageMultiplierWithNonCrits;
            Assert.AreEqual(expectedDamageWithNonCrits, actual);
            actual = nodes
                .GetNode(Build(_metaStats.SkillDpsWithDoTs).Single())
                .Value.Single();
            var expectedSkillDpsWithDoTs = expectedDamageWithNonCrits;
            Assert.AreEqual(expectedSkillDpsWithDoTs, actual);
        }

        private static IStat BuildMainHandSkillSingle(IDamageRelatedStatBuilder builder)
            => Build(builder.WithSkills.With(AttackDamageHand.MainHand)).Single();

        private static IReadOnlyList<IStat> Build(IStatBuilder builder)
            => builder.Build(default).SelectMany(r => r.Stats).ToList();
    }
}