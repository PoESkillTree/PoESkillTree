using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MoreLinq;
using NUnit.Framework;
using PoESkillTree.Computation.Console;
using PoESkillTree.Computation.Console.Builders;
using PoESkillTree.Computation.Parsing;
using PoESkillTree.Computation.Parsing.Builders.Damage;

namespace PoESkillTree.Computation.IntegrationTests
{
    [TestFixture]
    public class ParsingTest
    {
        private static IParser _parser;

        [OneTimeSetUp]
        public static void ClassInit()
        {
            _parser = Program.CreateParser();
        }

        [Test, TestCaseSource(nameof(ReadParsableStatLines))]
        public void Parses(string statLine)
        {
            var (success, remaining, result) = _parser.Parse(statLine);

            Assert.IsTrue(success, $"{remaining}\nResult:\n  {string.Join("\n  ", result)}");
            CollectionAssert.IsEmpty(remaining);
            foreach (var modifier in result)
            {
                Assert.NotNull(modifier);
                Assert.NotNull(modifier.Stat);
                Assert.NotNull(modifier.Form);
                Assert.NotNull(modifier.Value);
                Assert.NotNull(modifier.Condition);
                var s = modifier.ToString();
                // Assert it has no unresolved references or values
                StringAssert.DoesNotContain("References", s);
                StringAssert.DoesNotContain("Values", s);
            }
        }

        [Test, TestCaseSource(nameof(ReadUnparsableStatLines))]
        public void DoesNotParse(string statLine)
        {
            var (success, remaining, result) = _parser.Parse(statLine);

            Assert.IsFalse(success, $"{remaining}\nResult:\n  {string.Join("\n  ", result)}");
            foreach (var modifier in result)
            {
                var s = modifier?.ToString();
                StringAssert.DoesNotContain("References", s);
                StringAssert.DoesNotContain("Values", s);
            }
        }

        private static string[] ReadParsableStatLines()
        {
            var unparsable = ReadUnparsableStatLines().ToHashSet();

            return ReadStatLines("AllSkillTreeStatLines")
                .Concat(ReadStatLines("ParsableStatLines"))
                .Where(s => !unparsable.Contains(s))
                .ToArray();
        }

        private static string[] ReadUnparsableStatLines()
        {
            return ReadStatLines("UnparsableStatLines")
                .Concat(ReadStatLines("NotYetParsableStatLines"))
                .ToArray();
        }

        private static IEnumerable<string> ReadStatLines(string fileName)
        {
            return File.ReadAllLines(TestContext.CurrentContext.TestDirectory + $"/Data/{fileName}.txt")
                .Where(s => !s.StartsWith("//", StringComparison.Ordinal))
                .Distinct()
                .Select(s => s.ToLowerInvariant());
        }

        [Test, TestCaseSource(nameof(ParsingReturnsCorrectModifiers_TestCases))]
        public IEnumerable<Modifier> ParsingReturnsCorrectModifiers(string statLine)
        {
            var (_, _, result) = _parser.Parse(statLine);
            return result;
        }

        private static IEnumerable<TestCaseData> ParsingReturnsCorrectModifiers_TestCases()
        {
            var f = new BuilderFactories();

            yield return new TestCaseData("+10 to Dexterity").Returns(new[]
            {
                new Modifier(
                    f.StatBuilders.Attribute.Dexterity,
                    f.FormBuilders.BaseAdd,
                    f.ValueBuilders.Create(10),
                    f.ConditionBuilders.True)
            });

            yield return new TestCaseData("Gain 30 Mana per Grand Spectrum").Returns(new[]
            {
                new Modifier(
                    f.StatBuilders.GrandSpectrumJewelsSocketed,
                    f.FormBuilders.BaseAdd,
                    f.ValueBuilders.Create(1),
                    f.ConditionBuilders.True),
                new Modifier(
                    f.StatBuilders.Pool.Mana,
                    f.FormBuilders.BaseAdd,
                    f.ValueBuilders.Create(30).Multiply(f.StatBuilders.GrandSpectrumJewelsSocketed.Value),
                    f.ConditionBuilders.True),
            });

            yield return new TestCaseData(
                    "With 5 Corrupted Items Equipped: 50% of Chaos Damage does not bypass Energy Shield, and 50% of Physical Damage bypasses Energy Shield")
                .Returns(new[]
                {
                    new Modifier(
                        f.DamageTypeBuilders.Chaos.Damage.TakenFrom(f.StatBuilders.Pool.EnergyShield)
                            .Before(f.StatBuilders.Pool.Life),
                        f.FormBuilders.BaseAdd,
                        f.ValueBuilders.Create(50),
                        f.EquipmentBuilders.Equipment.Count(e => e.IsCorrupted) >= 5),
                    new Modifier(
                        f.DamageTypeBuilders.Physical.Damage.TakenFrom(f.StatBuilders.Pool.EnergyShield)
                            .Before(f.StatBuilders.Pool.Life),
                        f.FormBuilders.BaseSubtract,
                        f.ValueBuilders.Create(50),
                        f.EquipmentBuilders.Equipment.Count(e => e.IsCorrupted) >= 5)
                });

            yield return new TestCaseData(
                    "Life Leeched per Second is doubled.\nMaximum Life Leech Rate is doubled.\nLife Regeneration has no effect.")
                .Returns(new[]
                {
                    new Modifier(
                        f.StatBuilders.Pool.Life.Leech.Rate,
                        f.FormBuilders.PercentMore,
                        f.ValueBuilders.Create(100),
                        f.ConditionBuilders.True),
                    new Modifier(
                        f.StatBuilders.Pool.Life.Leech.RateLimit,
                        f.FormBuilders.PercentMore,
                        f.ValueBuilders.Create(100),
                        f.ConditionBuilders.True),
                    new Modifier(
                        f.StatBuilders.Pool.Life.Regen,
                        f.FormBuilders.BaseOverride,
                        f.ValueBuilders.Create(0),
                        f.ConditionBuilders.True)
                });

            Modifier ParagonOfCalamityFor(IDamageTypeBuilder damageType) =>
                new Modifier(
                    damageType.Damage.Taken,
                    f.FormBuilders.PercentReduce,
                    f.ValueBuilders.Create(8),
                    f.ActionBuilders.Hit.With(damageType).Taken.Recently);

            yield return new TestCaseData(
                    "For each Element you've been hit by Damage of Recently, 8% reduced Damage taken of that Element")
                .Returns(new[]
                {
                    ParagonOfCalamityFor(f.DamageTypeBuilders.Fire),
                    ParagonOfCalamityFor(f.DamageTypeBuilders.Lightning),
                    ParagonOfCalamityFor(f.DamageTypeBuilders.Cold),
                });

            yield return new TestCaseData(
                    "Auras you Cast grant 3% increased Attack and Cast Speed to you and Allies")
                .Returns(new[]
                {
                    new Modifier(
                        f.SkillBuilders.Skills.Speed.AddTo(f.SkillBuilders.Skills[f.KeywordBuilders.Aura]),
                        f.FormBuilders.PercentIncrease,
                        f.ValueBuilders.Create(3),
                        f.ConditionBuilders.True)
                });
        }
    }
}