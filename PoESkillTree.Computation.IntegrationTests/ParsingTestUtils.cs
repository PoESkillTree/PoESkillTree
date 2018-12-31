using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MoreLinq;
using NUnit.Framework;
using PoESkillTree.Computation.Parsing;

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
            => File.ReadAllLines(TestContext.CurrentContext.TestDirectory + $"/Data/{fileName}.txt")
                .Where(s => !s.StartsWith("//", StringComparison.Ordinal))
                .Distinct();
    }
}