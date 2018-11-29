using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MoreLinq;
using NUnit.Framework;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Common.Builders;
using PoESkillTree.Computation.Common.Builders.Conditions;
using PoESkillTree.Computation.Common.Builders.Damage;
using PoESkillTree.Computation.Common.Builders.Forms;
using PoESkillTree.Computation.Common.Builders.Stats;
using PoESkillTree.Computation.Common.Builders.Values;
using PoESkillTree.Computation.Console;
using PoESkillTree.Computation.Data.GivenStats;
using PoESkillTree.Computation.Parsing;
using PoESkillTree.Utils.Extensions;
using static PoESkillTree.Computation.IntegrationTests.ParsingTestUtils;

namespace PoESkillTree.Computation.IntegrationTests
{
    [TestFixture]
    public class ParsingTest : CompositionRootTestBase
    {
        private ICoreParser _parser;
        private IBuilderFactories _f;

        [SetUp]
        public async Task SetUpAsync()
        {
            _parser = await CompositionRoot.CoreParser.ConfigureAwait(false);
            _f = await CompositionRoot.BuilderFactories.ConfigureAwait(false);
        }

        [Test, TestCaseSource(nameof(ReadParsableStatLines))]
        public void Parses(string statLine)
        {
            var actual = _parser.Parse(statLine);

            AssertIsParsedSuccessfully(actual);
        }

        [Test, TestCaseSource(nameof(ReadUnparsableStatLines))]
        public void DoesNotParse(string statLine)
        {
            var actual = _parser.Parse(statLine);

            AssertIsParsedUnsuccessfully(actual);
        }

        private static IEnumerable<string> ReadParsableStatLines()
        {
            var unparsable = ReadUnparsableStatLines().Select(s => s.ToLowerInvariant()).ToHashSet();

            var unparsedGivenStats = new GivenStatsCollection(null, null, null, null).SelectMany(s => s.GivenStatLines);
            return ReadDataLines("SkillTreeStatLines")
                .Concat(ReadDataLines("ParsableStatLines"))
                .Concat(unparsedGivenStats)
                .Where(s => !unparsable.Contains(s.ToLowerInvariant()));
        }

        private static IEnumerable<string> ReadUnparsableStatLines()
            => ReadDataLines("UnparsableStatLines").Concat(ReadDataLines("NotYetParsableStatLines"));

        [Test]
        public void Dexterity()
        {
            var expected = CreateModifier(
                _f.StatBuilders.Attribute.Dexterity,
                _f.FormBuilders.BaseAdd,
                _f.ValueBuilders.Create(10));
            var actual = _parser.Parse("+10 to Dexterity").Modifiers;

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ManaPerGrandSpectrum()
        {
            var expected = new[]
            {
                CreateModifier(
                    _f.StatBuilders.GrandSpectrumJewelsSocketed,
                    _f.FormBuilders.BaseAdd,
                    _f.ValueBuilders.Create(1)),
                CreateModifier(
                    _f.StatBuilders.Pool.From(Pool.Mana),
                    _f.FormBuilders.BaseAdd,
                    _f.ValueBuilders.Create(30).Multiply(_f.StatBuilders.GrandSpectrumJewelsSocketed.Value)),
            }.Flatten();
            var actual = _parser.Parse("Gain 30 Mana per Grand Spectrum").Modifiers;

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void CorruptedEnergy()
        {
            var expected = new[]
            {
                CreateModifier(
                    _f.DamageTypeBuilders.Chaos.DamageTakenFrom(_f.StatBuilders.Pool.From(Pool.EnergyShield))
                        .Before(_f.StatBuilders.Pool.From(Pool.Life)),
                    _f.FormBuilders.BaseAdd,
                    _f.ValueBuilders.Create(50),
                    _f.EquipmentBuilders.Equipment.Count(e => e.IsCorrupted) >= 5),
                CreateModifier(
                    _f.DamageTypeBuilders.Physical.DamageTakenFrom(_f.StatBuilders.Pool.From(Pool.EnergyShield))
                        .Before(_f.StatBuilders.Pool.From(Pool.Life)),
                    _f.FormBuilders.BaseSubtract,
                    _f.ValueBuilders.Create(50),
                    _f.EquipmentBuilders.Equipment.Count(e => e.IsCorrupted) >= 5)
            }.Flatten();
            var actual = _parser.Parse(
                    "With 5 Corrupted Items Equipped: 50% of Chaos Damage does not bypass Energy Shield, and 50% of Physical Damage bypasses Energy Shield")
                .Modifiers;

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void VaalPact()
        {
            var life = _f.StatBuilders.Pool.From(Pool.Life);
            var expected = new[]
            {
                CreateModifier(
                    life.Leech.Rate,
                    _f.FormBuilders.PercentMore,
                    _f.ValueBuilders.Create(100)),
                CreateModifier(
                    life.Leech.RateLimit,
                    _f.FormBuilders.PercentMore,
                    _f.ValueBuilders.Create(100)),
                CreateModifier(
                    life.Regen,
                    _f.FormBuilders.PercentLess,
                    _f.ValueBuilders.Create(100))
            }.Flatten();
            var actual = _parser.Parse(
                    "Life Leeched per Second is doubled.\nMaximum Life Leech Rate is doubled.\nLife Regeneration has no effect.")
                .Modifiers;

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ParagonOfCalamity()
        {
            var expected = new[]
            {
                ParagonOfCalamityFor(_f.DamageTypeBuilders.Fire),
                ParagonOfCalamityFor(_f.DamageTypeBuilders.Lightning),
                ParagonOfCalamityFor(_f.DamageTypeBuilders.Cold),
            }.Flatten();
            var actual = _parser.Parse(
                    "For each Element you've been hit by Damage of Recently, 8% reduced Damage taken of that Element")
                .Modifiers;

            Assert.AreEqual(expected, actual);

            IEnumerable<Modifier> ParagonOfCalamityFor(IDamageTypeBuilder damageType) =>
                CreateModifier(
                    damageType.Damage.Taken,
                    _f.FormBuilders.PercentReduce,
                    _f.ValueBuilders.Create(8),
                    _f.ActionBuilders.HitWith(damageType).By(_f.EntityBuilders.Enemy).Recently);
        }

        [Test]
        public void AurasGrantCastRate()
        {
            var expected = new[]
            {
                CreateModifier(
                    _f.BuffBuilders.Buffs(_f.EntityBuilders.Self, _f.EntityBuilders.Self, _f.EntityBuilders.Ally)
                        .With(_f.KeywordBuilders.Aura).Without(_f.KeywordBuilders.Curse)
                        .AddStat(_f.StatBuilders.CastRate),
                    _f.FormBuilders.PercentIncrease,
                    _f.ValueBuilders.Create(3))
            }.Flatten();
            var actual = _parser.Parse(
                    "Auras from your Skills grant 3% increased Attack and Cast Speed to you and Allies")
                .Modifiers;

            Assert.AreEqual(expected, actual);
        }

        private static IEnumerable<Modifier> CreateModifier(
            IStatBuilder statBuilder, IFormBuilder formBuilder, IValueBuilder valueBuilder)
        {
            var statBuilderResults = statBuilder.Build(default(BuildParameters).With(new ModifierSource.Global()));
            var (form, formValueConverter) = formBuilder.Build();
            foreach (var (stats, source, statValueConverter) in statBuilderResults)
            {
                var value = formValueConverter(statValueConverter(valueBuilder)).Build(default);
                yield return new Modifier(stats, form, value, source);
            }
        }

        private static IEnumerable<Modifier> CreateModifier(
            IStatBuilder statBuilder, IFormBuilder formBuilder, IValueBuilder valueBuilder,
            IConditionBuilder conditionBuilder)
        {
            return CreateModifier(statBuilder.WithCondition(conditionBuilder), formBuilder, valueBuilder);
        }
    }
}