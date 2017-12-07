using System.Collections.Generic;
using System.IO;
using System.Linq;
using MoreLinq;
using NUnit.Framework;
using PoESkillTree.Computation.Console;

namespace PoESkillTree.Computation.IntegrationTests
{
    [TestFixture]
    public class ParsingTest
    {
        private static IParser<IReadOnlyList<Modifier>> _parser;

        [OneTimeSetUp]
        public static void ClassInit()
        {
            _parser = new CompositionRoot().CreateParser();
        }

        [Test, TestCaseSource(nameof(ReadParsableStatLines))]
        public void Parses(string statLine)
        {
            var r = _parser.TryParse(statLine, out var remaining, out var result);

            Assert.IsTrue(r, $"{remaining}\nResult:\n  {string.Join("\n  ", result)}");
            CollectionAssert.IsEmpty(remaining);
            foreach (var modifier in result)
            {
                Assert.NotNull(modifier);
                var s = modifier.ToString();
                // Assert it has no unresolved references or values
                StringAssert.DoesNotContain("References", s);
                StringAssert.DoesNotContain("Values", s);
            }
        }

        [Test, TestCaseSource(nameof(ReadUnparsableStatLines))]
        public void DoesNotParse(string statLine)
        {
            var r = _parser.TryParse(statLine, out var remaining, out var result);

            Assert.IsFalse(r, $"{remaining}\nResult:\n  {string.Join("\n  ", result)}");
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
            return File.ReadAllLines(TestContext.CurrentContext.TestDirectory + $"/TestData/{fileName}.txt")
                .Where(s => !s.StartsWith("//"))
                .Distinct()
                .Select(s => s.ToLowerInvariant());
        }
    }
}