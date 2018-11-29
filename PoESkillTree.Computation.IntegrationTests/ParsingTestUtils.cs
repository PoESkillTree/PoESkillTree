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
        public static void AssertIsParsedSuccessfully(ParseResult parseResult)
        {
            var (failedLines, remaining, result) = parseResult;
            
            Assert.IsEmpty(failedLines, "\"" + remaining.ToDelimitedString("\", \"") + "\"");
            Assert.IsEmpty(remaining);

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

        public static void AssertIsParsedUnsuccessfully(ParseResult parseResult)
        {
            Assert.IsFalse(parseResult.SuccessfullyParsed, parseResult.RemainingSubstrings[0]);
        }

        public static IEnumerable<string> ReadDataLines(string fileName)
            => File.ReadAllLines(TestContext.CurrentContext.TestDirectory + $"/Data/{fileName}.txt")
                .Where(s => !s.StartsWith("//", StringComparison.Ordinal))
                .Distinct();
    }
}