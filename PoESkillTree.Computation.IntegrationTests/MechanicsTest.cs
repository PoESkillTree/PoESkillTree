using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using PoESkillTree.Common.Utils.Extensions;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Damage;
using PoESkillTree.Computation.Common.Builders.Effects;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.Computation.Console;
using PoESkillTree.Computation.Core;
using PoESkillTree.Computation.IntegrationTests.Core;
using PoESkillTree.Computation.Parsing;
using PoESkillTree.GameModel.Items;

namespace PoESkillTree.Computation.IntegrationTests
{
    [TestFixture]
    public class MechanicsTest
    {
        private static readonly double Accuracy = (-2 + 2 * 90) + 1000;
        private static readonly double ChanceToHit = Accuracy / (Accuracy + Math.Pow(1000, 0.8));

        private static IReadOnlyList<Modifier> _givenMods;
        private static IBuilderFactories _builderFactories;
        private static IMetaStatBuilders _metaStats;

        [OneTimeSetUp]
        public static void ClassInit()
        {
            var compRoot = new CompositionRoot();
            _builderFactories = compRoot.BuilderFactories;
            _metaStats = compRoot.MetaStats;
            var modSource = new ModifierSource.Global();
            _givenMods = GivenStatsParser.Parse(compRoot.Parser, compRoot.GivenStats)
                .Append(
                    new Modifier(
                        Build(_builderFactories.StatBuilders.Evasion.For(_builderFactories.EntityBuilders.Enemy)),
                        Form.BaseSet, new Constant(4000), modSource),
                    new Modifier(
                        Build(_builderFactories.DamageTypeBuilders.Physical.Damage.WithHits
                            .For(_builderFactories.EntityBuilders.Enemy)),
                        Form.BaseSet, new Constant(1000), modSource),
                    new Modifier(
                        Build(_builderFactories.StatBuilders.Pool.From(Pool.Life)
                            .For(_builderFactories.EntityBuilders.Enemy)),
                        Form.BaseSet, new Constant(200), modSource),
                    new Modifier(
                        Build(_builderFactories.DamageTypeBuilders.Physical.Damage.Taken
                            .For(_builderFactories.EntityBuilders.Enemy)),
                        Form.Increase, new Constant(20), modSource),
                    new Modifier(
                        Build(_builderFactories.DamageTypeBuilders.Physical.Resistance
                            .For(_builderFactories.EntityBuilders.Enemy)),
                        Form.BaseSet, new Constant(60), modSource),
                    new Modifier(
                        Build(_builderFactories.StatBuilders.Level),
                        Form.BaseSet, new Constant(90), modSource),
                    new Modifier(
                        Build(_builderFactories.DamageTypeBuilders.Physical.Damage.WithSkills),
                        Form.BaseSet, new Constant(5), modSource),
                    new Modifier(
                        Build(_builderFactories.StatBuilders.Accuracy),
                        Form.BaseAdd, new Constant(1000), modSource),
                    new Modifier(
                        Build(_metaStats.SkillHitDamageSource),
                        Form.BaseSet, new Constant((int) DamageSource.Attack), modSource),
                    new Modifier(
                        Build(_metaStats.SkillUsesHand(AttackDamageHand.MainHand)),
                        Form.BaseSet, new Constant(true), modSource),
                    new Modifier(
                        Build(_metaStats.SkillUsesHand(AttackDamageHand.OffHand)),
                        Form.BaseSet, new Constant(true), modSource),
                    new Modifier(
                        Build(_builderFactories.EquipmentBuilders.Equipment[ItemSlot.MainHand].ItemTags),
                        Form.TotalOverride, new Constant((int) Tags.Sword), modSource))
                .ToList();
        }

        [Test]
        public void SkillDpsWithHits()
        {
            var calculator = Calculator.CreateCalculator();
            var nodes = calculator.NodeRepository;

            calculator.NewBatchUpdate()
                .AddModifiers(_givenMods)
                .AddModifier(Build(_builderFactories.StatBuilders.CastRate.With(DamageSource.Attack)), Form.BaseSet, 2)
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
            Assert.AreEqual(ChanceToHit * 100, actual);
            actual = nodes
                .GetNode(BuildMainHandSkillSingle(_metaStats.AverageDamagePerHit))
                .Value.Single();
            var effectiveCritChance = 0.1 * ChanceToHit;
            var expectedAverageDamagePerHit = expectedDamageWithNonCrits * (1 - effectiveCritChance) +
                                              expectedDamageWithCrits * effectiveCritChance;
            Assert.AreEqual(expectedAverageDamagePerHit, actual);
            actual = nodes
                .GetNode(BuildMainHandSkillSingle(_metaStats.AverageDamage))
                .Value.Single();
            var expectedAverageDamage = expectedAverageDamagePerHit * ChanceToHit;
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

        [Test]
        public void BleedDps()
        {
            var calculator = Calculator.CreateCalculator();
            var nodes = calculator.NodeRepository;

            calculator.NewBatchUpdate()
                .AddModifiers(_givenMods)
                .AddModifier(Build(_builderFactories.StatBuilders.CastRate.With(DamageSource.Attack)), Form.BaseSet, 2)
                .AddModifier(Build(_builderFactories.ActionBuilders.CriticalStrike.Chance), Form.BaseSet, 10)
                .AddModifier(Build(_builderFactories.EffectBuilders.Ailment.Bleed.Chance), Form.BaseSet, 10)
                .AddModifier(Build(_builderFactories.EffectBuilders.Ailment.Bleed.CriticalStrikesAlwaysInflict),
                    Form.BaseSet, 1)
                .DoUpdate();

            var critChance = 0.1 * ChanceToHit;
            var ailmentChanceNonCrits = 0.1;
            var ailmentChanceCrits = 1;
            var baseDamage = 5 * 0.7;
            var nonCritDamage = baseDamage * (1 - 60 / 100d) * 1.2;
            var critDamage = nonCritDamage * 1.5;
            var expected = (nonCritDamage * (1 - critChance) * ailmentChanceNonCrits +
                            critDamage * critChance * ailmentChanceCrits) /
                           ((1 - critChance) * ailmentChanceNonCrits + critChance * ailmentChanceCrits);
            var actual = nodes
                .GetNode(Build(_metaStats.AilmentDps(Ailment.Bleed)).Single())
                .Value.Single();
            Assert.AreEqual(expected, actual, 1e-10);
        }

        [Test]
        public void ResistanceAgainstPhysicalHits()
        {
            var calculator = Calculator.CreateCalculator();
            var nodes = calculator.NodeRepository;

            calculator.NewBatchUpdate()
                .AddModifiers(_givenMods)
                .AddModifier(Build(_builderFactories.StatBuilders.Armour), Form.BaseAdd, 4000)
                .AddModifier(Build(_builderFactories.DamageTypeBuilders.Physical.Resistance), Form.BaseAdd, 50)
                .DoUpdate();

            var armour = 4000;
            var enemyDamage = 1000d;
            var expected = 50 + 100 * armour / (armour + 10 * enemyDamage);
            var actual = nodes
                .GetNode(Build(_metaStats.ResistanceAgainstHits(DamageType.Physical)).Single())
                .Value.Single();
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void StunChance()
        {
            var calculator = Calculator.CreateCalculator();
            var nodes = calculator.NodeRepository;

            calculator.NewBatchUpdate()
                .AddModifiers(_givenMods)
                .AddModifier(Build(_builderFactories.StatBuilders.Armour), Form.BaseAdd, 4000)
                .AddModifier(Build(_builderFactories.EffectBuilders.Stun.Threshold), Form.Increase, -100)
                .DoUpdate();

            var damage = 5 * 0.4 * 1.2 * ChanceToHit;
            var expected = 200 * damage / (200 * 0.125);
            var actual = nodes
                .GetNode(Build(_builderFactories.EffectBuilders.Stun.Chance.With(AttackDamageHand.MainHand)).Single())
                .Value.Single();
            Assert.AreEqual(expected, actual);
        }

        private static IStat BuildMainHandSkillSingle(IDamageRelatedStatBuilder builder)
            => Build(builder.WithSkills.With(AttackDamageHand.MainHand)).Single();

        private static IReadOnlyList<IStat> Build(IStatBuilder builder)
            => builder.Build(default).SelectMany(r => r.Stats).ToList();
    }
}