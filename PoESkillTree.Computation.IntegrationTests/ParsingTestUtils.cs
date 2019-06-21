using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Parsing;
using PoESkillTree.GameModel;
using PoESkillTree.GameModel.StatTranslation;
using static MoreLinq.Extensions.EquiZipExtension;

namespace PoESkillTree.Computation.IntegrationTests
{
    public static class ParsingTestUtils
    {
        public static void AssertIsParsedSuccessfully(
            ParseResult parseResult, IEnumerable<string> ignoredStatLines = null)
        {
            var (failedLines, remaining, result) = parseResult;

            var failedLinesWithRemaining = failedLines.EquiZip(remaining, (line, remain) => (line, remain));
            if (ignoredStatLines != null)
            {
                failedLinesWithRemaining = failedLinesWithRemaining
                    .Where(t => !ignoredStatLines.Contains(CanonicalizeFailedStatLine(t.line)));
            }
            Assert.IsEmpty(failedLinesWithRemaining);

            foreach (var modifier in result)
            {
                Assert.NotNull(modifier);
                Assert.NotNull(modifier.Stats);
                CollectionAssert.IsNotEmpty(modifier.Stats);
                Assert.NotNull(modifier.Form);
                Assert.NotNull(modifier.Value);
                Assert.NotNull(modifier.Source);
                var s = modifier.ToString();
                // Assert it has no unresolved references or values
                StringAssert.DoesNotContain("References", s);
                StringAssert.DoesNotContain("Values", s);
            }
        }

        public static void AssertCorrectModifiers(
            IValueCalculationContext context,
            (string stat, Form form, NodeValue? value, ModifierSource source)[] expectedModifiers,
            ParseResult result)
        {
            var (failedLines, remainingSubstrings, modifiers) = result;

            Assert.IsEmpty(failedLines);
            Assert.IsEmpty(remainingSubstrings);
            for (var i = 0; i < Math.Min(modifiers.Count, expectedModifiers.Length); i++)
            {
                var expected = expectedModifiers[i];
                var actual = modifiers[i];
                Assert.AreEqual(expected.stat, actual.Stats[0].Identity);
                Assert.AreEqual(Entity.Character, actual.Stats[0].Entity, expected.stat);
                Assert.AreEqual(expected.form, actual.Form, expected.stat);
                Assert.AreEqual(expected.source, actual.Source, expected.stat);

                var expectedValue = expected.value;
                var actualValue = actual.Value.Calculate(context);
                Assert.AreEqual(expectedValue, actualValue, expected.stat);
            }
            Assert.AreEqual(expectedModifiers.Length, modifiers.Count);
        }

        private static string CanonicalizeFailedStatLine(string statLine)
            => statLine.ToLowerInvariant().Replace("\r", "").Replace("\n", " ");

        public static void AssertIsParsedUnsuccessfully(ParseResult parseResult)
        {
            Assert.IsFalse(parseResult.SuccessfullyParsed, parseResult.RemainingSubstrings.FirstOrDefault());
        }

        public static readonly Lazy<IEnumerable<string>> NotParseableStatLines = new Lazy<IEnumerable<string>>(
            () => ReadNotParseableStatLines().Select(s => s.ToLowerInvariant()).ToHashSet());

        public static IEnumerable<string> ReadNotParseableStatLines()
            => ReadDataLines("NotParseableStatLines").Concat(ReadDataLines("NotYetParseableStatLines"));

        public static IEnumerable<string> ReadDataLines(string fileName)
            => File.ReadAllLines(AppContext.BaseDirectory + $"/Data/{fileName}.txt")
                .Where(s => !s.StartsWith("//", StringComparison.Ordinal))
                .Distinct();

        public static IReadOnlyList<string> Translate(
            IEnumerable<CraftableStat> craftableStats, IStatTranslator translator)
        {
            var untranslatedStats =
                craftableStats.Select(s => new UntranslatedStat(s.StatId, (s.MinValue + s.MaxValue) / 2));
            return translator.Translate(untranslatedStats).TranslatedStats;
        }
    }
}