using System.Linq;
using EnumsNET;
using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Builders.Entities;
using PoESkillTree.Computation.Builders.Stats;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders.Damage;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.Computation.Common.Parsing;
using PoESkillTree.GameModel;
using PoESkillTree.GameModel.Skills;
using PoESkillTree.Utils.Extensions;

namespace PoESkillTree.Computation.Builders.Damage
{
    [TestFixture]
    public class DamageTypeBuilderTest
    {
        [TestCase(DamageType.Fire, Keyword.Fire)]
        [TestCase(DamageType.Physical, Keyword.Physical)]
        public void BuildKeywordReturnsCorrectResult(DamageType damageType, Keyword expected)
        {
            var sut = CreateSut(damageType);

            var actual = sut.Build(default);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void BuildRandomElementThrows()
        {
            var sut = CreateSut(DamageType.RandomElement);

            Assert.Throws<ParseException>(() => sut.Build(default));
        }

        [TestCase(DamageType.Chaos)]
        [TestCase(DamageType.Cold)]
        public void BuildDamageTypesReturnsCorrectResult(DamageType damageType)
        {
            var sut = CreateSut(damageType);

            var actual = sut.BuildDamageTypes(default);

            Assert.AreEqual(new[] { damageType }, actual);
        }

        [Test]
        public void AndBuildsToCorrectDamageTypes()
        {
            var expected = new[] { DamageType.Fire, DamageType.Chaos, DamageType.Cold };
            var other = Mock.Of<IDamageTypeBuilder>(b =>
                b.BuildDamageTypes(default) == new[] { DamageType.Chaos, DamageType.Fire, DamageType.Cold });
            var sut = CreateSut(expected.First());

            var actual = sut.And(other).BuildDamageTypes(default);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void InvertBuildsToCorrectDamageTypes()
        {
            var expected = Enums.GetValues<DamageType>().Except(DamageType.Fire, DamageType.RandomElement);
            var sut = CreateSut(DamageType.Fire);

            var actual = sut.Invert.BuildDamageTypes(default);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ExceptBuildsToCorrectDamageTypes()
        {
            var expected = Enums.GetValues<DamageType>()
                .Except(DamageType.Fire, DamageType.RandomElement, DamageType.Cold);
            var sut = CreateSut(DamageType.Fire);

            var actual = sut.Invert.Except(CreateSut(DamageType.Cold)).BuildDamageTypes(default);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void BuildKeywordThrowsIfNoDamageTypes()
        {
            var sut = CreateSut(DamageType.Fire).Except(CreateSut(DamageType.Fire));

            Assert.Throws<ParseException>(() => sut.Build(default));
        }

        [Test]
        public void BuildKeywordThrowsIfMultipleDamageTypes()
        {
            var sut = CreateSut(DamageType.Fire).And(CreateSut(DamageType.Cold));

            Assert.Throws<ParseException>(() => sut.Build(default));
        }

        [Test]
        public void ResolveResolvesAndParameter()
        {
            var expected = new[] { DamageType.Fire, DamageType.Chaos };
            var other = CreateSut(DamageType.Chaos);
            var unresolved = Mock.Of<IDamageTypeBuilder>(b => b.Resolve(null) == other);
            var sut = CreateSut(DamageType.Fire).And(unresolved);

            var resolvedSut = (IDamageTypeBuilder) sut.Resolve(null);
            var actual = resolvedSut.BuildDamageTypes(default);

            Assert.AreEqual(expected, actual);
        }

        [TestCase(DamageType.Chaos)]
        [TestCase(DamageType.Fire)]
        public void ResistanceBuildsToCorrectResults(DamageType damageType)
        {
            var sut = CreateSut(damageType);

            var stat = sut.Resistance.BuildToSingleStat();

            Assert.AreEqual($"{damageType}.Resistance", stat.Identity);
        }

        [Test]
        public void ResistanceResolveResolvesAndParameter()
        {
            var other = CreateSut(DamageType.Chaos);
            var unresolved = Mock.Of<IDamageTypeBuilder>(b => b.Resolve(null) == other);
            var sut = CreateSut(DamageType.Fire).And(unresolved);

            var resolved = sut.Resistance.Resolve(null);
            var (stats, _, _) = resolved.BuildToSingleResult();

            Assert.AreEqual("Fire.Resistance", stats[0].Identity);
            Assert.AreEqual("Chaos.Resistance", stats[1].Identity);
        }

        [Test]
        public void ResistanceForEntityBuildsToCorrectStat()
        {
            var sut = CreateSut(DamageType.Fire);

            var statBuilder = sut.Resistance.For(new EntityBuilder(Entity.Enemy));
            var stat = statBuilder.BuildToSingleStat();

            Assert.AreEqual(Entity.Enemy, stat.Entity);
        }

        [Test]
        public void ResistanceMaximumBuildsToCorrectStat()
        {
            var sut = CreateSut(DamageType.Fire);

            var statBuilder = sut.Resistance.Maximum;
            var stat = statBuilder.BuildToSingleStat();

            Assert.AreEqual("Fire.Resistance.Maximum", stat.Identity);
        }

        [TestCase(42)]
        [TestCase(1)]
        public void ResistanceValueIsCalculatedCorrectly(int statValue)
        {
            var stat = new Stat("Fire.Resistance");
            var context = Mock.Of<IValueCalculationContext>(c =>
                c.GetValue(stat, NodeType.Total, PathDefinition.MainPath) == new NodeValue(statValue));
            var sut = CreateSut(DamageType.Fire);

            var valueBuilder = sut.Resistance.Value;
            var actual = valueBuilder.Build().Calculate(context);

            Assert.AreEqual(new NodeValue(statValue), actual);
        }

        [Test]
        public void ResistanceValueBuildThrowsIfMultipleDamageTypes()
        {
            var sut = CreateSut(DamageType.Fire).And(CreateSut(DamageType.Chaos));

            var valueBuilder = sut.Resistance.Value;

            Assert.Throws<ParseException>(() => valueBuilder.Build());
        }

        [Test]
        public void IgnoreResistanceBuildsToCorrectStat()
        {
            var sut = CreateSut(DamageType.Fire);

            var stats = sut.IgnoreResistance.Build(default).SelectMany(r => r.Stats).ToList();

            Assert.That(stats, Has.Exactly(8).Items); // Restricted to hits
            Assert.AreEqual("Fire.IgnoreResistanceWithCrits.Attack.MainHand.Skill", stats[0].Identity);
            Assert.AreEqual("Fire.IgnoreResistanceWithNonCrits.Attack.MainHand.Skill", stats[1].Identity);
        }

        [Test]
        public void PenetrationBuildsToCorrectStats()
        {
            var sut = CreateSut(DamageType.Fire);

            var stats = sut.Penetration.Build(default).SelectMany(r => r.Stats).ToList();

            Assert.That(stats, Has.Exactly(8).Items); // Restricted to hits
            Assert.AreEqual("Fire.PenetrationWithCrits.Attack.MainHand.Skill", stats[0].Identity);
            Assert.AreEqual("Fire.PenetrationWithNonCrits.Attack.MainHand.Skill", stats[1].Identity);
        }

        [Test]
        public void DamageBuildsToCorrectStats()
        {
            var sut = CreateSut(DamageType.Fire);

            var results = sut.Damage.Build(default).ToList();

            var (stats, _, _) = results[0];
            Assert.AreEqual("Fire.Damage.Attack.MainHand.Skill", stats[0].Identity);
        }

        [TestCase(Pool.Life, Pool.Mana)]
        [TestCase(Pool.EnergyShield, Pool.Life)]
        public void DamageTakenFromBuildsToCorrectStat(Pool source, Pool target)
        {
            var sourcePool = Mock.Of<IPoolStatBuilder>(b => b.BuildPool(default) == source);
            var targetPool = Mock.Of<IPoolStatBuilder>(b => b.BuildPool(default) == target);
            var sut = CreateSut(DamageType.Fire);

            var taken = sut.DamageTakenFrom(sourcePool).Before(targetPool);
            var stat = taken.BuildToSingleStat();

            Assert.AreEqual($"Fire.Damage.TakenFrom({source}).Before({target})", stat.Identity);
        }

        [TestCase(Pool.EnergyShield, Pool.Mana)]
        public void DamageTakenFromIsResolvable(Pool source, Pool target)
        {
            var sourcePool = Mock.Of<IPoolStatBuilder>(b => b.BuildPool(default) == source);
            var unresolvedSource = Mock.Of<IPoolStatBuilder>(b => b.Resolve(null) == sourcePool);
            var targetPool = Mock.Of<IPoolStatBuilder>(b => b.BuildPool(default) == target);
            var unresolvedTarget = Mock.Of<IPoolStatBuilder>(b => b.Resolve(null) == targetPool);
            var sut = CreateSut(DamageType.Fire);

            var taken = sut.DamageTakenFrom(unresolvedSource).Before(unresolvedTarget);
            var stat = taken.Resolve(null).BuildToSingleStat();

            Assert.AreEqual($"Fire.Damage.TakenFrom({source}).Before({target})", stat.Identity);
        }

        private static DamageTypeBuilder CreateSut(DamageType damageType) =>
            new DamageTypeBuilder(new StatFactory(), damageType);
    }
}